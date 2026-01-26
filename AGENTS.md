# Project: Devlivery

## Project Overview

This is a monorepo project for a delivery platform named **Devlivery**. It consists of two main applications: a backend API and a frontend web application.

- **Backend API (`apps/api`)**: A .NET Core application built with a focus on clean architecture, vertical slices, and Domain-Driven Design (DDD). It uses ASP.NET Core Minimal APIs, Entity Framework Core, Dapper, and PostgreSQL. It supports multi-tenancy and uses JWT for authentication.

- **Frontend Web (`apps/web`)**: A React application built with TypeScript and Vite. It uses TanStack Query for state management, React Hook Form for forms, and Tailwind CSS for styling. It is modularized by features and includes authentication, a dashboard, and management for orders, products, and cash flow.

## Building and Running

### Prerequisites

- Node.js 18+
- .NET 8+ SDK
- Docker & Docker Compose (recommended)
- PostgreSQL 15+ (or via Docker)

### Local Setup

#### Backend API

To run the backend API, follow these steps:

```bash
cd apps/api
dotnet restore
dotnet build
dotnet ef database update
dotnet run --project src/Devlivery
```

#### Frontend Web

To run the frontend web application, follow these steps:

```bash
cd apps/web
npm install
npm run dev
```

### With Docker Compose

To run both applications using Docker Compose, use the following command from the root directory:

```bash
docker-compose up -d
```

## Development Conventions

### Backend (API)

- **Architecture**: The API follows Vertical Slice Architecture, with features organized in `src/Devlivery/Features`.
- **Testing**: Tests are located in `test/Devlivery.Tests`. To run the tests, use the command `dotnet test test/Devlivery.Tests`.
- **Commits**: The commit history from the original `devlivery-webapi` repository is preserved.

### Frontend (Web)

- **Structure**: The project is organized by features under the `src/features` directory. Shared components and utilities are in `src/shared`.
- **Linting and Formatting**: The project uses Biome for linting and formatting. Use `npm run lint` and `npm run format` to check and format the code.
- **Commits**: The commit history from the original `devlivery-webapp` repository is preserved.
