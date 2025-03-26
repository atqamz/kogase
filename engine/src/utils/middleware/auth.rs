use actix_web::{
    dev::{forward_ready, Service, ServiceRequest, ServiceResponse, Transform},
    Error, HttpMessage,
};
use futures::future::{ready, LocalBoxFuture, Ready};
use jsonwebtoken::{decode, Algorithm, DecodingKey, Validation};
use std::env;
use std::future::Future;
use std::pin::Pin;
use std::rc::Rc;
use std::task::{Context, Poll};

use crate::models::auth::Claims;
use crate::utils::errors::{AppError, ErrorKind};

// JWT middleware struct
pub struct JwtMiddleware;

// Implementation of the Transform trait which is required by Actix-web middleware
impl<S, B> Transform<S, ServiceRequest> for JwtMiddleware
where
    S: Service<ServiceRequest, Response = ServiceResponse<B>, Error = Error> + 'static,
    S::Future: 'static,
    B: 'static,
{
    type Response = ServiceResponse<B>;
    type Error = Error;
    type Transform = JwtMiddlewareService<S>;
    type InitError = ();
    type Future = Ready<Result<Self::Transform, Self::InitError>>;

    fn new_transform(&self, service: S) -> Self::Future {
        ready(Ok(JwtMiddlewareService(Rc::new(service))))
    }
}

// Service struct that wraps the original service
pub struct JwtMiddlewareService<S>(Rc<S>);

// Implementation of the Service trait which processes requests
impl<S, B> Service<ServiceRequest> for JwtMiddlewareService<S>
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

        // Skip authentication for login route
        if req.path() == "/api/v1/auth/login" {
            return Box::pin(async move { srv.call(req).await });
        }

        // Get the JWT token from the Authorization header
        let auth_header = match req.headers().get("Authorization") {
            Some(header) => header,
            None => {
                return Box::pin(async {
                    Err(AppError::new(
                        ErrorKind::Unauthorized,
                        "Missing authorization header".to_string(),
                    )
                    .into())
                });
            }
        };

        // Convert header to string and extract the token part
        let auth_str = match auth_header.to_str() {
            Ok(str) => str,
            Err(_) => {
                return Box::pin(async {
                    Err(AppError::new(
                        ErrorKind::Unauthorized,
                        "Invalid authorization header".to_string(),
                    )
                    .into())
                });
            }
        };

        // Check if it's a Bearer token
        if !auth_str.starts_with("Bearer ") {
            return Box::pin(async {
                Err(AppError::new(
                    ErrorKind::Unauthorized,
                    "Invalid authorization scheme".to_string(),
                )
                .into())
            });
        }

        // Extract just the token part
        let token = auth_str.trim_start_matches("Bearer ").trim();

        // Get JWT secret from environment
        let jwt_secret = match env::var("JWT_SECRET") {
            Ok(secret) => secret,
            Err(_) => {
                return Box::pin(async {
                    Err(AppError::new(
                        ErrorKind::Internal,
                        "JWT_SECRET not set".to_string(),
                    )
                    .into())
                });
            }
        };

        // Decode JWT token
        let token_data = match decode::<Claims>(
            token,
            &DecodingKey::from_secret(jwt_secret.as_bytes()),
            &Validation::new(Algorithm::HS256),
        ) {
            Ok(data) => data,
            Err(e) => {
                return Box::pin(async move {
                    Err(AppError::new(
                        ErrorKind::Unauthorized,
                        format!("Invalid token: {}", e),
                    )
                    .into())
                });
            }
        };

        // Add claims to request extensions
        req.extensions_mut().insert(token_data.claims.user_id.clone());

        // If everything is valid, continue with the request
        Box::pin(async move { srv.call(req).await })
    }
} 