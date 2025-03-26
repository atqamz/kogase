use crate::models::users::User;
use serde::{Deserialize, Serialize};

#[derive(Debug, Deserialize)]
pub struct LoginRequest {
    pub email: String,
    pub password: String,
}

#[derive(Debug, Serialize)]
pub struct LoginResponse {
    pub token: String,
    pub user: User,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct ApiKeyAuth {
    pub project_id: String,
    pub api_key: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct Claims {
    pub sub: String,         // User ID
    pub role: String,        // User role
    pub exp: u64,            // Expiration time
    pub project_id: Option<String>, // Optional project ID for API key authentication
    pub is_api_key: bool,    // Flag to indicate if this is an API key token
} 