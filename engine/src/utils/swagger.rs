use crate::models::{
    auth::{LoginRequest, LoginResponse},
    events::{BatchEventsRequest, EventResponse, InstallationRequest, QueryEventsResponse, SessionEndRequest, SessionStartRequest},
    metrics::{DashboardMetrics, MetricTimeseries, QueryMetricsResponse, RetentionMetrics},
    projects::{CreateProjectRequest, ProjectApiKeyResponse, ProjectResponse, ProjectUserResponse, UpdateProjectRequest},
    users::{CreateUserRequest, UpdateUserRequest, UserResponse},
};
use crate::utils::errors::ErrorResponse;
use utoipa::{OpenApi, ToSchema};

#[derive(OpenApi)]
#[openapi(
    paths(
        // Auth
        crate::api::auth::login,
        // Users
        crate::api::users::create_user,
        crate::api::users::get_users,
        crate::api::users::get_user,
        crate::api::users::update_user,
        crate::api::users::delete_user,
        // Projects
        crate::api::projects::create_project,
        crate::api::projects::get_projects,
        crate::api::projects::get_project,
        crate::api::projects::update_project,
        crate::api::projects::delete_project,
        crate::api::projects::regenerate_api_key,
        crate::api::projects::add_user_to_project,
        crate::api::projects::get_project_users,
        crate::api::projects::update_project_user_role,
        crate::api::projects::remove_user_from_project,
        // Telemetry
        crate::api::telemetry::batch_events,
        crate::api::telemetry::start_session,
        crate::api::telemetry::end_session,
        crate::api::telemetry::track_installation,
        // Analytics
        crate::api::analytics::get_events,
        crate::api::analytics::get_metrics,
        crate::api::analytics::get_dashboard,
        crate::api::analytics::get_retention,
    ),
    components(
        schemas(
            // Auth
            LoginRequest,
            LoginResponse,
            // Users
            CreateUserRequest,
            UpdateUserRequest,
            UserResponse,
            // Projects
            CreateProjectRequest,
            UpdateProjectRequest,
            ProjectResponse,
            ProjectApiKeyResponse,
            ProjectUserResponse,
            // Events
            BatchEventsRequest,
            EventResponse,
            SessionStartRequest,
            SessionEndRequest,
            InstallationRequest,
            QueryEventsResponse,
            // Metrics
            MetricTimeseries,
            DashboardMetrics,
            QueryMetricsResponse,
            RetentionMetrics,
            // Errors
            ErrorResponse,
        )
    ),
    tags(
        (name = "auth", description = "Authentication endpoints"),
        (name = "users", description = "User management endpoints"),
        (name = "projects", description = "Project management endpoints"),
        (name = "telemetry", description = "Telemetry collection endpoints"),
        (name = "analytics", description = "Analytics and reporting endpoints"),
    ),
    info(
        title = "Kogase API",
        version = "1.0.0",
        description = "Kogase backend API for game telemetry",
        license(
            name = "MIT",
            url = "https://opensource.org/licenses/MIT"
        ),
        contact(
            name = "Kogase Team",
            email = "info@kogase.dev",
            url = "https://kogase.dev"
        )
    ),
    external_docs(url = "https://github.com/kogase/kogase", description = "GitHub Repository")
)]
pub struct ApiDoc;

pub fn get_swagger_config() -> ApiDoc {
    ApiDoc {}
} 