# Makefile for Devlivery WebAPI migrations management
# Versioning standard: vXXX (ex: v001, v002, v003)

# Project directory
PROJECT_DIR = src/Devlivery

# Migrations paths
MIGRATIONS_DB_PATH = ./Shared/Database/Migrations
MIGRATIONS_IDENTITY_PATH = ./Shared/Identity/Migrations

# Contexts
CONTEXT_DB = ApplicationDbContext
CONTEXT_IDENTITY = ApplicationIdentityDbContext

# Colors for output (PowerShell) - Note: This comment is retained but colors themselves are removed from output messages
.PHONY: help migration-db migration-identity migration-new migration-update-db migration-update-identity migration-status migration-remove-db migration-remove-identity migration-apply-all

help:
	@echo "==================================================="
	@echo "  Devlivery WebAPI"
	@echo "==================================================="
	@echo ""
	@echo "Available commands:"
	@echo ""
	@echo "  make migration-new VERSION=XXX        - Creates ApplicationDbContext migration"
	@echo "  make migration-db VERSION=XXX         - Creates ApplicationDbContext migration"
	@echo "  make migration-identity VERSION=XXX   - Creates ApplicationIdentityDbContext migration"
	@echo ""
	@echo "  make migration-update-db              - Applies Database migrations"
	@echo "  make migration-update-identity        - Applies Identity migrations"
	@echo ""
	@echo "  make migration-status                 - Shows migrations status"
	@echo "  make migration-remove-db              - Removes last Database migration"
	@echo "  make migration-remove-identity        - Removes last Identity migration"
	@echo ""
	@echo "  make migration-apply-all              - Applies all migrations (DB + Identity)"
	@echo ""
	@echo "Examples:"
	@echo "  make migration-new VERSION=002"
	@echo "  make migration-update-db"
	@echo ""

# Creates ApplicationDbContext migration
migration-db:
ifndef VERSION
	@echo "Error: VERSION was not specified."
	@echo "Usage: make migration-db VERSION=XXX"
	@exit 1
endif
	@echo "Creating migration v$(VERSION) for ApplicationDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef migrations add v$(VERSION) -o $(MIGRATIONS_DB_PATH) -c $(CONTEXT_DB)
	@echo "Migration v$(VERSION) successfully created for Database!"

# Creates ApplicationIdentityDbContext migration
migration-identity:
ifndef VERSION
	@echo "Error: VERSION was not specified."
	@echo "Usage: make migration-identity VERSION=XXX"
	@exit 1
endif
	@echo "Creating migration v$(VERSION) for ApplicationIdentityDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef migrations add v$(VERSION) -o $(MIGRATIONS_IDENTITY_PATH) -c $(CONTEXT_IDENTITY)
	@echo "Migration v$(VERSION) successfully created for Identity!"

# Alias for migration-db (main command)
migration-new:
	@$(MAKE) migration-db VERSION=$(VERSION)

# Applies ApplicationDbContext migrations
migration-update-db:
	@echo "Applying ApplicationDbContext migrations..."
	@cd $(PROJECT_DIR) && dotnet ef database update -c $(CONTEXT_DB)
	@echo "Database successfully updated!"

# Applies ApplicationIdentityDbContext migrations
migration-update-identity:
	@echo "Applying ApplicationIdentityDbContext migrations..."
	@cd $(PROJECT_DIR) && dotnet ef database update -c $(CONTEXT_IDENTITY)
	@echo "Identity database successfully updated!"

# Shows migrations status
migration-status:
	@echo "Migrations Status - ApplicationDbContext:"
	@echo "================================================"
	@cd $(PROJECT_DIR) && dotnet ef migrations list -c $(CONTEXT_DB) || echo "No migration found"
	@echo ""
	@echo "Migrations Status - ApplicationIdentityDbContext:"
	@echo "========================================================="
	@cd $(PROJECT_DIR) && dotnet ef migrations list -c $(CONTEXT_IDENTITY) || echo "No migration found"

# Removes last Database migration only
migration-remove-db:
	@echo "Removing last ApplicationDbContext migration..."
	@cd $(PROJECT_DIR) && dotnet ef migrations remove -c $(CONTEXT_DB) --force
	@echo "Migration successfully removed!"

# Removes last Identity migration only
migration-remove-identity:
	@echo "Removing last ApplicationIdentityDbContext migration..."
	@cd $(PROJECT_DIR) && dotnet ef migrations remove -c $(CONTEXT_IDENTITY) --force
	@echo "Migration successfully removed!"

# Applies all migrations (Database + Identity)
migration-apply-all:
	@echo "Applying all migrations..."
	@$(MAKE) migration-update-db
	@echo ""
	@$(MAKE) migration-update-identity
	@echo ""
	@echo "All migrations were successfully applied!"