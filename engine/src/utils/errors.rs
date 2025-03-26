use actix_web::{HttpResponse, ResponseError};
use sea_orm::DbErr;
use serde::{Deserialize, Serialize};
use std::fmt;
use thiserror::Error;

#[derive(Debug, Error)]
pub enum AppError {
    #[error("Authentication error: {0}")]
    AuthError(String),
    
    #[error("Authorization error: {0}")]
    ForbiddenError(String),
    
    #[error("Not found: {0}")]
    NotFoundError(String),
    
    #[error("Validation error: {0}")]
    ValidationError(String),
    
    #[error("Database error: {0}")]
    DatabaseError(#[from] DbErr),
    
    #[error("Internal server error: {0}")]
    InternalServerError(String),
}

#[derive(Debug, Serialize, Deserialize)]
pub struct ErrorResponse {
    pub error: String,
    pub message: String,
}

impl ResponseError for AppError {
    fn error_response(&self) -> HttpResponse {
        match self {
            AppError::AuthError(message) => {
                HttpResponse::Unauthorized().json(ErrorResponse {
                    error: "UNAUTHORIZED".to_string(),
                    message: message.to_string(),
                })
            }
            AppError::ForbiddenError(message) => {
                HttpResponse::Forbidden().json(ErrorResponse {
                    error: "FORBIDDEN".to_string(),
                    message: message.to_string(),
                })
            }
            AppError::NotFoundError(message) => {
                HttpResponse::NotFound().json(ErrorResponse {
                    error: "NOT_FOUND".to_string(),
                    message: message.to_string(),
                })
            }
            AppError::ValidationError(message) => {
                HttpResponse::BadRequest().json(ErrorResponse {
                    error: "VALIDATION_ERROR".to_string(),
                    message: message.to_string(),
                })
            }
            AppError::DatabaseError(err) => {
                log::error!("Database error: {:?}", err);
                HttpResponse::InternalServerError().json(ErrorResponse {
                    error: "DATABASE_ERROR".to_string(),
                    message: "A database error occurred".to_string(),
                })
            }
            AppError::InternalServerError(message) => {
                log::error!("Internal server error: {}", message);
                HttpResponse::InternalServerError().json(ErrorResponse {
                    error: "INTERNAL_SERVER_ERROR".to_string(),
                    message: "An internal server error occurred".to_string(),
                })
            }
        }
    }
} 