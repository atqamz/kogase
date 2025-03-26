use crate::{
    models::metrics::{MetricDefinitionRequest, MetricDefinitionResponse, MetricResponse},
    services::metrics::MetricService,
    utils::errors::{AppError, ErrorResponse},
};
use actix_web::{delete, get, post, put, web, HttpResponse};
use sea_orm::DatabaseConnection;
use utoipa::OpenApi;

/// Configure metrics routes
pub fn configure_routes(cfg: &mut web::ServiceConfig) {
    cfg.service(
        web::scope("/metrics")
            .service(create_metric_definition)
            .service(get_metric_definitions)
            .service(get_metric_definition)
            .service(update_metric_definition)
            .service(delete_metric_definition)
            .service(get_metric_data),
    );
}

/// Create a new metric definition
#[utoipa::path(
    post,
    path = "/api/v1/metrics",
    request_body = MetricDefinitionRequest,
    responses(
        (status = 201, description = "Metric definition created successfully", body = MetricDefinitionResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["metrics"],
    security(
        ("jwt_auth" = [])
    )
)]
#[post("")]
async fn create_metric_definition(
    db: web::Data<DatabaseConnection>,
    metric_service: web::Data<MetricService>,
    definition_data: web::Json<MetricDefinitionRequest>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let metric_definition = metric_service
        .create_metric_definition(&db, definition_data.into_inner(), &user_id)
        .await?;
    
    Ok(HttpResponse::Created().json(metric_definition))
}

/// Get all metric definitions for a project
#[utoipa::path(
    get,
    path = "/api/v1/metrics",
    params(
        ("project_id" = String, Query, description = "Project ID to filter metric definitions"),
    ),
    responses(
        (status = 200, description = "List of metric definitions", body = Vec<MetricDefinitionResponse>),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["metrics"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("")]
async fn get_metric_definitions(
    db: web::Data<DatabaseConnection>,
    metric_service: web::Data<MetricService>,
    query: web::Query<GetMetricsQuery>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let definitions = metric_service
        .get_metric_definitions(&db, &query.project_id, &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(definitions))
}

/// Get a specific metric definition by ID
#[utoipa::path(
    get,
    path = "/api/v1/metrics/{id}",
    params(
        ("id" = String, Path, description = "Metric definition ID")
    ),
    responses(
        (status = 200, description = "Metric definition details", body = MetricDefinitionResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Metric definition not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["metrics"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("/{id}")]
async fn get_metric_definition(
    db: web::Data<DatabaseConnection>,
    metric_service: web::Data<MetricService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let definition = metric_service
        .get_metric_definition_by_id(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(definition))
}

/// Update a metric definition
#[utoipa::path(
    put,
    path = "/api/v1/metrics/{id}",
    params(
        ("id" = String, Path, description = "Metric definition ID")
    ),
    request_body = MetricDefinitionRequest,
    responses(
        (status = 200, description = "Metric definition updated successfully", body = MetricDefinitionResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Metric definition not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["metrics"],
    security(
        ("jwt_auth" = [])
    )
)]
#[put("/{id}")]
async fn update_metric_definition(
    db: web::Data<DatabaseConnection>,
    metric_service: web::Data<MetricService>,
    id: web::Path<String>,
    definition_data: web::Json<MetricDefinitionRequest>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let definition = metric_service
        .update_metric_definition(&db, &id, definition_data.into_inner(), &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(definition))
}

/// Delete a metric definition
#[utoipa::path(
    delete,
    path = "/api/v1/metrics/{id}",
    params(
        ("id" = String, Path, description = "Metric definition ID")
    ),
    responses(
        (status = 204, description = "Metric definition deleted successfully"),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Metric definition not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["metrics"],
    security(
        ("jwt_auth" = [])
    )
)]
#[delete("/{id}")]
async fn delete_metric_definition(
    db: web::Data<DatabaseConnection>,
    metric_service: web::Data<MetricService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    metric_service
        .delete_metric_definition(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::NoContent().finish())
}

/// Get metric data by definition ID
#[utoipa::path(
    get,
    path = "/api/v1/metrics/{id}/data",
    params(
        ("id" = String, Path, description = "Metric definition ID"),
        ("start_date" = Option<String>, Query, description = "Start date for metric data (ISO format)"),
        ("end_date" = Option<String>, Query, description = "End date for metric data (ISO format)"),
        ("interval" = Option<String>, Query, description = "Interval for data aggregation (hour, day, week, month)"),
    ),
    responses(
        (status = 200, description = "Metric data", body = Vec<MetricResponse>),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Metric definition not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["metrics"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("/{id}/data")]
async fn get_metric_data(
    db: web::Data<DatabaseConnection>,
    metric_service: web::Data<MetricService>,
    id: web::Path<String>,
    query: web::Query<GetMetricDataQuery>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let data = metric_service
        .get_metric_data(
            &db,
            &id,
            query.start_date.as_deref(),
            query.end_date.as_deref(),
            query.interval.as_deref().unwrap_or("day"),
            &user_id,
        )
        .await?;
    
    Ok(HttpResponse::Ok().json(data))
}

#[derive(serde::Deserialize)]
pub struct GetMetricsQuery {
    pub project_id: String,
}

#[derive(serde::Deserialize)]
pub struct GetMetricDataQuery {
    pub start_date: Option<String>,
    pub end_date: Option<String>,
    pub interval: Option<String>,
} 