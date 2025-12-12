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

**Método**: dotnet ef via GitHub Actions (✅ ATIVO)

O workflow `main-build-deploy.yml` possui um job dedicado que aplica migrations automaticamente:

```yaml
apply-migrations:
  name: Apply Database Migrations
  needs: build-test-publish
  if: github.ref == 'refs/heads/main'
  steps:
    - name: Apply ApplicationDbContext migrations
      env:
        ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION_STRING }}
      run: |
        cd src/Devlivery
        dotnet ef database update -c ApplicationDbContext
```

**Como funciona:**
1. Executa após o build ser bem-sucedido
2. Instala o `dotnet-ef` global tool
3. Usa secret `DATABASE_CONNECTION_STRING` do GitHub
4. Executa `dotnet ef database update` para cada contexto
5. Falha o deploy se migrations falharem

**Prós**:
- ✅ Totalmente automatizado
- ✅ Simples e direto
- ✅ Usa ferramentas oficiais do EF Core
- ✅ Logs claros no GitHub Actions
- ✅ Aplicado antes de qualquer deploy

**Contras**:
- ⚠️ Requer configuração do secret `DATABASE_CONNECTION_STRING`
- ⚠️ Executado automaticamente em todo push na main

---

### 3. **Staging/Production Deploy** 🌐

**Método Atual**: Job automatizado no CI/CD ✅ **ATIVO**

As migrations são aplicadas automaticamente na branch `main` através do GitHub Actions.

**Pré-requisito:**
⚠️ **Configure o secret no GitHub:**
- Acesse: `Settings → Secrets and variables → Actions → New repository secret`
- Name: `DATABASE_CONNECTION_STRING`
- Value: Connection string de produção

**Fluxo automático:**
1. Push na branch `main`
2. Build & Test executado com sucesso
3. ✅ **Migrations aplicadas automaticamente** (job `apply-migrations`)
4. Aplicação está pronta para deploy

**Prós**:
- ✅ Totalmente automatizado
- ✅ Garante que o banco está atualizado antes do deploy
- ✅ Logs centralizados no GitHub Actions
- ✅ Falha o deploy se houver problemas nas migrations

**Contras**:
- ⚠️ Requer configuração do secret `DATABASE_CONNECTION_STRING`
- ⚠️ Pode causar downtime em migrations pesadas (considere janela de manutenção)

---

### 4. **Manual/SSH** 🖥️

**Método**: Executar comandos diretos (para casos especiais)

Se precisar aplicar migrations manualmente ou fazer rollback:

```bash
# Conecte via SSH ao servidor e execute:
cd src/Devlivery

# Aplicar migrations
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c ApplicationIdentityDbContext

# Rollback para uma migration específica
dotnet ef database update v001 -c ApplicationDbContext
```

**Quando usar:**
- 🔧 Rollback de migrations
- 🔧 Troubleshooting de problemas
- 🔧 Janela de manutenção programada

**Prós**:
- ✅ Controle total do processo
- ✅ Permite rollback fácil
- ✅ Ideal para janelas de manutenção

**Contras**:
- ❌ Processo manual
- ❌ Requer acesso SSH ao servidor
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

### Para Desenvolvimento Local
✅ **Use auto-migration** (padrão atual)
- Configurado automaticamente em `Startup.cs`
- Migrations são aplicadas no startup em modo Development
- Dados de seed também são aplicados automaticamente

### Para Produção
✅ **CI/CD Automatizado** (configuração atual)
- Migrations aplicadas automaticamente na branch `main`
- ⚠️ **Requer**: Secret `DATABASE_CONNECTION_STRING` configurado
- ✅ **Vantagem**: Zero intervenção manual, processo confiável

💡 **Alternativas para casos especiais:**
- Desabilitar o job `apply-migrations` no workflow
- Executar migrations manualmente via SSH em janela de manutenção
- Útil para migrations complexas que requerem monitoramento

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

## � Troubleshooting

### ❌ Erro: "dotnet ef not found"

```bash
# Instale o EF Core tools globalmente
dotnet tool install --global dotnet-ef
```

### ❌ Erro: "No DbContext was found"

Certifique-se de especificar o contexto:
```bash
dotnet ef database update -c ApplicationDbContext
```

### ❌ Erro: "Unable to create an object of type 'ApplicationDbContext'"

Verifique se a connection string está configurada:
```bash
# Para linha de comando
export ConnectionStrings__DefaultConnection="Host=localhost;Database=devlivery;..."

# Ou use o appsettings.Development.json
```

### ❌ Migrations falhando no CI/CD

1. Verifique se o secret `DATABASE_CONNECTION_STRING` está configurado corretamente
2. Teste a connection string localmente primeiro
3. Verifique os logs do GitHub Actions para detalhes do erro
4. Confirme que o banco de dados está acessível

## 📚 Referências

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Migration Bundles](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#bundles)
- [GitHub Actions Artifacts](https://docs.github.com/en/actions/using-workflows/storing-workflow-data-as-artifacts)
