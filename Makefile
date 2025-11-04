# Makefile para gerenciamento de migrations do Devlivery WebAPI
# Padrão de versionamento: vXXX (ex: v001, v002, v003)

# Diretório do projeto
PROJECT_DIR = src/Devlivery.WebApi

# Caminhos das migrations
MIGRATIONS_DB_PATH = ./Shared/Infrastructure/Database/Migrations
MIGRATIONS_IDENTITY_PATH = ./Shared/Infrastructure/Identity/Migrations

# Contextos
CONTEXT_DB = ApplicationDbContext
CONTEXT_IDENTITY = ApplicationIdentityDbContext

# Cores para output (PowerShell)
.PHONY: help migration-db migration-identity migration-new migration-update-db migration-update-identity migration-status migration-remove-db migration-remove-identity

help:
	@echo "==================================================="
	@echo "  Devlivery WebAPI - Gerenciamento de Migrations"
	@echo "==================================================="
	@echo ""
	@echo "Comandos disponíveis:"
	@echo ""
	@echo "  make migration-new VERSION=XXX       - Cria migration do ApplicationDbContext"
	@echo "  make migration-db VERSION=XXX        - Cria migration do ApplicationDbContext"
	@echo "  make migration-identity VERSION=XXX  - Cria migration do ApplicationIdentityDbContext"
	@echo ""
	@echo "  make migration-update-db             - Aplica migrations do Database"
	@echo "  make migration-update-identity       - Aplica migrations do Identity"
	@echo ""
	@echo "  make migration-status                - Mostra status das migrations"
	@echo "  make migration-remove-db             - Remove última migration do Database"
	@echo "  make migration-remove-identity       - Remove última migration do Identity"
	@echo ""
	@echo "  make migration-apply-all             - Aplica todas as migrations (DB + Identity)"
	@echo ""
	@echo "Exemplos:"
	@echo "  make migration-new VERSION=002"
	@echo "  make migration-update-db"
	@echo ""

# Cria migration do ApplicationDbContext
migration-db:
ifndef VERSION
	@echo "❌ Erro: VERSION não foi especificado."
	@echo "Uso: make migration-db VERSION=XXX"
	@exit 1
endif
	@echo "📦 Criando migration v$(VERSION) para ApplicationDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef migrations add v$(VERSION) -o $(MIGRATIONS_DB_PATH) -c $(CONTEXT_DB)
	@echo "✅ Migration v$(VERSION) criada com sucesso para Database!"

# Cria migration do ApplicationIdentityDbContext
migration-identity:
ifndef VERSION
	@echo "❌ Erro: VERSION não foi especificado."
	@echo "Uso: make migration-identity VERSION=XXX"
	@exit 1
endif
	@echo "🔐 Criando migration v$(VERSION) para ApplicationIdentityDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef migrations add v$(VERSION) -o $(MIGRATIONS_IDENTITY_PATH) -c $(CONTEXT_IDENTITY)
	@echo "✅ Migration v$(VERSION) criada com sucesso para Identity!"

# Alias para migration-db (comando principal)
migration-new:
	@$(MAKE) migration-db VERSION=$(VERSION)

# Aplica migrations do ApplicationDbContext
migration-update-db:
	@echo "📦 Aplicando migrations do ApplicationDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef database update -c $(CONTEXT_DB)
	@echo "✅ Database atualizado com sucesso!"

# Aplica migrations do ApplicationIdentityDbContext
migration-update-identity:
	@echo "🔐 Aplicando migrations do ApplicationIdentityDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef database update -c $(CONTEXT_IDENTITY)
	@echo "✅ Identity database atualizado com sucesso!"

# Mostra status das migrations
migration-status:
	@echo "📊 Status das Migrations - ApplicationDbContext:"
	@echo "================================================"
	@cd $(PROJECT_DIR) && dotnet ef migrations list -c $(CONTEXT_DB) || echo "Nenhuma migration encontrada"
	@echo ""
	@echo "📊 Status das Migrations - ApplicationIdentityDbContext:"
	@echo "========================================================="
	@cd $(PROJECT_DIR) && dotnet ef migrations list -c $(CONTEXT_IDENTITY) || echo "Nenhuma migration encontrada"

# Remove última migration apenas do Database
migration-remove-db:
	@echo "🗑️  Removendo última migration do ApplicationDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef migrations remove -c $(CONTEXT_DB) --force
	@echo "✅ Migration removida com sucesso!"

# Remove última migration apenas do Identity
migration-remove-identity:
	@echo "🗑️  Removendo última migration do ApplicationIdentityDbContext..."
	@cd $(PROJECT_DIR) && dotnet ef migrations remove -c $(CONTEXT_IDENTITY) --force
	@echo "✅ Migration removida com sucesso!"

# Aplica todas as migrations (Database + Identity)
migration-apply-all:
	@echo "🚀 Aplicando todas as migrations..."
	@$(MAKE) migration-update-db
	@echo ""
	@$(MAKE) migration-update-identity
	@echo ""
	@echo "✅ Todas as migrations foram aplicadas com sucesso!"
