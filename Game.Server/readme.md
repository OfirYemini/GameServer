# Game Server - README

## Overview
This project implements a game server following clean architecture principles. The solution is designed for scalability and maintainability, leveraging Redis for notifications and ensuring data consistency with transactions and optimistic locking.

---

## Prerequisites
Ensure you have the following tools installed before running the application:
- .NET SDK 8.0+
- Docker (for containerization)
- Redis (for pubsub operations)

---

## Getting Started

### 1. Install Entity Framework Core CLI
```bash
 dotnet tool install --global dotnet-ef --version 8.0.0
```

### 2. Apply Database Migrations
Navigate to the Game.Server project directory and run the following commands:
```bash
 cd Game.Server 
 dotnet ef database update
```

### 3. Dropping the Database (Optional)
If you need to reset the database, you can drop it using:
```bash
 dotnet ef database drop
```

---

## Running Redis with Docker
If you do not have Redis installed locally, you can run it using Docker:
```bash
 docker run --name redis-server -d -p 6379:6379 redis
```

To verify Redis is running:
```bash
 docker ps
```
To stop and remove the Redis container (if needed):
```bash
 docker stop redis-server
 docker rm redis-server
```

or if you prefer you can use the docker compose command to quickly run the app:
```bash
docker-compose up --build
```


---

## Configuration
Configuration can be set through multiple providers, following industry best practices:
- **appsettings.json**
- **Environment Variables**
- **Command Line Arguments**

---

## Database Schema
The database schema consists of two main tables:
- **Player Table**:
  - `playerId` - Primary Key
  - `deviceId` - Indexed for faster lookups
- **PlayerBalance Table**:
  - Composite Primary Key: `playerId` + `resourceType`

### Data Integrity and Concurrency
- **Transactional Operations**: Transactions are used to ensure atomicity during player balance transfers, preventing partial updates.
- **Optimistic Locking**: Concurrency versioning is employed to avoid data inconsistencies arising from parallel execution.

---

## Application Logic
- **Player Registration/Login**:
  - A player is automatically registered upon login if the `deviceId` is new.
  - While a dedicated sign-in method would typically handle this, the current implementation consolidates these operations for simplicity during testing.
- **Response Handling**:
  - Server responses leverage Protobuf's `OneOf` to return either a successful response or an error.
- **Scalability**:
  - Redis Pub/Sub is used for notifications, supporting horizontal scaling across multiple server instances.

---

## Containerization
To containerize and run the application, use the following commands:

```bash
 docker build -t gameserver .
 docker run -d -p 5214:5214 gameserver
```

---

## TODO

- [x] Add Unit Tests
- [x] Implement Clean Architecture
- [x] Validate Core Functionality
- [x] Support Horizontal Scaling
- [ ] Add Integration Tests with TestContainers
- [ ] Dockerize Both Server and Client
- [x] Validate Database Schema
- [x] Implement Proper Input Validation
- [x] Improve Client Console Output
- [ ] Enable Client Extensions

---

For contributions or further inquiries, please feel free to open an issue or reach out directly.

