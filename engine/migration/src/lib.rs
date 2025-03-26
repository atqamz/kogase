pub use sea_orm_migration::prelude::*;

mod m20250326_000001_create_users_table;
mod m20250326_000002_create_auth_tokens_table;
mod m20250326_000003_create_projects_table;
mod m20250326_000004_create_project_users_table;
mod m20250326_000005_create_devices_table;
mod m20250326_000006_create_events_table;
mod m20250326_000007_create_metrics_table;

pub struct Migrator;

#[async_trait::async_trait]
impl MigratorTrait for Migrator {
    fn migrations() -> Vec<Box<dyn MigrationTrait>> {
        vec![
            Box::new(m20250326_000001_create_users_table::Migration),
            Box::new(m20250326_000002_create_auth_tokens_table::Migration),
            Box::new(m20250326_000003_create_projects_table::Migration),
            Box::new(m20250326_000004_create_project_users_table::Migration),
            Box::new(m20250326_000005_create_devices_table::Migration),
            Box::new(m20250326_000006_create_events_table::Migration),
            Box::new(m20250326_000007_create_metrics_table::Migration),
        ]
    }
} 