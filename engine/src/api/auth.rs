use crate::{
    models::auth::{LoginRequest, LoginResponse},
    models::users::User,
    services::auth::AuthService,
    utils::errors::{AppError, ErrorResponse},
};
use actix_web::{post, web, HttpResponse};
use sea_orm::DatabaseConnection;
use utoipa::OpenApi;

/// Configure auth routes
pub fn configure_routes(cfg: &mut web::ServiceConfig) {
    cfg.service(web::scope("/auth").service(login));
}

/// Login user with email and password
#[utoipa::path(
    post,
    path = "/api/v1/auth/login",
    request_body = LoginRequest,
    responses(
        (status = 200, description = "User logged in successfully", body = LoginResponse),
        (status = 401, description = "Invalid credentials", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["auth"]
)]
#[post("/login")]
async fn login(
    db: web::Data<DatabaseConnection>,
    auth_service: web::Data<AuthService>,
    login_data: web::Json<LoginRequest>,
) -> Result<HttpResponse, AppError> {
    let response = auth_service
        .login(&db, &login_data.email, &login_data.password)
        .await?;
    
    Ok(HttpResponse::Ok().json(response))
} 