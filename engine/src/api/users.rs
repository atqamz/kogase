use crate::{
    models::users::{CreateUserRequest, UpdateUserRequest, UserResponse},
    services::users::UserService,
    utils::errors::{AppError, ErrorResponse},
};
use actix_web::{delete, get, post, put, web, HttpResponse};
use sea_orm::DatabaseConnection;
use utoipa::OpenApi;

/// Configure user routes
pub fn configure_routes(cfg: &mut web::ServiceConfig) {
    cfg.service(
        web::scope("/users")
            .service(create_user)
            .service(get_users)
            .service(get_user)
            .service(update_user)
            .service(delete_user),
    );
}

/// Create a new user
#[utoipa::path(
    post,
    path = "/api/v1/users",
    request_body = CreateUserRequest,
    responses(
        (status = 201, description = "User created successfully", body = UserResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 409, description = "User already exists", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["users"],
    security(
        ("jwt_auth" = [])
    )
)]
#[post("")]
async fn create_user(
    db: web::Data<DatabaseConnection>,
    user_service: web::Data<UserService>,
    user_data: web::Json<CreateUserRequest>,
) -> Result<HttpResponse, AppError> {
    let user = user_service.create_user(&db, user_data.into_inner()).await?;
    Ok(HttpResponse::Created().json(user))
}

/// Get all users
#[utoipa::path(
    get,
    path = "/api/v1/users",
    responses(
        (status = 200, description = "List of users", body = Vec<UserResponse>),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["users"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("")]
async fn get_users(
    db: web::Data<DatabaseConnection>,
    user_service: web::Data<UserService>,
) -> Result<HttpResponse, AppError> {
    let users = user_service.get_users(&db).await?;
    Ok(HttpResponse::Ok().json(users))
}

/// Get user by ID
#[utoipa::path(
    get,
    path = "/api/v1/users/{id}",
    params(
        ("id" = String, Path, description = "User ID")
    ),
    responses(
        (status = 200, description = "User details", body = UserResponse),
        (status = 404, description = "User not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["users"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("/{id}")]
async fn get_user(
    db: web::Data<DatabaseConnection>,
    user_service: web::Data<UserService>,
    id: web::Path<String>,
) -> Result<HttpResponse, AppError> {
    let user = user_service.get_user_by_id(&db, &id).await?;
    Ok(HttpResponse::Ok().json(user))
}

/// Update user by ID
#[utoipa::path(
    put,
    path = "/api/v1/users/{id}",
    params(
        ("id" = String, Path, description = "User ID")
    ),
    request_body = UpdateUserRequest,
    responses(
        (status = 200, description = "User updated successfully", body = UserResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 404, description = "User not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["users"],
    security(
        ("jwt_auth" = [])
    )
)]
#[put("/{id}")]
async fn update_user(
    db: web::Data<DatabaseConnection>,
    user_service: web::Data<UserService>,
    id: web::Path<String>,
    user_data: web::Json<UpdateUserRequest>,
) -> Result<HttpResponse, AppError> {
    let user = user_service.update_user(&db, &id, user_data.into_inner()).await?;
    Ok(HttpResponse::Ok().json(user))
}

/// Delete user by ID
#[utoipa::path(
    delete,
    path = "/api/v1/users/{id}",
    params(
        ("id" = String, Path, description = "User ID")
    ),
    responses(
        (status = 204, description = "User deleted successfully"),
        (status = 404, description = "User not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["users"],
    security(
        ("jwt_auth" = [])
    )
)]
#[delete("/{id}")]
async fn delete_user(
    db: web::Data<DatabaseConnection>,
    user_service: web::Data<UserService>,
    id: web::Path<String>,
) -> Result<HttpResponse, AppError> {
    user_service.delete_user(&db, &id).await?;
    Ok(HttpResponse::NoContent().finish())
} 