# ==============================================================================
# DEVLIVERY WEBAPI MAKEFILE (Windows Compatible)
# ==============================================================================

PROJECT_NAME = Devlivery
PROJECT_DIR  = src/Devlivery
TEST_PROJECT = test/Devlivery.Tests/Devlivery.Tests.csproj

# Contexts
CONTEXT_DB       = ApplicationDbContext
CONTEXT_IDENTITY = ApplicationIdentityDbContext

# Migrations Paths
MIGRATIONS_DB_PATH       = ./Infrastructure/Persistence/Migrations
MIGRATIONS_IDENTITY_PATH = ./Infrastructure/Identity/Migrations

DOTNET_CMD = dotnet

.PHONY: help build clean test db-add db-update

# No Windows, o 'help' manual é mais seguro para evitar erro de comando não encontrado
help:
	@echo ===================================================
	@echo  $(PROJECT_NAME) CLI - Available Commands
	@echo ===================================================
	@echo  GENERAL:
	@echo    make setup             - Install tools and restore
	@echo    make build             - Build the solution
	@echo    make clean             - Remove build artifacts
	@echo.
	@echo  DATABASE:
	@echo    make migrate-all       - Apply all migrations (App + Identity)
	@echo.
	@echo  DATABASE (App):
	@echo    make db-add V=XXX      - Add migration (Ex: make db-add V=001)
	@echo    make db-update         - Apply migrations
	@echo    make db-remove         - Remove last migration
	@echo.
	@echo  DATABASE (Identity):
	@echo    make id-add V=XXX      - Add identity migration
	@echo    make id-update         - Apply identity migrations
	@echo.
	@echo  TESTS:
	@echo    make test              - Run all tests
	@echo    make test-unit         - Run unit tests
	@echo    make test-coverage     - Run coverage
	@echo ===================================================

# --- Build & Maintenance ---

setup:
	$(DOTNET_CMD) tool restore
	$(DOTNET_CMD) restore

build:
	$(DOTNET_CMD) build $(PROJECT_DIR) -c Release

clean:
	$(DOTNET_CMD) clean
	@if exist TestResults rmdir /s /q TestResults

# --- Database Migrations ---
migrate-all: 
	make db-update && make id-update
	@echo All migrations applied successfully.

# --- Database Migrations (App) ---

db-add:
	@if "$(V)"=="" (echo ERROR: V is required. Use: make db-add V=001 & exit /b 1)
	$(DOTNET_CMD) ef migrations add v$(V) -p $(PROJECT_DIR) -o $(MIGRATIONS_DB_PATH) -c $(CONTEXT_DB)

db-update:
	$(DOTNET_CMD) ef database update -p $(PROJECT_DIR) -c $(CONTEXT_DB)

db-remove:
	$(DOTNET_CMD) ef migrations remove -p $(PROJECT_DIR) -c $(CONTEXT_DB) --force

# --- Database Migrations (Identity) ---

id-add:
	@if "$(V)"=="" (echo ERROR: V is required. Use: make id-add V=001 & exit /b 1)
	$(DOTNET_CMD) ef migrations add v$(V) -p $(PROJECT_DIR) -o $(MIGRATIONS_IDENTITY_PATH) -c $(CONTEXT_IDENTITY)

id-update:
	$(DOTNET_CMD) ef database update -p $(PROJECT_DIR) -c $(CONTEXT_IDENTITY)

# --- Testing ---

test:
	$(DOTNET_CMD) test $(TEST_PROJECT) --logger "console;verbosity=normal"

test-unit:
	$(DOTNET_CMD) test $(TEST_PROJECT) --filter "Category=Unit Tests"

test-coverage:
	$(DOTNET_CMD) test $(TEST_PROJECT) --collect:"XPlat Code Coverage" --results-directory ./TestResults