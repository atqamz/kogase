use crate::{
    models::events::{BatchEventsRequest, EventResponse},
    services::events::EventService,
    utils::errors::{AppError, ErrorResponse},
};
use actix_web::{get, post, web, HttpResponse};
use sea_orm::DatabaseConnection;
use utoipa::OpenApi;

/// Configure event routes
pub fn configure_routes(cfg: &mut web::ServiceConfig) {
    cfg.service(
        web::scope("/events")
            .service(create_event)
            .service(create_batch_events)
            .service(get_events)
            .service(get_event),
    );
}

/// Create a single telemetry event
#[utoipa::path(
    post,
    path = "/api/v1/events",
    request_body = EventData,
    responses(
        (status = 201, description = "Event created successfully", body = EventResponse),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["events"],
    security(
        ("api_key" = [])
    )
)]
#[post("")]
async fn create_event(
    db: web::Data<DatabaseConnection>,
    event_service: web::Data<EventService>,
    event_data: web::Json<serde_json::Value>,
    project_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let event = event_service
        .create_event(&db, event_data.into_inner(), &project_id)
        .await?;
    
    Ok(HttpResponse::Created().json(event))
}

/// Create multiple telemetry events in batch
#[utoipa::path(
    post,
    path = "/api/v1/events/batch",
    request_body = BatchEventsRequest,
    responses(
        (status = 201, description = "Events created successfully", body = Vec<EventResponse>),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["events"],
    security(
        ("api_key" = [])
    )
)]
#[post("/batch")]
async fn create_batch_events(
    db: web::Data<DatabaseConnection>,
    event_service: web::Data<EventService>,
    batch_data: web::Json<BatchEventsRequest>,
    project_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let events = event_service
        .create_batch_events(&db, batch_data.into_inner(), &project_id)
        .await?;
    
    Ok(HttpResponse::Created().json(events))
}

/// Get events by project
#[utoipa::path(
    get,
    path = "/api/v1/events",
    params(
        ("project_id" = String, Query, description = "Project ID to filter events"),
        ("event_type" = Option<String>, Query, description = "Event type to filter"),
        ("start_date" = Option<String>, Query, description = "Start date for events (ISO format)"),
        ("end_date" = Option<String>, Query, description = "End date for events (ISO format)"),
        ("page" = Option<i64>, Query, description = "Page number for pagination"),
        ("limit" = Option<i64>, Query, description = "Limit per page for pagination"),
    ),
    responses(
        (status = 200, description = "List of events", body = Vec<EventResponse>),
        (status = 400, description = "Validation error", body = ErrorResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["events"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("")]
async fn get_events(
    db: web::Data<DatabaseConnection>,
    event_service: web::Data<EventService>,
    query: web::Query<GetEventsQuery>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let events = event_service
        .get_events(
            &db,
            &query.project_id,
            query.event_type.as_deref(),
            query.start_date.as_deref(),
            query.end_date.as_deref(),
            query.page.unwrap_or(1),
            query.limit.unwrap_or(50),
            &user_id,
        )
        .await?;
    
    Ok(HttpResponse::Ok().json(events))
}

/// Get a specific event by ID
#[utoipa::path(
    get,
    path = "/api/v1/events/{id}",
    params(
        ("id" = String, Path, description = "Event ID")
    ),
    responses(
        (status = 200, description = "Event details", body = EventResponse),
        (status = 401, description = "Unauthorized", body = ErrorResponse),
        (status = 403, description = "Access forbidden", body = ErrorResponse),
        (status = 404, description = "Event not found", body = ErrorResponse),
        (status = 500, description = "Internal server error", body = ErrorResponse),
    ),
    tags = ["events"],
    security(
        ("jwt_auth" = [])
    )
)]
#[get("/{id}")]
async fn get_event(
    db: web::Data<DatabaseConnection>,
    event_service: web::Data<EventService>,
    id: web::Path<String>,
    user_id: web::ReqData<String>,
) -> Result<HttpResponse, AppError> {
    let event = event_service
        .get_event_by_id(&db, &id, &user_id)
        .await?;
    
    Ok(HttpResponse::Ok().json(event))
}

#[derive(serde::Deserialize)]
pub struct GetEventsQuery {
    pub project_id: String,
    pub event_type: Option<String>,
    pub start_date: Option<String>,
    pub end_date: Option<String>,
    pub page: Option<i64>,
    pub limit: Option<i64>,
} 