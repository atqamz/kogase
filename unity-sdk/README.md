# Kogase Unity SDK

The Kogase Unity SDK provides game telemetry and analytics tracking capabilities for Unity games. It allows you to easily record and track various game events, metrics, and player behavior.

## Installation

1. Import the Kogase Unity SDK package into your Unity project
2. Create an empty GameObject in your initial scene
3. Add the `KogaseManager` component to the GameObject
4. Configure the API URL and API Key in the inspector

## Configuration

The `KogaseManager` component requires two configuration parameters:

- **API URL**: The URL of your Kogase server (e.g., `http://localhost:8080` for local development)
- **API Key**: The API key generated for your project in the Kogase dashboard

## Usage

### Basic Event Tracking

```csharp
// Record a simple event
KogaseSDK.Instance.RecordEvent("level_start");

// Record an event with parameters
var parameters = new Dictionary<string, object>
{
    { "level_id", "level_1" },
    { "difficulty", "hard" },
    { "player_health", 100 }
};
KogaseSDK.Instance.RecordEvent("level_complete", parameters);
```

### Sample Events to Track

- Game Start/End
```csharp
KogaseSDK.Instance.RecordEvent("game_start");
KogaseSDK.Instance.RecordEvent("game_end", new Dictionary<string, object>
{
    { "total_playtime", playTime },
    { "score", finalScore }
});
```

- Level Progress
```csharp
KogaseSDK.Instance.RecordEvent("level_start", new Dictionary<string, object>
{
    { "level_id", currentLevel },
    { "difficulty", difficulty }
});

KogaseSDK.Instance.RecordEvent("level_complete", new Dictionary<string, object>
{
    { "level_id", currentLevel },
    { "time_taken", completionTime },
    { "stars_earned", stars }
});
```

- In-Game Actions
```csharp
KogaseSDK.Instance.RecordEvent("item_collected", new Dictionary<string, object>
{
    { "item_id", itemId },
    { "item_type", itemType },
    { "location", playerPosition }
});

KogaseSDK.Instance.RecordEvent("achievement_unlocked", new Dictionary<string, object>
{
    { "achievement_id", achievementId },
    { "difficulty", difficulty }
});
```

## Features

- Automatic event batching and sending
- Offline event queueing
- Automatic retry on failed network requests
- Device information tracking
- Installation tracking
- Cross-scene persistence

## Best Practices

1. **Event Names**: Use clear, descriptive names for events following a consistent naming convention (e.g., `level_start`, `item_collected`)

2. **Parameters**: Include relevant context in event parameters, but avoid sending sensitive or personal information

3. **Frequency**: Balance the frequency of event tracking to avoid overwhelming your server while maintaining meaningful analytics

4. **Error Handling**: The SDK handles network errors automatically, but ensure your game's critical functionality doesn't depend on successful event tracking

## Automatic Event Flushing

The SDK automatically flushes events in the following situations:

- When the event queue reaches its maximum size (100 events)
- Every 60 seconds during gameplay
- When the application is paused or quit
- When manually called via `KogaseSDK.Instance.FlushEvents()`

## Technical Details

- Events are stored in memory until flushed
- Failed event sends are automatically requeued
- The SDK is thread-safe and handles Unity's lifecycle events appropriately
- Installation events are automatically tracked on first launch

## Support

For issues, feature requests, or questions, please visit the [Kogase GitHub repository](https://github.com/kogase/kogase). 