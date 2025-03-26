use actix_web::web;
use utoipa::OpenApi;

pub mod auth;
pub mod users;
pub mod projects;
pub mod events;
pub mod metrics;

/// API documentation schema
#[derive(OpenApi)]
#[openapi(
    paths(
        // Auth endpoints
        auth::login,

        // User endpoints
        users::create_user,
        users::get_users,
        users::get_user,
        users::update_user,
        users::delete_user,

        // Project endpoints
        projects::create_project,
        projects::get_projects,
        projects::get_project,
        projects::update_project,
        projects::delete_project,
        projects::regenerate_api_key,
        projects::add_user_to_project,
        projects::get_project_users,
        projects::update_project_user_role,
        projects::remove_user_from_project,

        // Event endpoints
        events::create_event,
        events::create_batch_events,
        events::get_events,
        events::get_event,

        // Metric endpoints
        metrics::create_metric_definition,
        metrics::get_metric_definitions,
        metrics::get_metric_definition,
        metrics::update_metric_definition,
        metrics::delete_metric_definition,
        metrics::get_metric_data,
    ),
    components(
        schemas(
            // Auth models
            crate::models::auth::LoginRequest,
            crate::models::auth::LoginResponse,
            crate::models::auth::Claims,

            // User models
            crate::models::users::User,
            crate::models::users::CreateUserRequest,
            crate::models::users::UpdateUserRequest,
            crate::models::users::ChangePasswordRequest,
            crate::models::users::UserResponse,

            // Project models
            crate::models::projects::Project,
            crate::models::projects::CreateProjectRequest,
            crate::models::projects::UpdateProjectRequest,
            crate::models::projects::ProjectResponse,
            crate::models::projects::ProjectUser,
            crate::models::projects::AddUserToProjectRequest,
            crate::models::projects::UpdateUserRoleRequest,
            crate::models::projects::ProjectUserResponse,
            crate::models::projects::ProjectApiKeyResponse,

            // Event models
            crate::models::events::EventData,
            crate::models::events::BatchEventsRequest,
            crate::models::events::EventResponse,

            // Metric models
            crate::models::metrics::MetricDefinition,
            crate::models::metrics::MetricDefinitionRequest,
            crate::models::metrics::MetricDefinitionResponse,
            crate::models::metrics::MetricResponse,

            // Error models
            crate::utils::errors::ErrorResponse,
        ),
        responses(
            crate::utils::errors::ErrorResponse
        ),
        security_schemes(
            ("jwt_auth" = ("bearer")),
            ("api_key" = ("apiKey", "header", "X-API-Key"))
        )
    ),
    tags(
        (name = "auth", description = "Authentication endpoints"),
        (name = "users", description = "User management endpoints"),
        (name = "projects", description = "Project management endpoints"),
        (name = "events", description = "Event telemetry endpoints"),
        (name = "metrics", description = "Metrics and analytics endpoints"),
    ),
    info(
        title = "Game Telemetry API",
        version = "1.0.0",
        description = "API for game telemetry, analytics, and metrics tracking"
    )
)]
pub struct ApiDoc;

/// Configure all API routes
pub fn configure_routes(cfg: &mut web::ServiceConfig) {
    cfg.service(
        web::scope("/api/v1")
            .configure(auth::configure_routes)
            .configure(users::configure_routes)
            .configure(projects::configure_routes)
            .configure(events::configure_routes)
            .configure(metrics::configure_routes)
    );
} 