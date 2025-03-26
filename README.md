# Kogase - Game Analytics and Telemetry

Kogase is an open-source, self-hosted game analytics and telemetry platform. It provides a simple, yet powerful way to track and analyze player behavior in your games.

## Features

- **Game Telemetry**: Track player actions, sessions, and custom events
- **Analytics Dashboard**: Visualize your game's performance metrics
- **Multiple Projects**: Manage multiple games from a single dashboard
- **Self-hosted**: Full control over your data
- **Unity SDK**: Easy integration with Unity games (coming soon)

## Tech Stack

- **Backend**: Go with Gin framework
- **Database**: PostgreSQL
- **Frontend**: Coming soon (React or Vue.js)
- **Unity SDK**: Coming soon

## Quick Start

### Prerequisites

- Docker and Docker Compose
- Make (optional, for using Makefile commands)

### Running with Docker Compose

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/kogase.git
   cd kogase
   ```

2. Create a `.env` file based on the example:
   ```
   cp backend/.env.example backend/.env
   ```

3. Start the services:
   ```
   docker-compose up -d
   ```

4. Access the API at `http://localhost:8080/api/v1`

### Running the Backend Locally

1. Ensure you have Go 1.22 or later installed
2. Set up a PostgreSQL database
3. Configure the `.env` file in the `backend` directory
4. Run the backend:
   ```
   cd backend
   go run main.go
   ```

## API Documentation

The API documentation is available at `/api/v1/docs` when the server is running.

## Project Structure

```
kogase/
├── backend/            # Go backend
│   ├── controllers/    # API controllers
│   ├── middleware/     # Custom middleware
│   ├── models/         # Database models
│   ├── server/         # Server configuration
│   └── utils/          # Utility functions
├── frontend/           # Frontend application (coming soon)
├── unity-sdk/          # Unity SDK (coming soon)
└── docker-compose.yaml # Docker Compose configuration
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request 