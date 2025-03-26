use crate::{
    models::projects::{AddUserToProjectRequest, CreateProjectRequest, ProjectApiKeyResponse, ProjectResponse, ProjectUserResponse, UpdateProjectRequest, UpdateUserRoleRequest},
    services::projects::ProjectService,
    utils::errors::{AppError, ErrorResponse},
};
use actix_web::{delete, get, post, put, web, HttpResponse};
use sea_orm::DatabaseConnection;
use utoipa::OpenApi;

/// Configure project routes
pub fn configure_routes(cfg: &mut web::ServiceConfig) {
    cfg.service(
        web::scope("/projects")
            .service(create_project)
            .service(get_projects)
            .service(get_project)
            .service(update_project)
            .service(delete_project)
            .service(regenerate_api_key)
            .service(add_user_to_project)
            .service(get_project_users)
            .service(update_project_user_role)
            .service(remove_user_from_project),
    );
}

/// Create a new project
#[utoipa::path(
    post,
    path = "/api/v1/projects",
    request_body = CreateProjectRequest,
    responses(
        (status = 201, description = "Project created successfully", body = ProjectResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[post("")]
async fn create_project(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    project_data: web::Json<CreateProjectRequest>,
    // The user ID would typically come from the JWT token in a real implementation
    // For now, we'll hardcode it for development
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let project = project_service
        .create_project(&db, project_data.into_inner(), &user_id)
        .await?;
    
    Ok(HttpResponse::Created().json(project))
}

/// Get all projects for the authenticated user
#[utoipa::path(
    get,
    path = "/api/v1/projects",
    responses(
        (status = 200, description = "List of projects", body = Vec<ProjectResponse>),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("")]
async fn get_projects(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let projects = project_service.get_projects_for_user(&db, &user_id).await?;
    Ok(HttpResponse::Ok().json(projects))
}

/// Get project by ID
#[utoipa::path(
    get,
    path = "/api/v1/projects/{id}",
    params(
        ("id" = String, Path, description = "Project ID")
    ),
    responses(
        (status = 200, description = "Project details", body = ProjectResponse),
        (status = 404, description = "Project not found", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("/{id}")]
async fn get_project(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let project = project_service
        .get_project_by_id(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(project))
}

/// Update project by ID
#[utoipa::path(
    put,
    path = "/api/v1/projects/{id}",
    params(
        ("id" = String, Path, description = "Project ID")
    ),
    request_body = UpdateProjectRequest,
    responses(
        (status = 200, description = "Project updated successfully", body = ProjectResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[put("/{id}")]
async fn update_project(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    id: web::Path<String>,
    project_data: web::Json<UpdateProjectRequest>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let project = project_service
        .update_project(&db, &id, project_data.into_inner(), &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(project))
}

/// Delete project by ID
#[utoipa::path(
    delete,
    path = "/api/v1/projects/{id}",
    params(
        ("id" = String, Path, description = "Project ID")
    ),
    responses(
        (status = 204, description = "Project deleted successfully"),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[delete("/{id}")]
async fn delete_project(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    project_service
        .delete_project(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::NoContent().finish())
}

/// Regenerate API key for a project
#[utoipa::path(
    post,
    path = "/api/v1/projects/{id}/apikey",
    params(
        ("id" = String, Path, description = "Project ID")
    ),
    responses(
        (status = 200, description = "API key regenerated successfully", body = ProjectApiKeyResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[post("/{id}/apikey")]
async fn regenerate_api_key(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let api_key = project_service
        .regenerate_api_key(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(ProjectApiKeyResponse { api_key }))
}

/// Add a user to a project
#[utoipa::path(
    post,
    path = "/api/v1/projects/{id}/users",
    params(
        ("id" = String, Path, description = "Project ID")
    ),
    request_body = AddUserToProjectRequest,
    responses(
        (status = 201, description = "User added to project successfully", body = ProjectUserResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project or user not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[post("/{id}/users")]
async fn add_user_to_project(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    id: web::Path<String>,
    user_data: web::Json<AddUserToProjectRequest>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let project_user = project_service
        .add_user_to_project(&db, &id, user_data.into_inner(), &user_id)
        .await?;
    
    Ok(HttpResponse::Created().json(project_user))
}

/// Get users for a project
#[utoipa::path(
    get,
    path = "/api/v1/projects/{id}/users",
    params(
        ("id" = String, Path, description = "Project ID")
    ),
    responses(
        (status = 200, description = "List of project users", body = Vec<ProjectUserResponse>),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("/{id}/users")]
async fn get_project_users(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let project_users = project_service
        .get_project_users(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(project_users))
}

/// Update a user's role in a project
#[utoipa::path(
    put,
    path = "/api/v1/projects/{id}/users/{user_id}",
    params(
        ("id" = String, Path, description = "Project ID"),
        ("user_id" = String, Path, description = "User ID")
    ),
    request_body = UpdateUserRoleRequest,
    responses(
        (status = 200, description = "User role updated successfully", body = ProjectUserResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project or user not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[put("/{id}/users/{target_user_id}")]
async fn update_project_user_role(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    path: web::Path<(String, String)>,
    role_data: web::Json<UpdateUserRoleRequest>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let (project_id, target_user_id) = path.into_inner();
    
    let project_user = project_service
        .update_user_role(&db, &project_id, &target_user_id, role_data.into_inner(), &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(project_user))
}

/// Remove a user from a project
#[utoipa::path(
    delete,
    path = "/api/v1/projects/{id}/users/{user_id}",
    params(
        ("id" = String, Path, description = "Project ID"),
        ("user_id" = String, Path, description = "User ID")
    ),
    responses(
        (status = 204, description = "User removed from project successfully"),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Project or user not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["projects"],
    security(
        ("jwt_auth" = [])
    )
)]
#[delete("/{id}/users/{target_user_id}")]
async fn remove_user_from_project(
    db: web::Data<DatabaseConnection>,
    project_service: web::Data<ProjectService>,
    path: web::Path<(String, String)>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let (project_id, target_user_id) = path.into_inner();
    
    project_service
        .remove_user_from_project(&db, &project_id, &target_user_id, &user_id)
        .await?;
    
    Ok(HttpResponse::NoContent().finish())
} 