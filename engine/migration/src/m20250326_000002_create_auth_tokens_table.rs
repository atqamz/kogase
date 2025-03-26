use sea_orm_migration::prelude::*;

#[derive(DeriveMigrationName)]
pub struct Migration;

#[async_trait::async_trait]
impl MigrationTrait for Migration {
    async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
        manager
            .create_table(
                Table::create()
                    .table(AuthTokens::Table)
                    .if_not_exists()
                    .col(
                        ColumnDef::new(AuthTokens::Id)
                            .uuid()
                            .not_null()
                            .primary_key(),
                    )
                    .col(ColumnDef::new(AuthTokens::UserId).uuid().not_null())
                    .col(ColumnDef::new(AuthTokens::Token).string().not_null())
                    .col(
                        ColumnDef::new(AuthTokens::ExpiresAt)
                            .timestamp_with_time_zone()
                            .not_null(),
                    )
                    .col(
                        ColumnDef::new(AuthTokens::LastUsedAt)
                            .timestamp_with_time_zone()
                            .not_null(),
                    )
                    .foreign_key(
                        ForeignKey::create()
                            .name("fk_auth_tokens_user_id")
                            .from(AuthTokens::Table, AuthTokens::UserId)
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
            .drop_table(Table::drop().table(AuthTokens::Table).to_owned())
            .await
    }
}

/// Learn more at https://docs.rs/sea-query#iden
#[derive(Iden)]
enum AuthTokens {
    Table,
    Id,
    UserId,
    Token,
    ExpiresAt,
    LastUsedAt,
}

#[derive(Iden)]
enum Users {
    Table,
    Id,
} 