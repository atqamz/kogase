package models

import (
	"time"

	"github.com/google/uuid"
	"gorm.io/gorm"
)

// Project represents a game project that is tracked by Kogase
type Project struct {
	ID        uuid.UUID      `json:"id" gorm:"type:uuid;primary_key"`
	Name      string         `json:"name" gorm:"not null"`
	ApiKey    string         `json:"api_key,omitempty" gorm:"unique;not null"` // Not always exposed in responses
	OwnerID   uuid.UUID      `json:"owner_id" gorm:"type:uuid;not null"`
	CreatedAt time.Time      `json:"created_at"`
	UpdatedAt time.Time      `json:"updated_at"`
	DeletedAt gorm.DeletedAt `json:"-" gorm:"index"`
	Users     []User         `json:"users,omitempty" gorm:"many2many:project_users;"`
	Devices   []Device       `json:"devices,omitempty" gorm:"foreignKey:ProjectID"`
	Events    []Event        `json:"events,omitempty" gorm:"foreignKey:ProjectID"`
}

// BeforeCreate will set a UUID rather than numeric ID and generate an API key
func (project *Project) BeforeCreate(tx *gorm.DB) error {
	if project.ID == uuid.Nil {
		project.ID = uuid.New()
	}

	// Generate API key if empty
	if project.ApiKey == "" {
		project.ApiKey = uuid.New().String()
	}

	return nil
}
