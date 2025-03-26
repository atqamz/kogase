use entity::prelude::MetricsModel;
use serde::{Deserialize, Serialize};
use serde_json::Value;
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct Metric {
    pub metric_type: String,
    pub period: String,
    pub period_start: String,
    pub value: f64,
    pub dimensions: Option<Value>,
}

impl From<MetricsModel> for Metric {
    fn from(model: MetricsModel) -> Self {
        Self {
            metric_type: model.metric_type,
            period: model.period,
            period_start: model.period_start.to_rfc3339(),
            value: model.value,
            dimensions: model.dimensions,
        }
    }
}

#[derive(Debug, Deserialize)]
pub struct QueryMetricsRequest {
    pub metric_type: Option<String>,
    pub period: Option<String>,
    pub start_date: Option<String>,
    pub end_date: Option<String>,
    pub dimension: Option<String>,
    pub dimension_value: Option<String>,
}

#[derive(Debug, Serialize)]
pub struct QueryMetricsResponse {
    pub metrics: Vec<Metric>,
}

#[derive(Debug, Serialize)]
pub struct MetricTimeseriesPoint {
    pub timestamp: String,
    pub value: f64,
}

#[derive(Debug, Serialize)]
pub struct MetricTimeseries {
    pub metric_type: String,
    pub period: String,
    pub data: Vec<MetricTimeseriesPoint>,
}

#[derive(Debug, Serialize)]
pub struct DashboardMetrics {
    pub dau: MetricTimeseries,
    pub mau: MetricTimeseries,
    pub new_users: MetricTimeseries,
    pub sessions: MetricTimeseries,
    pub session_duration: MetricTimeseries,
}

#[derive(Debug, Serialize)]
pub struct RetentionData {
    pub day: i32,
    pub retention_rate: f64,
    pub user_count: i64,
}

#[derive(Debug, Serialize)]
pub struct RetentionMetrics {
    pub data: Vec<RetentionData>,
} 