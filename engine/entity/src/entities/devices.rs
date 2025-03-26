use sea_orm::entity::prelude::*;
use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, PartialEq, Eq, DeriveEntityModel, Serialize, Deserialize)]
#[sea_orm(table_name = "devices")]
pub struct Model {
    #[sea_orm(primary_key, auto_increment = false)]
    pub id: Uuid,
    pub project_id: Uuid,
    pub device_id: String,
    pub platform: String,
    pub os_version: Option<String>,
    pub app_version: Option<String>,
    pub first_seen: chrono::DateTime<chrono::Utc>,
    pub last_seen: chrono::DateTime<chrono::Utc>,
    pub ip_address: Option<String>,
    pub country: Option<String>,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {
    #[sea_orm(
        belongs_to = "super::projects::Entity",
        from = "Column::ProjectId",
        to = "super::projects::Column::Id"
    )]
    Project,
    #[sea_orm(has_many = "super::events::Entity")]
    Events,
}

impl Related<super::projects::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Project.def()
    }
}

impl Related<super::events::Entity> for Entity {
    fn to() -> RelationDef {
        Relation::Events.def()
    }
}

impl ActiveModelBehavior for ActiveModel {
    /// Custom behavior before insert
    fn before_save(mut self, insert: bool) -> Result<Self, DbErr> {
        if insert {
            self.id = Set(Uuid::new_v4());
            self.first_seen = Set(chrono::Utc::now());
            self.last_seen = Set(chrono::Utc::now());
        } else {
            self.last_seen = Set(chrono::Utc::now());
        }
        Ok(self)
    }
} 