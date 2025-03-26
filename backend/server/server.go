package server

import (
	"fmt"
	"log"
	"os"
	"strconv"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/kogase/backend/controllers"
	"github.com/kogase/backend/middleware"
	"github.com/kogase/backend/models"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

// Server represents the main server application
type Server struct {
	Router *gin.Engine
	DB     *gorm.DB
}

// New creates a new server instance
func New() (*Server, error) {
	dbHost := getEnv("DB_HOST", "localhost")
	dbPort := getEnv("DB_PORT", "5432")
	dbUser := getEnv("DB_USER", "postgres")
	dbPassword := getEnv("DB_PASSWORD", "postgres")
	dbName := getEnv("DB_NAME", "kogase")
	dbSSLMode := getEnv("DB_SSLMODE", "disable")

	// Database connection
	dsn := fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=%s",
		dbHost, dbPort, dbUser, dbPassword, dbName, dbSSLMode)

	// Configure GORM logger
	newLogger := logger.New(
		log.New(os.Stdout, "\r\n", log.LstdFlags),
		logger.Config{
			SlowThreshold:             time.Second,
			LogLevel:                  logger.Info,
			IgnoreRecordNotFoundError: true,
			Colorful:                  true,
		},
	)

	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{
		Logger: newLogger,
	})
	if err != nil {
		return nil, fmt.Errorf("failed to connect to database: %w", err)
	}

	// Migrate the schema
	if err := models.MigrateDB(db); err != nil {
		return nil, fmt.Errorf("failed to migrate database: %w", err)
	}

	// Set up Gin
	r := gin.Default()

	// Create a new server
	s := &Server{
		Router: r,
		DB:     db,
	}

	// Initialize routes
	s.setupRoutes()

	return s, nil
}

// setupRoutes sets up all the routes
func (s *Server) setupRoutes() {
	// Global middleware
	s.Router.Use(middleware.CORSMiddleware())

	// Create controllers
	authController := controllers.NewAuthController(s.DB)
	projectController := controllers.NewProjectController(s.DB)
	telemetryController := controllers.NewTelemetryController(s.DB)
	analyticsController := controllers.NewAnalyticsController(s.DB)

	// API v1 routes
	v1 := s.Router.Group("/api/v1")

	// Auth routes (no auth required)
	auth := v1.Group("/auth")
	{
		auth.POST("/login", authController.Login)
		auth.POST("/register", authController.Register)
	}

	// SDK routes (API key required)
	sdk := v1.Group("/sdk")
	sdk.Use(middleware.APIKeyMiddleware(s.DB))
	{
		sdk.POST("/event", telemetryController.RecordEvent)
		sdk.POST("/events", telemetryController.RecordEvents)
		sdk.POST("/session/start", telemetryController.StartSession)
		sdk.POST("/session/end", telemetryController.EndSession)
		sdk.POST("/installation", telemetryController.RecordInstallation)
	}

	// Dashboard routes (JWT auth required)
	dashboard := v1.Group("/dashboard")
	dashboard.Use(middleware.AuthMiddleware(s.DB))
	{
		// User routes
		dashboard.GET("/user/me", authController.Me)
		dashboard.POST("/user/logout", authController.Logout)

		// Project routes
		projects := dashboard.Group("/projects")
		{
			projects.GET("", projectController.GetProjects)
			projects.POST("", projectController.CreateProject)
			projects.GET("/:id", projectController.GetProject)
			projects.PUT("/:id", projectController.UpdateProject)
			projects.DELETE("/:id", projectController.DeleteProject)
			projects.GET("/:id/api-key", projectController.GetAPIKey)
			projects.POST("/:id/api-key/regenerate", projectController.RegenerateAPIKey)
		}

		// Analytics routes
		analytics := dashboard.Group("/analytics")
		{
			analytics.GET("/metrics", analyticsController.GetMetrics)
			analytics.GET("/events", analyticsController.GetEvents)
			analytics.GET("/devices", analyticsController.GetDevices)
		}
	}

	// Admin routes (admin role required)
	admin := v1.Group("/admin")
	admin.Use(middleware.AuthMiddleware(s.DB), middleware.AdminMiddleware())
	{
		// TODO: Add admin routes as needed
	}
}

// Run starts the server
func (s *Server) Run() error {
	port := getEnv("PORT", "8080")
	return s.Router.Run(":" + port)
}

// getEnv gets an environment variable or returns a default value
func getEnv(key, defaultValue string) string {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}
	return value
}

// getEnvAsInt gets an environment variable as an integer or returns a default value
func getEnvAsInt(key string, defaultValue int) int {
	valueStr := getEnv(key, "")
	if value, err := strconv.Atoi(valueStr); err == nil {
		return value
	}
	return defaultValue
}

// getEnvAsBool gets an environment variable as a boolean or returns a default value
func getEnvAsBool(key string, defaultValue bool) bool {
	valueStr := getEnv(key, "")
	if value, err := strconv.ParseBool(valueStr); err == nil {
		return value
	}
	return defaultValue
}
