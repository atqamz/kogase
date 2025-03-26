use sea_orm_migration::prelude::*;

#[derive(DeriveMigrationName)]
pub struct Migration;

#[async_trait::async_trait]
impl MigrationTrait for Migration {
    async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .create_table(
                Table::create()
                    .table(Devices::Table)
                    .if_not_exists()
                    .col(
                        ColumnDef::new(Devices::Id)
                            .uuid()
                            .not_null()
                            .primary_key(),
                    )
                    .col(ColumnDef::new(Devices::ProjectId).uuid().not_null())
                    .col(ColumnDef::new(Devices::DeviceId).string().not_null())
                    .col(ColumnDef::new(Devices::Platform).string().not_null())
                    .col(ColumnDef::new(Devices::OsVersion).string())
                    .col(ColumnDef::new(Devices::AppVersion).string())
                    .col(
                        ColumnDef::new(Devices::FirstSeen)
                            .timestamp_with_time_zone()
                            .not_null()
                            .default(Expr::current_timestamp()),
                    )
                    .col(
                        ColumnDef::new(Devices::LastSeen)
                            .timestamp_with_time_zone()
                            .not_null()
                            .default(Expr::current_timestamp()),
                    )
                    .col(ColumnDef::new(Devices::IpAddress).string())
                    .col(ColumnDef::new(Devices::Country).string())
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_devices_project_id")
                            .from(Devices::Table, Devices::ProjectId)
                            .to(Projects::Table, Projects::Id)
                            .on_delete(ForeignKeyAction::Cascade)
                            .on_update(ForeignKeyAction::Cascade),
                    )
                    .to_owned(),
            )
            .await?;

        // Add a unique constraint for project_id + device_id
        manager
            .create_index(
                Index::create()
                    .if_not_exists()
                    .name("idx_devices_project_device")
                    .table(Devices::Table)
                    .col(Devices::ProjectId)
                    .col(Devices::DeviceId)
                    .unique()
                    .to_owned(),
            )
            .await
    }

    async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .drop_table(Table::drop().table(Devices::Table).to_owned())
            .await
    }
}

/// Learn more at https://docs.rs/sea-query#iden
#[derive(Iden)]
enum Devices {
    Table,
    Id,
    ProjectId,
    DeviceId,
    Platform,
    OsVersion,
    AppVersion,
    FirstSeen,
    LastSeen,
    IpAddress,
    Country,
}

#[derive(Iden)]
enum Projects {
    Table,
    Id,
} 