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
  - Server responses leverage Protobuf's `OneOf` to return either a successful response or an error. app can be extended by additional messages handlers by implementing the IHandler interface and by registring it to the ioc container. the handler will need to expose a new meesage type in order for it to be correctly routed. 
    ```
    services.AddSingleton<IHandler, NewHandler>();
    ```
    ```
    public override MessageType MessageType { get; } = MessageType.UpdateRequest;
    ```
- **Scalability**:
  - Redis Pub/Sub is used for notifications, supporting horizontal scaling across multiple server instances.

---

## Containerization
To containerize and run the application, use the following command, it will spin up both app & redis container:

```bash
docker-compose up --build
```

## Test Client
Test client can be run by using the following command under the Game.Client lib

```bash
.\Game.Client.exe
```




---

## TODO

- [x] Add Unit Tests
- [x] Implement Clean Architecture
- [x] Validate Core Functionality
- [x] Support Horizontal Scaling
- [ ] Add Integration Tests with TestContainers
- [x] Dockerize Server
- [x] Validate Database Schema
- [x] Implement Proper Input Validation
- [x] Improve Client Console Output
- [ ] Use https with certificate for secured connection

---

For contributions or further inquiries, please feel free to open an issue or reach out directly.

