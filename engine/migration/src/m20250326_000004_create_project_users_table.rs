use sea_orm_migration::prelude::*;

#[derive(DeriveMigrationName)]
pub struct Migration;

#[async_trait::async_trait]
impl MigrationTrait for Migration {
    async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .create_table(
                Table::create()
                    .table(ProjectUsers::Table)
                    .if_not_exists()
                    .col(ColumnDef::new(ProjectUsers::ProjectId).uuid().not_null())
                    .col(ColumnDef::new(ProjectUsers::UserId).uuid().not_null())
                    .col(ColumnDef::new(ProjectUsers::Role).string().not_null())
                    .primary_key(
                        Index::create()
                            .col(ProjectUsers::ProjectId)
                            .col(ProjectUsers::UserId),
                    )
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_project_users_project_id")
                            .from(ProjectUsers::Table, ProjectUsers::ProjectId)
                            .to(Projects::Table, Projects::Id)
                            .on_delete(ForeignKeyAction::Cascade)
                            .on_update(ForeignKeyAction::Cascade),
                    )
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_project_users_user_id")
                            .from(ProjectUsers::Table, ProjectUsers::UserId)
                            .to(Users::Table, Users::Id)
                            .on_delete(ForeignKeyAction::Cascade)
                            .on_update(ForeignKeyAction::Cascade),
                    )
                    .to_owned(),
            )
            .await
    }

    async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .drop_table(Table::drop().table(ProjectUsers::Table).to_owned())
            .await
    }
}

/// Learn more at https://docs.rs/sea-query#iden
#[derive(Iden)]
enum ProjectUsers {
    Table,
    ProjectId,
    UserId,
    Role,
}

#[derive(Iden)]
enum Projects {
    Table,
    Id,
}

#[derive(Iden)]
enum Users {
    Table,
    Id,
} 