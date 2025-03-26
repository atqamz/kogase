use sea_orm_migration::prelude::*;

#[derive(DeriveMigrationName)]
pub struct Migration;

#[async_trait::async_trait]
impl MigrationTrait for Migration {
    async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .create_table(
                Table::create()
                    .table(Events::Table)
                    .if_not_exists()
                    .col(
                        ColumnDef::new(Events::Id)
                            .uuid()
                            .not_null()
                            .primary_key(),
                    )
                    .col(ColumnDef::new(Events::ProjectId).uuid().not_null())
                    .col(ColumnDef::new(Events::DeviceId).uuid().not_null())
                    .col(ColumnDef::new(Events::EventType).string().not_null())
                    .col(ColumnDef::new(Events::EventName).string())
                    .col(ColumnDef::new(Events::Parameters).json_binary())
                    .col(
                        ColumnDef::new(Events::Timestamp)
                            .timestamp_with_time_zone()
                            .not_null(),
                    )
                    .col(
                        ColumnDef::new(Events::ReceivedAt)
                            .timestamp_with_time_zone()
                            .not_null()
                            .default(Expr::current_timestamp()),
                    )
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_events_project_id")
                            .from(Events::Table, Events::ProjectId)
                            .to(Projects::Table, Projects::Id)
                            .on_delete(ForeignKeyAction::Cascade)
                            .on_update(ForeignKeyAction::Cascade),
                    )
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_events_device_id")
                            .from(Events::Table, Events::DeviceId)
                            .to(Devices::Table, Devices::Id)
                            .on_delete(ForeignKeyAction::Cascade)
                            .on_update(ForeignKeyAction::Cascade),
                    )
                    .to_owned(),
            )
            .await?;

        // Create index on event type and timestamp for faster queries
        manager
            .create_index(
                Index::create()
                    .if_not_exists()
                    .name("idx_events_type_timestamp")
                    .table(Events::Table)
                    .col(Events::EventType)
                    .col(Events::Timestamp)
                    .to_owned(),
            )
            .await?;

        // Create index on project_id and timestamp for faster queries
        manager
            .create_index(
                Index::create()
                    .if_not_exists()
                    .name("idx_events_project_timestamp")
                    .table(Events::Table)
                    .col(Events::ProjectId)
                    .col(Events::Timestamp)
                    .to_owned(),
            )
            .await
    }

    async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .drop_table(Table::drop().table(Events::Table).to_owned())
            .await
    }
}

/// Learn more at https://docs.rs/sea-query#iden
#[derive(Iden)]
enum Events {
    Table,
    Id,
    ProjectId,
    DeviceId,
    EventType,
    EventName,
    Parameters,
    Timestamp,
    ReceivedAt,
}

#[derive(Iden)]
enum Projects {
    Table,
    Id,
}

#[derive(Iden)]
enum Devices {
    Table,
    Id,
} 