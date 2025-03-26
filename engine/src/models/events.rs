use entity::prelude::EventsModel;
use serde::{Deserialize, Serialize};
use serde_json::Value;
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct EventData {
    pub event_type: String,
    pub event_name: Option<String>,
    pub parameters: Option<Value>,
    pub timestamp: String,
    pub device_id: String,
    pub platform: String,
    pub os_version: Option<String>,
    pub app_version: Option<String>,
    pub ip_address: Option<String>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct BatchEventsRequest {
    pub events: Vec<EventData>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct EventResponse {
    pub id: String,
    pub project_id: String,
    pub device_id: String,
    pub event_type: String,
    pub event_name: Option<String>,
    pub parameters: Option<Value>,
    pub timestamp: String,
    pub received_at: String,
}

impl From<EventsModel> for EventResponse {
    fn from(model: EventsModel) -> Self {
        Self {
            id: model.id.to_string(),
            project_id: model.project_id.to_string(),
            device_id: model.device_id.to_string(),
            event_type: model.event_type,
            event_name: model.event_name,
            parameters: model.parameters,
            timestamp: model.timestamp.to_rfc3339(),
            received_at: model.received_at.to_rfc3339(),
        }
    }
}

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionStartRequest {
    pub device_id: String,
    pub platform: String,
    pub os_version: Option<String>,
    pub app_version: Option<String>,
    pub ip_address: Option<String>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionEndRequest {
    pub device_id: String,
    pub duration_seconds: u64,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct InstallationRequest {
    pub device_id: String,
    pub platform: String,
    pub os_version: Option<String>,
    pub app_version: Option<String>,
    pub ip_address: Option<String>,
}

#[derive(Debug, Deserialize)]
pub struct QueryEventsRequest {
    pub event_type: Option<String>,
    pub event_name: Option<String>,
    pub device_id: Option<String>,
    pub start_date: Option<String>,
    pub end_date: Option<String>,
    pub page: Option<u64>,
    pub page_size: Option<u64>,
}

#[derive(Debug, Serialize)]
pub struct QueryEventsResponse {
    pub events: Vec<EventResponse>,
    pub total: u64,
    pub page: u64,
    pub page_size: u64,
    pub pages: u64,
} 