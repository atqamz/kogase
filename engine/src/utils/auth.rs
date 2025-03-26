use crate::models::auth::Claims;
use crate::utils::errors::AppError;
use argon2::{
    password_hash::{rand_core::OsRng, PasswordHash, PasswordHasher, PasswordVerifier, SaltString},
    Argon2,
};
use chrono::{Duration, Utc};
use jsonwebtoken::{decode, encode, DecodingKey, EncodingKey, Header, TokenData, Validation};
use uuid::Uuid;

/// Hash a password using Argon2
pub fn hash_password(password: &str) -> Result<String, AppError> {
    let salt = SaltString::generate(&mut OsRng);
    let argon2 = Argon2::default();
    
    argon2.hash_password(password.as_bytes(), &salt)
        .map(|hash| hash.to_string())
        .map_err(|e| AppError::InternalServerError(format!("Failed to hash password: {}", e)))
}

/// Verify a password against a hash
pub fn verify_password(password: &str, hash: &str) -> Result<bool, AppError> {
    let parsed_hash = PasswordHash::new(hash)
        .map_err(|e| AppError::InternalServerError(format!("Failed to parse hash: {}", e)))?;
    
    Ok(Argon2::default().verify_password(password.as_bytes(), &parsed_hash).is_ok())
}

/// Generate a JWT token for a user
pub fn generate_token(
    user_id: &Uuid,
    role: &str,
    jwt_secret: &str,
    expiration_hours: u64,
) -> Result<String, AppError> {
    let expiration = Utc::now()
        .checked_add_signed(Duration::hours(expiration_hours as i64))
        .expect("Valid timestamp")
        .timestamp() as u64;

    let claims = Claims {
        sub: user_id.to_string(),
        role: role.to_string(),
        exp: expiration,
        project_id: None,
        is_api_key: false,
    };

    encode(
        &Header::default(),
        &claims,
        &EncodingKey::from_secret(jwt_secret.as_bytes()),
    )
    .map_err(|e| AppError::InternalServerError(format!("Failed to generate token: {}", e)))
}

/// Generate a JWT token for API key authentication
pub fn generate_api_key_token(
    project_id: &Uuid,
    jwt_secret: &str,
) -> Result<String, AppError> {
    // API keys don't expire
    let expiration = Utc::now()
        .checked_add_signed(Duration::days(365 * 10)) // 10 years
        .expect("Valid timestamp")
        .timestamp() as u64;

    let claims = Claims {
        sub: Uuid::new_v4().to_string(), // Random UUID for the token
        role: "api_key".to_string(),
        exp: expiration,
        project_id: Some(project_id.to_string()),
        is_api_key: true,
    };

    encode(
        &Header::default(),
        &claims,
        &EncodingKey::from_secret(jwt_secret.as_bytes()),
    )
    .map_err(|e| AppError::InternalServerError(format!("Failed to generate API key token: {}", e)))
}

/// Verify and decode a JWT token
pub fn verify_token(token: &str, jwt_secret: &str) -> Result<TokenData<Claims>, AppError> {
    decode::<Claims>(
        token,
        &DecodingKey::from_secret(jwt_secret.as_bytes()),
        &Validation::default(),
    )
    .map_err(|e| AppError::AuthError(format!("Invalid token: {}", e)))
} 