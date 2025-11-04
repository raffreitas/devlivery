# 🔄 Estratégia de Migrations - Devlivery WebAPI

Este documento descreve as estratégias para aplicar migrations no Devlivery WebAPI em diferentes ambientes.

## 📋 Contextos de Banco de Dados

O projeto utiliza **dois DbContexts separados**:

1. **ApplicationDbContext** - Dados da aplicação (Products, Orders, etc.)
2. **ApplicationIdentityDbContext** - Dados de autenticação (Users, Roles, etc.)

## 🛠️ Ferramentas Disponíveis

### Makefile (Desenvolvimento Local)

```bash
# Criar nova migration
make migration-db VERSION=002           # ApplicationDbContext
make migration-identity VERSION=002     # ApplicationIdentityDbContext

# Aplicar migrations
make migration-update-db                # ApplicationDbContext
make migration-update-identity          # ApplicationIdentityDbContext

# Verificar status
make migration-status

# Remover última migration
make migration-remove-db
make migration-remove-identity
```

### Scripts PowerShell/Bash

```bash
# Aplicar todas as migrations localmente
./scripts/apply-migrations.sh          # Linux/macOS
./scripts/apply-migrations.ps1         # Windows

# Gerar migration bundles
./scripts/generate-migration-bundle.sh
```

## 🚀 Estratégias por Ambiente

### 1. **Desenvolvimento Local** ⚙️

**Método**: Auto-migration no startup (padrão atual)

O arquivo `Startup.cs` aplica automaticamente as migrations quando em modo Development:

```csharp
if (app.Environment.IsDevelopment())
{
    // Auto migrate and seed
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    // ...
}
```

**Prós**:
- ✅ Zero configuração
- ✅ Rápido para desenvolvimento
- ✅ Seed automático de dados

**Contras**:
- ❌ Não recomendado para produção

---

### 2. **CI/CD Pipeline** 🔄

**Método**: dotnet ef via GitHub Actions

O workflow `main-build-deploy.yml` possui um job opcional (comentado) que aplica migrations usando `dotnet ef`:

```yaml
- name: Apply ApplicationDbContext migrations
  env:
    ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION_STRING }}
  run: |
    cd src/Devlivery.WebApi
    dotnet ef database update -c ApplicationDbContext
```

**Como funciona:**
1. Instala o `dotnet-ef` global tool
2. Usa variável de ambiente para connection string
3. Executa `dotnet ef database update` para cada contexto
4. Falha o deploy se migrations falharem

**Prós**:
- ✅ Simples e direto
- ✅ Usa ferramentas oficiais do EF Core
- ✅ Logs claros no GitHub Actions
- ✅ Não requer artifacts ou bundles

**Contras**:
- ⚠️ Requer .NET SDK instalado (já presente no runner)
- ⚠️ Precisa do código-fonte (já feito checkout)

---

### 3. **Staging/Production Deploy** 🌐

**Método**: Job automatizado no CI/CD ✅ **ATIVO**

O workflow está configurado para aplicar migrations automaticamente na branch `main`:

**Pré-requisitos:**
1. ✅ Job `apply-migrations` está ativo no workflow
2. ⚠️ **Configure** o secret no GitHub:
   - `DATABASE_CONNECTION_STRING` - Connection string do banco de produção

**Como funciona:**
```yaml
apply-migrations:
  name: Apply Database Migrations
  needs: build-test-publish  # Executa após build bem-sucedido
  if: github.ref == 'refs/heads/main'  # Apenas na branch main
  steps:
    - name: Apply ApplicationDbContext migrations
      env:
        ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION_STRING }}
      run: |
        cd src/Devlivery.WebApi
        dotnet ef database update -c ApplicationDbContext
```

**Fluxo:**
1. Push na branch `main`
2. Build & Test executado
3. ✅ **Migrations aplicadas automaticamente**
4. Docker image publicado
5. Release criado

**Prós**:
- ✅ Totalmente automatizado
- ✅ Aplicado antes do deploy da aplicação
- ✅ Logs centralizados no GitHub Actions
- ✅ Usa ferramentas oficiais do EF Core
- ✅ Falha o deploy se migrations falharem

**Contras**:
- ⚠️ Requer configuração de secret `DATABASE_CONNECTION_STRING`
- ⚠️ Pode causar downtime em migrations pesadas
- ⚠️ Executa automaticamente a cada push na main

---

### 4. **Manual/SSH** 🖥️

**Método**: Executar script ou comando direto no servidor

```bash
# Opção 1: Usar o script
export DATABASE_CONNECTION_STRING="Host=prod-db;Database=devlivery;Username=user;Password=***"
./scripts/apply-migrations.sh

# Opção 2: Executar comandos diretos
cd src/Devlivery.WebApi
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c ApplicationIdentityDbContext
```

**Prós**:
- ✅ Controle total
- ✅ Pode ser executado em janela de manutenção
- ✅ Fácil rollback (`dotnet ef database update <migration-anterior>`)

**Contras**:
- ❌ Manual
- ❌ Requer acesso ao servidor
- ❌ Requer .NET SDK instalado no servidor

---

## 🔐 Configuração de Secrets

⚠️ **IMPORTANTE**: O job de migrations está ativo e requer configuração!

Para que as migrations automáticas funcionem em produção, você **DEVE** configurar o secret no GitHub:

**Passo a passo:**

1. Vá em **Settings → Secrets and variables → Actions**
2. Clique em **New repository secret**
3. Adicione:
   - **Name**: `DATABASE_CONNECTION_STRING`
   - **Value**: Sua connection string de produção

**Exemplo de connection string:**
```bash
Host=prod-db.example.com;Port=5432;Database=devlivery;Username=app_user;Password=sua_senha_segura;SSL Mode=Require
```

> **Nota**: Este projeto usa o **mesmo banco** para ambos os contextos (ApplicationDbContext e ApplicationIdentityDbContext), apenas schemas/tabelas diferentes. Use a mesma connection string para ambos.

> **⚠️ Segurança**: Nunca commite a connection string de produção no código! Use sempre secrets do GitHub.

## 🎯 Recomendações

### Para Desenvolvimento
✅ Use auto-migration (padrão atual) - configurado em `Startup.cs`

### Para Staging/Produção
✅ **Configurado**: CI/CD job automatizado com `dotnet ef` ativo
⚠️ **Requer**: Secret `DATABASE_CONNECTION_STRING` configurado no GitHub
💡 **Alternativa**: Desabilitar o job e usar execução manual via SSH (mais controle)

## 🔄 Workflow Completo

### 🎯 Fluxo Atual (Automatizado)

1. **Desenvolver feature** → criar migrations locais com `make migration-db VERSION=002`
2. **Commit & Push** → migrations vão junto no repositório
3. **CI/CD Build** → compila e testa a aplicação
4. **✅ Apply Migrations** → job automaticamente aplica migrations no banco (branch main)
5. **Deploy** → Docker image é publicado no GitHub Container Registry
6. **Release** → Nova release criada com a versão

### ⚙️ Configuração Necessária

Antes do primeiro deploy para produção:

```bash
# 1. Configure o secret no GitHub
GitHub → Settings → Secrets → New repository secret
Name: DATABASE_CONNECTION_STRING
Value: Host=prod-db;Port=5432;Database=devlivery;Username=...;Password=...

# 2. Push na branch main
git push origin main

# 3. Acompanhe no GitHub Actions
# O workflow irá:
#   ✅ Build & Test
#   ✅ Apply Migrations (se secret configurado)
#   ✅ Build & Push Docker image
#   ✅ Create Release
```

### 🔄 Fluxo Alternativo (Manual)

Se preferir controle manual, você pode:
1. Comentar o job `apply-migrations` no workflow
2. Aplicar migrations manualmente via SSH:
   ```bash
   export DATABASE_CONNECTION_STRING="..."
   ./scripts/apply-migrations.sh
   ```

## 🐳 Bonus: Migrations no Docker

Se quiser aplicar migrations no startup do container:

**Opção 1: Multi-stage build com script** (Recomendado)

```dockerfile
# Adicionar stage para instalar EF tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS migration
WORKDIR /src
COPY ["src/Devlivery.WebApi/Devlivery.WebApi.csproj", "src/Devlivery.WebApi/"]
RUN dotnet restore "src/Devlivery.WebApi/Devlivery.WebApi.csproj"
COPY . .
RUN dotnet tool install --global dotnet-ef

# Stage final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=migration /root/.dotnet/tools /tools
ENV PATH="${PATH}:/tools"

# Copiar script de entrypoint
COPY docker-entrypoint.sh /app/
RUN chmod +x /app/docker-entrypoint.sh
ENTRYPOINT ["/app/docker-entrypoint.sh"]
```

**docker-entrypoint.sh:**
```bash
#!/bin/bash
set -e

echo "Applying migrations..."

if [ -n "$ConnectionStrings__DefaultConnection" ]; then
    cd /src/src/Devlivery.WebApi
    dotnet ef database update -c ApplicationDbContext || echo "Warning: Failed to apply DB migrations"
    dotnet ef database update -c ApplicationIdentityDbContext || echo "Warning: Failed to apply Identity migrations"
else
    echo "Warning: No connection string provided, skipping migrations"
fi

echo "Starting application..."
cd /app
exec dotnet Devlivery.WebApi.dll
```

**Opção 2: Init container no Kubernetes/Docker Compose**

```yaml
# docker-compose.yml
services:
  migration:
    image: devlivery-webapi:latest
    command: sh -c "dotnet ef database update -c ApplicationDbContext && dotnet ef database update -c ApplicationIdentityDbContext"
    environment:
      - ConnectionStrings__DefaultConnection=${DATABASE_CONNECTION_STRING}
    depends_on:
      - postgres
  
  api:
    image: devlivery-webapi:latest
    depends_on:
      migration:
        condition: service_completed_successfully
    # ... resto da config
```

## 📚 Referências

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Migration Bundles](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#bundles)
- [GitHub Actions Artifacts](https://docs.github.com/en/actions/using-workflows/storing-workflow-data-as-artifacts)
