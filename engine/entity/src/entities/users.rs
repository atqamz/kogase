use sea_orm::entity::prelude::*;
use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, PartialEq, Eq, DeriveEntityModel, Serialize, Deserialize)]
#[sea_orm(table_name = "users")]
pub struct Model {
    #[sea_orm(primary_key, auto_increment = false)]
    pub id: Uuid,
    #[sea_orm(unique)]
    pub email: String,
    #[sea_orm(column_type = "Text")]
    pub password_hash: String,
    pub name: String,
    pub role: String,
    pub created_at: chrono::DateTime<chrono::Utc>,
    pub updated_at: chrono::DateTime<chrono::Utc>,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {
    #[sea_orm(has_many = "super::auth_tokens::Entity")]
    AuthTokens,
    #[sea_orm(has_many = "super::projects::Entity")]
    Projects,
    #[sea_orm(has_many = "super::project_users::Entity")]
    ProjectUsers,
}

impl Related<super::auth_tokens::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::AuthTokens.def()
    }
}

impl Related<super::projects::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Projects.def()
    }
}

impl Related<super::project_users::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::ProjectUsers.def()
    }
}

impl ActiveModelBehavior for ActiveModel {
    /// Custom behavior before insert
    fn before_save(mut self, insert: bool) -> Result<Self, DbErr> {
        if insert {
            self.id = Set(Uuid::new_v4());
            self.created_at = Set(chrono::Utc::now());
            self.updated_at = Set(chrono::Utc::now());
        } else {
            self.updated_at = Set(chrono::Utc::now());
        }
        Ok(self)
    }
} 