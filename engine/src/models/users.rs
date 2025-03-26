use entity::prelude::UsersModel;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct User {
    pub id: String,
    pub email: String,
    pub name: String,
    pub role: String,
}

impl From<UsersModel> for User {
    fn from(model: UsersModel) -> Self {
        Self {
            id: model.id.to_string(),
            email: model.email,
            name: model.name,
            role: model.role,
        }
    }
}

#[derive(Debug, Deserialize)]
pub struct CreateUserRequest {
    pub email: String,
    pub name: String,
    pub password: String,
    pub role: Option<String>,
}

#[derive(Debug, Deserialize)]
pub struct UpdateUserRequest {
    pub name: Option<String>,
    pub email: Option<String>,
    pub role: Option<String>,
}

#[derive(Debug, Deserialize)]
pub struct ChangePasswordRequest {
    pub current_password: String,
    pub new_password: String,
}

#[derive(Debug, Serialize)]
pub struct UserResponse {
    pub id: String,
    pub email: String,
    pub name: String,
    pub role: String,
    pub created_at: String,
    pub updated_at: String,
}

impl From<UsersModel> for UserResponse {
    fn from(model: UsersModel) -> Self {
        Self {
            id: model.id.to_string(),
            email: model.email,
            name: model.name,
            role: model.role,
            created_at: model.created_at.to_rfc3339(),
            updated_at: model.updated_at.to_rfc3339(),
        }
    }
} 