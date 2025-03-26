use entity::prelude::ProjectsModel;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct Project {
    pub id: String,
    pub name: String,
    pub owner_id: String,
}

impl From<ProjectsModel> for Project {
    fn from(model: ProjectsModel) -> Self {
        Self {
            id: model.id.to_string(),
            name: model.name,
            owner_id: model.owner_id.to_string(),
        }
    }
}

#[derive(Debug, Deserialize)]
pub struct CreateProjectRequest {
    pub name: String,
}

#[derive(Debug, Deserialize)]
pub struct UpdateProjectRequest {
    pub name: Option<String>,
}

#[derive(Debug, Serialize)]
pub struct ProjectResponse {
    pub id: String,
    pub name: String,
    pub api_key: String,
    pub owner_id: String,
    pub created_at: String,
    pub updated_at: String,
}

impl From<ProjectsModel> for ProjectResponse {
    fn from(model: ProjectsModel) -> Self {
        Self {
            id: model.id.to_string(),
            name: model.name,
            api_key: model.api_key,
            owner_id: model.owner_id.to_string(),
            created_at: model.created_at.to_rfc3339(),
            updated_at: model.updated_at.to_rfc3339(),
        }
    }
}

#[derive(Debug, Serialize)]
pub struct ProjectApiKeyResponse {
    pub api_key: String,
}

#[derive(Debug, Deserialize)]
pub struct AddUserToProjectRequest {
    pub user_id: String,
    pub role: String,
}

#[derive(Debug, Deserialize)]
pub struct UpdateUserRoleRequest {
    pub role: String,
}

#[derive(Debug, Serialize)]
pub struct ProjectUserResponse {
    pub project_id: String,
    pub user_id: String,
    pub role: String,
    pub user_name: String,
    pub user_email: String,
} 