use sea_orm::entity::prelude::*;
use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, PartialEq, Eq, DeriveEntityModel, Serialize, Deserialize)]
#[sea_orm(table_name = "projects")]
pub struct Model {
    #[sea_orm(primary_key, auto_increment = false)]
    pub id: Uuid,
    pub name: String,
    #[sea_orm(unique)]
    pub api_key: String,
    pub owner_id: Uuid,
    pub created_at: chrono::DateTime<chrono::Utc>,
    pub updated_at: chrono::DateTime<chrono::Utc>,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {
    #[sea_orm(
        belongs_to = "super::users::Entity",
        from = "Column::OwnerId",
        to = "super::users::Column::Id"
    )]
    Owner,
    #[sea_orm(has_many = "super::project_users::Entity")]
    ProjectUsers,
    #[sea_orm(has_many = "super::devices::Entity")]
    Devices,
    #[sea_orm(has_many = "super::events::Entity")]
    Events,
    #[sea_orm(has_many = "super::metrics::Entity")]
    Metrics,
}

impl Related<super::users::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Owner.def()
    }
}

impl Related<super::project_users::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::ProjectUsers.def()
    }
}

impl Related<super::devices::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Devices.def()
    }
}

impl Related<super::events::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Events.def()
    }
}

impl Related<super::metrics::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Metrics.def()
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