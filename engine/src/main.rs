mod api;
mod models;
mod services;
mod utils;

use actix_cors::Cors;
use actix_web::{middleware, web, App, HttpServer};
use dotenv::dotenv;
use sea_orm::{ConnectOptions, Database, DatabaseConnection};
use std::env;
use std::time::Duration;
use utoipa::OpenApi;
use utoipa_swagger_ui::SwaggerUi;

use services::{
    auth::AuthService, 
    events::EventService, 
    metrics::MetricService, 
    projects::ProjectService, 
    users::UserService
};
use utils::middleware::{api_key::ApiKeyMiddleware, auth::JwtMiddleware};

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    // Load environment variables from .env file
    dotenv().ok();
    
    // Initialize logger
    env_logger::init_from_env(env_logger::Env::default().default_filter_or("info"));
    
    // Get database connection URL from environment
    let database_url = env::var("DATABASE_URL").expect("DATABASE_URL is not set in .env file");
    let host = env::var("HOST").unwrap_or_else(|_| "127.0.0.1".to_string());
    let port = env::var("PORT").unwrap_or_else(|_| "8080".to_string());
    let server_url = format!("{}:{}", host, port);
    
    // Configure database connection
    let mut opt = ConnectOptions::new(database_url);
    opt.max_connections(100)
        .min_connections(5)
        .connect_timeout(Duration::from_secs(8))
        .acquire_timeout(Duration::from_secs(8))
        .idle_timeout(Duration::from_secs(8))
        .max_lifetime(Duration::from_secs(8))
        .sqlx_logging(true);
    
    // Connect to database
    let db = Database::connect(opt)
        .await
        .expect("Failed to connect to database");
    
    // Generate OpenAPI documentation
    let openapi = api::ApiDoc::openapi();
    
    // Initialize services
    let auth_service = AuthService::new();
    let user_service = UserService::new();
    let project_service = ProjectService::new();
    let event_service = EventService::new();
    let metric_service = MetricService::new();
    
    // Start HTTP server
    println!("Starting server at http://{}", server_url);
    HttpServer::new(move || {
        // Configure CORS
        let cors = Cors::default()
            .allow_any_origin()
            .allow_any_method()
            .allow_any_header()
            .max_age(3600);
        
        App::new()
            // Enable logger
            .wrap(middleware::Logger::default())
            // Enable CORS
            .wrap(cors)
            // Add database connection to app data
            .app_data(web::Data::new(db.clone()))
            // Add services to app data
            .app_data(web::Data::new(auth_service.clone()))
            .app_data(web::Data::new(user_service.clone()))
            .app_data(web::Data::new(project_service.clone()))
            .app_data(web::Data::new(event_service.clone()))
            .app_data(web::Data::new(metric_service.clone()))
            // Register API routes
            .configure(api::configure_routes)
            // Serve Swagger UI
            .service(
                SwaggerUi::new("/swagger-ui/{_:.*}")
                    .url("/api-docs/openapi.json", openapi.clone())
            )
    })
    .bind(server_url)?
    .run()
    .await
}
