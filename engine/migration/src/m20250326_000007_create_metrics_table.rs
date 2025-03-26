use sea_orm_migration::prelude::*;

#[derive(DeriveMigrationName)]
pub struct Migration;

#[async_trait::async_trait]
impl MigrationTrait for Migration {
    async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .create_table(
                Table::create()
                    .table(Metrics::Table)
                    .if_not_exists()
                    .col(
                        ColumnDef::new(Metrics::Id)
                            .uuid()
                            .not_null()
                            .primary_key(),
                    )
                    .col(ColumnDef::new(Metrics::ProjectId).uuid().not_null())
                    .col(ColumnDef::new(Metrics::MetricType).string().not_null())
                    .col(ColumnDef::new(Metrics::Period).string().not_null())
                    .col(
                        ColumnDef::new(Metrics::PeriodStart)
                            .timestamp_with_time_zone()
                            .not_null(),
                    )
                    .col(ColumnDef::new(Metrics::Value).double().not_null())
                    .col(ColumnDef::new(Metrics::Dimensions).json_binary())
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_metrics_project_id")
                            .from(Metrics::Table, Metrics::ProjectId)
                            .to(Projects::Table, Projects::Id)
                            .on_delete(ForeignKeyAction::Cascade)
                            .on_update(ForeignKeyAction::Cascade),
                    )
                    .to_owned(),
            )
            .await?;

        // Create a unique index on project_id, metric_type, period, period_start, and dimensions
        manager
            .create_index(
                Index::create()
                    .if_not_exists()
                    .name("idx_metrics_unique")
                    .table(Metrics::Table)
                    .col(Metrics::ProjectId)
                    .col(Metrics::MetricType)
                    .col(Metrics::Period)
                    .col(Metrics::PeriodStart)
                    .unique()
                    .to_owned(),
            )
            .await?;

        // Create an index on project_id and period_start for faster queries
        manager
            .create_index(
                Index::create()
                    .if_not_exists()
                    .name("idx_metrics_project_period")
                    .table(Metrics::Table)
                    .col(Metrics::ProjectId)
                    .col(Metrics::PeriodStart)
                    .to_owned(),
            )
            .await
    }

    async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .drop_table(Table::drop().table(Metrics::Table).to_owned())
            .await
    }
}

/// Learn more at https://docs.rs/sea-query#iden
#[derive(Iden)]
enum Metrics {
    Table,
    Id,
    ProjectId,
    MetricType,
    Period,
    PeriodStart,
    Value,
    Dimensions,
}

#[derive(Iden)]
enum Projects {
    Table,
    Id,
} 