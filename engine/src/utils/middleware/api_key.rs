use actix_web::{
    dev::{forward_ready, Service, ServiceRequest, ServiceResponse, Transform},
    Error, HttpMessage,
};
use futures::future::{ready, LocalBoxFuture, Ready};
use sea_orm::{ColumnTrait, DatabaseConnection, EntityTrait, QueryFilter};
use std::rc::Rc;

use entity::project;
use entity::project::Column as ProjectColumn;

use crate::utils::errors::{AppError, ErrorKind};

// API Key middleware struct
pub struct ApiKeyMiddleware;

// Implementation of the Transform trait which is required by Actix-web middleware
impl<S, B> Transform<S, ServiceRequest> for ApiKeyMiddleware
where
    S: Service<ServiceRequest, Response = ServiceResponse<B>, Error = Error> + 'static,
    S::Future: 'static,
    B: 'static,
{
    type Response = ServiceResponse<B>;
    type Error = Error;
    type Transform = ApiKeyMiddlewareService<S>;
    type InitError = ();
    type Future = Ready<Result<Self::Transform, Self::InitError>>;

    fn new_transform(&self, service: S) -> Self::Future {
        ready(Ok(ApiKeyMiddlewareService(Rc::new(service))))
    }
}

// Service struct that wraps the original service
pub struct ApiKeyMiddlewareService<S>(Rc<S>);

// Implementation of the Service trait which processes requests
impl<S, B> Service<ServiceRequest> for ApiKeyMiddlewareService<S>
where
    S: Service<ServiceRequest, Response = ServiceResponse<B>, Error = Error> + 'static,
    S::Future: 'static,
    B: 'static,
{
    type Response = ServiceResponse<B>;
    type Error = Error;
    type Future = LocalBoxFuture<'static, Result<Self::Response, Self::Error>>;

    // Required by the Service trait
    forward_ready!(S);

    // This method is called for each request
    fn call(&self, req: ServiceRequest) -> Self::Future {
        let srv = Rc::clone(&self.0);

        // Only apply this middleware to event endpoints
        if !req.path().starts_with("/api/v1/events") {
            return Box::pin(async move { srv.call(req).await });
        }

        // Get the API key from the X-API-Key header
        let api_key_header = match req.headers().get("X-API-Key") {
            Some(header) => header,
            None => {
                return Box::pin(async {
                    Err(AppError::new(
                        ErrorKind::Unauthorized,
                        "Missing API key".to_string(),
                    )
                    .into())
                });
            }
        };

        // Convert header to string
        let api_key = match api_key_header.to_str() {
            Ok(str) => str,
            Err(_) => {
                return Box::pin(async {
                    Err(AppError::new(
                        ErrorKind::Unauthorized,
                        "Invalid API key format".to_string(),
                    )
                    .into())
                });
            }
        };

        // Get database connection from app data
        let db = match req.app_data::<actix_web::web::Data<DatabaseConnection>>() {
            Some(db) => db.clone(),
            None => {
                return Box::pin(async {
                    Err(AppError::new(
                        ErrorKind::Internal,
                        "Database connection not available".to_string(),
                    )
                    .into())
                });
            }
        };

        // Clone values for async move
        let api_key = api_key.to_string();

        Box::pin(async move {
            // Look up project by API key
            let project = project::Entity::find()
                .filter(ProjectColumn::ApiKey.eq(api_key.clone()))
                .one(&*db)
                .await
                .map_err(|e| {
                    AppError::new(
                        ErrorKind::Internal,
                        format!("Database error: {}", e),
                    )
                })?;

            // Check if project exists
            let project = match project {
                Some(project) => project,
                None => {
                    return Err(AppError::new(
                        ErrorKind::Unauthorized,
                        "Invalid API key".to_string(),
                    )
                    .into());
                }
            };

            // Add project ID to request extensions
            req.extensions_mut().insert(project.id);

            // Continue with the request
            srv.call(req).await
        })
    }
} 