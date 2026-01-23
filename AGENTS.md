# Project: Devlivery

## Project Overview

This is the backend API for Devlivery, a delivery management system. It's a .NET 10 modular monolith built with ASP.NET Core, following a vertical slice architecture. The codebase is organized by features, promoting high cohesion and low coupling between different parts of the application. It uses a PostgreSQL database for data storage, accessed via Entity Framework Core and Dapper.

## Key Technologies

*   **.NET 10 / ASP.NET Core:** The core framework for building the web API.
*   **Entity Framework Core & Dapper:** Used for data access to the PostgreSQL database. EF Core is the primary ORM, while Dapper is available for more direct SQL queries.
*   **PostgreSQL:** The relational database used for data storage.
*   **Vertical Slice Architecture:** The code is organized by features, with each feature containing its own domain logic, data access, and API endpoints.
*   **Mediator Pattern:** Used to decouple components within each feature, with `Mediator.Abstractions` and its source generator being a key dependency.
*   **FluentValidation:** For implementing business rule validations.
*   **JWT & ASP.NET Core Identity:** For handling authentication and user management.
*   **OpenTelemetry:** For application observability, including tracing and metrics.
*   **Docker:** The project is containerized and can be run using Docker and Docker Compose.

## Building and Running

### Using .NET CLI

1.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```
2.  **Build the project:**
    ```bash
    dotnet build
    ```
3.  **Run the application:**
    ```bash
    dotnet run --project src/Devlivery
    ```

### Using Docker

1.  **Build and run the containers in detached mode:**
    ```bash
    docker-compose up --build -d
    ```

## Testing

To run the unit and integration tests, use the following command:

```bash
dotnet test
```

## Development Conventions

*   **Vertical Slices:** Each feature is located in its own folder under `src/Devlivery/Features`. Each feature should be self-contained and expose its functionality through API endpoints.
*   **Mediator:** The Mediator pattern is used to handle requests and responses within each feature. Commands and queries are sent through the mediator, which then invokes the appropriate handler.
*   **Validation:** Business rules and input validation should be implemented using FluentValidation.
*   **Database Migrations:** Entity Framework Core's migration tools are used to manage database schema changes.
*   **Configuration:** Application settings are managed through `appsettings.json` files, with environment-specific overrides.
*   **Dependency Injection:** The application makes extensive use of .NET's built-in dependency injection framework. Services are registered in the `Startup.cs` file.
