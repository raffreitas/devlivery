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

**Método**: Migration Bundles (implementado no workflow)

O workflow `main-build-deploy.yml` gera **migration bundles** durante o build:

```yaml
- name: Generate migration bundles
  run: |
    chmod +x ./scripts/generate-migration-bundle.sh
    ./scripts/generate-migration-bundle.sh

- name: Upload migration bundles as artifact
  uses: actions/upload-artifact@v4
  with:
    name: migration-bundles
```

**O que são Migration Bundles?**
- Executáveis standalone que aplicam migrations
- Não requerem .NET SDK instalado
- Self-contained e portáveis
- Suportam rollback e target específico

**Prós**:
- ✅ Seguro para produção
- ✅ Versionado junto com o código
- ✅ Pode ser executado em qualquer ambiente
- ✅ Suporta rollback

**Contras**:
- ⚠️ Requer configuração de connection string

---

### 3. **Staging/Production Deploy** 🌐

**Método**: Job separado no CI/CD (comentado no workflow)

Para ativar a aplicação automática de migrations em produção:

1. **Descomente** o job `apply-migrations` no workflow
2. **Configure** o secret no GitHub:
   - `DATABASE_CONNECTION_STRING` - Usado para ambos ApplicationDbContext e ApplicationIdentityDbContext
   <!-- Apenas um secret é necessário; ambos os DbContexts usam o mesmo connection string. -->

```yaml
apply-migrations:
  name: Apply Database Migrations
  needs: build-test-publish
  steps:
    - name: Apply ApplicationDbContext migrations
      env:
        CONNECTION_STRING: ${{ secrets.DATABASE_CONNECTION_STRING }}
      run: |
        ./migration-bundles/efbundle-db --connection "$CONNECTION_STRING"
```

**Prós**:
- ✅ Totalmente automatizado
- ✅ Aplicado antes do deploy da aplicação
- ✅ Logs centralizados no GitHub Actions

**Contras**:
- ⚠️ Requer gestão de secrets
- ⚠️ Pode causar downtime em migrations pesadas

---

### 4. **Manual/SSH** 🖥️

**Método**: Executar bundles diretamente no servidor

```bash
# 1. Baixar os bundles do GitHub Releases
wget https://github.com/raffreitas/devlivery-webapi/releases/download/v2024.11.04/migration-bundles.zip
unzip migration-bundles.zip

# 2. Tornar executável
chmod +x efbundle-db efbundle-identity

# 3. Aplicar migrations
./efbundle-db --connection "Host=localhost;Database=devlivery;..."
./efbundle-identity --connection "Host=localhost;Database=devlivery;..."
```

**Prós**:
- ✅ Controle total
- ✅ Pode ser executado em janela de manutenção
- ✅ Fácil rollback

**Contras**:
- ❌ Manual
- ❌ Requer acesso ao servidor

---

## 📦 Estrutura de Migration Bundles

Após executar o script de geração, você terá:

```
migration-bundles/
├── efbundle-db              # ApplicationDbContext migrations
└── efbundle-identity        # ApplicationIdentityDbContext migrations
```

Estes bundles são:
- **Self-contained**: Não precisam do .NET SDK
- **Versionados**: Correspondem ao commit/tag do código
- **Idempotentes**: Podem ser executados múltiplas vezes
- **Armazenados**: Como artifacts no GitHub Actions

## 🔐 Configuração de Secrets

Para usar migrations automáticas em produção, configure no GitHub:

**Settings → Secrets and variables → Actions → New repository secret**

```bash
DATABASE_CONNECTION_STRING="Host=prod-db.example.com;Port=5432;Database=devlivery;Username=app_user;Password=***"
IDENTITY_CONNECTION_STRING="Host=prod-db.example.com;Port=5432;Database=devlivery;Username=app_user;Password=***"
```

> **Nota**: Este projeto usa o **mesmo banco** para ambos os contextos, apenas schemas diferentes. Você pode usar a mesma connection string para ambos.

## 🎯 Recomendações

### Para Desenvolvimento
✅ Use auto-migration (padrão atual)

### Para Staging
✅ Use migration bundles via CI/CD job automatizado

### Para Produção
✅ **Recomendado**: Migration bundles executados manualmente em janela de manutenção
⚠️ **Alternativa**: CI/CD automatizado (se tiver bons testes de integração)

## 🔄 Workflow Completo

1. **Desenvolver feature** → criar migrations locais com Makefile
2. **Commit & Push** → migrations vão junto no repositório
3. **CI/CD Build** → gera migration bundles automaticamente
4. **Deploy** → escolher estratégia:
   - Auto (CI/CD job)
   - Manual (download + execute bundles)
   - Container startup (adicionar script no Dockerfile)

## 🐳 Bonus: Migrations no Docker

Se quiser aplicar migrations no startup do container:

```dockerfile
# Adicionar ao Dockerfile
COPY --from=publish /migration-bundles /app/migrations
RUN chmod +x /app/migrations/efbundle-*

# Criar entrypoint script
COPY docker-entrypoint.sh /app/
RUN chmod +x /app/docker-entrypoint.sh
ENTRYPOINT ["/app/docker-entrypoint.sh"]
```

```bash
# docker-entrypoint.sh
#!/bin/bash
set -e

echo "Applying migrations..."
/app/migrations/efbundle-db --connection "$ConnectionStrings__DefaultConnection"
/app/migrations/efbundle-identity --connection "$ConnectionStrings__DefaultConnection"

echo "Starting application..."
exec dotnet Devlivery.WebApi.dll
```

## 📚 Referências

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Migration Bundles](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#bundles)
- [GitHub Actions Artifacts](https://docs.github.com/en/actions/using-workflows/storing-workflow-data-as-artifacts)
