package controllers

import (
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
	"github.com/kogase/backend/models"
	"gorm.io/gorm"
)

// ProjectController handles project-related endpoints
type ProjectController struct {
	DB *gorm.DB
}

// ProjectUser represents the join table between projects and users
type ProjectUser struct {
	ProjectID uuid.UUID `gorm:"type:uuid;primary_key"`
	UserID    uuid.UUID `gorm:"type:uuid;primary_key"`
	Role      string    `gorm:"not null;default:contributor"` // contributor or admin
}

// NewProjectController creates a new ProjectController instance
func NewProjectController(db *gorm.DB) *ProjectController {
	return &ProjectController{DB: db}
}

// CreateProjectRequest represents the create project payload
type CreateProjectRequest struct {
	Name string `json:"name" binding:"required"`
}

// GetProjects returns all projects accessible by the current user
// @Summary List projects
// @Description Get all projects accessible by the current user
// @Tags projects
// @Accept json
// @Produce json
// @Security BearerAuth
// @Success 200 {array} models.Project
// @Failure 401 {object} map[string]string
// @Router /projects [get]
func (pc *ProjectController) GetProjects(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	role, _ := c.Get("user_role")

	var projects []models.Project

	// Admin can see all projects, developer only sees their own
	if role == "admin" {
		if err := pc.DB.Find(&projects).Error; err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve projects"})
			return
		}
	} else {
		if err := pc.DB.Joins("JOIN project_users ON project_users.project_id = projects.id").
			Where("project_users.user_id = ? OR projects.owner_id = ?", userID, userID).
			Group("projects.id").
			Find(&projects).Error; err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve projects"})
			return
		}
	}

	c.JSON(http.StatusOK, projects)
}

// GetProject returns a specific project by ID
// @Summary Get project
// @Description Get a specific project by ID
// @Tags projects
// @Accept json
// @Produce json
// @Param id path string true "Project ID"
// @Security BearerAuth
// @Success 200 {object} models.Project
// @Failure 401 {object} map[string]string
// @Failure 404 {object} map[string]string
// @Router /projects/{id} [get]
func (pc *ProjectController) GetProject(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	role, _ := c.Get("user_role")
	id := c.Param("id")

	projectID, err := uuid.Parse(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid project ID"})
		return
	}

	var project models.Project

	// Get project
	if err := pc.DB.First(&project, "id = ?", projectID).Error; err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Project not found"})
		return
	}

	// Check if user has access to project
	if role != "admin" && project.OwnerID != userID.(uuid.UUID) {
		var projectUser models.ProjectUser
		if err := pc.DB.Where("project_id = ? AND user_id = ?", projectID, userID).First(&projectUser).Error; err != nil {
			c.JSON(http.StatusForbidden, gin.H{"error": "Access denied"})
			return
		}
	}

	c.JSON(http.StatusOK, project)
}

// CreateProject creates a new project
// @Summary Create project
// @Description Create a new project
// @Tags projects
// @Accept json
// @Produce json
// @Param project body CreateProjectRequest true "Project details"
// @Security BearerAuth
// @Success 201 {object} models.Project
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Router /projects [post]
func (pc *ProjectController) CreateProject(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	var createReq CreateProjectRequest
	if err := c.ShouldBindJSON(&createReq); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request"})
		return
	}

	// Create project
	project := models.Project{
		Name:    createReq.Name,
		OwnerID: userID.(uuid.UUID),
	}

	if err := pc.DB.Create(&project).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create project"})
		return
	}

	c.JSON(http.StatusCreated, project)
}

// UpdateProject updates a project
// @Summary Update project
// @Description Update a project's details
// @Tags projects
// @Accept json
// @Produce json
// @Param id path string true "Project ID"
// @Param project body CreateProjectRequest true "Project details"
// @Security BearerAuth
// @Success 200 {object} models.Project
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 403 {object} map[string]string
// @Failure 404 {object} map[string]string
// @Router /projects/{id} [put]
func (pc *ProjectController) UpdateProject(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	role, _ := c.Get("user_role")
	id := c.Param("id")

	projectID, err := uuid.Parse(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid project ID"})
		return
	}

	var project models.Project

	// Get project
	if err := pc.DB.First(&project, "id = ?", projectID).Error; err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Project not found"})
		return
	}

	// Check if user has access to project
	if role != "admin" && project.OwnerID != userID.(uuid.UUID) {
		c.JSON(http.StatusForbidden, gin.H{"error": "Access denied"})
		return
	}

	var updateReq CreateProjectRequest
	if err := c.ShouldBindJSON(&updateReq); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request"})
		return
	}

	// Update project
	project.Name = updateReq.Name
	if err := pc.DB.Save(&project).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to update project"})
		return
	}

	c.JSON(http.StatusOK, project)
}

// DeleteProject deletes a project
// @Summary Delete project
// @Description Delete a project
// @Tags projects
// @Accept json
// @Produce json
// @Param id path string true "Project ID"
// @Security BearerAuth
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 403 {object} map[string]string
// @Failure 404 {object} map[string]string
// @Router /projects/{id} [delete]
func (pc *ProjectController) DeleteProject(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	role, _ := c.Get("user_role")
	id := c.Param("id")

	projectID, err := uuid.Parse(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid project ID"})
		return
	}

	var project models.Project

	// Get project
	if err := pc.DB.First(&project, "id = ?", projectID).Error; err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Project not found"})
		return
	}

	// Check if user has access to project
	if role != "admin" && project.OwnerID != userID.(uuid.UUID) {
		c.JSON(http.StatusForbidden, gin.H{"error": "Access denied"})
		return
	}

	// Delete project
	if err := pc.DB.Delete(&project).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to delete project"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Project deleted successfully"})
}

// GetAPIKey returns the API key for a project
// @Summary Get API key
// @Description Get the API key for a project
// @Tags projects
// @Accept json
// @Produce json
// @Param id path string true "Project ID"
// @Security BearerAuth
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 403 {object} map[string]string
// @Failure 404 {object} map[string]string
// @Router /projects/{id}/apikey [get]
func (pc *ProjectController) GetAPIKey(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	role, _ := c.Get("user_role")
	id := c.Param("id")

	projectID, err := uuid.Parse(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid project ID"})
		return
	}

	var project models.Project

	// Get project
	if err := pc.DB.First(&project, "id = ?", projectID).Error; err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Project not found"})
		return
	}

	// Check if user has access to project
	if role != "admin" && project.OwnerID != userID.(uuid.UUID) {
		var projectUser models.ProjectUser
		if err := pc.DB.Where("project_id = ? AND user_id = ?", projectID, userID).First(&projectUser).Error; err != nil {
			c.JSON(http.StatusForbidden, gin.H{"error": "Access denied"})
			return
		}
	}

	c.JSON(http.StatusOK, gin.H{"api_key": project.ApiKey})
}

// RegenerateAPIKey regenerates the API key for a project
// @Summary Regenerate API key
// @Description Regenerate the API key for a project
// @Tags projects
// @Accept json
// @Produce json
// @Param id path string true "Project ID"
// @Security BearerAuth
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 403 {object} map[string]string
// @Failure 404 {object} map[string]string
// @Router /projects/{id}/apikey [post]
func (pc *ProjectController) RegenerateAPIKey(c *gin.Context) {
	userID, exists := c.Get("user_id")
	if !exists {
		c.JSON(http.StatusUnauthorized, gin.H{"error": "User not found"})
		return
	}

	role, _ := c.Get("user_role")
	id := c.Param("id")

	projectID, err := uuid.Parse(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid project ID"})
		return
	}

	var project models.Project

	// Get project
	if err := pc.DB.First(&project, "id = ?", projectID).Error; err != nil {
		c.JSON(http.StatusNotFound, gin.H{"error": "Project not found"})
		return
	}

	// Check if user has access to project
	if role != "admin" && project.OwnerID != userID.(uuid.UUID) {
		c.JSON(http.StatusForbidden, gin.H{"error": "Access denied"})
		return
	}

	// Generate new API key
	project.ApiKey = uuid.New().String()

	// Save project
	if err := pc.DB.Save(&project).Error; err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to regenerate API key"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"api_key": project.ApiKey})
}
