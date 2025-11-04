# 🚀 Quick Start - Migrations

## 📋 Resumo Rápido

### Desenvolvimento Local

```bash
# 1. Criar migration
make migration-db VERSION=002

# 2. Aplicar migration
make migration-update-db

# OU aplicar todas de uma vez
make migration-apply-all
```

### CI/CD (✅ ATIVO E AUTOMATIZADO)

✅ **Migrations são aplicadas automaticamente na branch main!**

**Pré-requisito:**
⚠️ Configure o secret `DATABASE_CONNECTION_STRING` no GitHub antes do primeiro deploy!

**O workflow automaticamente:**
1. Instala EF Core tools
2. Compila e testa a aplicação
3. ✅ **Aplica migrations no banco de produção**
4. Gera e publica Docker image
5. Cria release

### Produção

#### ✅ Método Atual: Automatizado via CI/CD

Migrations são aplicadas automaticamente a cada push na `main`:

```yaml
# Configurado no workflow:
apply-migrations:
  name: Apply Database Migrations
  if: github.ref == 'refs/heads/main'  # Apenas main
```

**Configuração necessária:**
```bash
# GitHub → Settings → Secrets → New repository secret
Name: DATABASE_CONNECTION_STRING
Value: Host=prod-db;Port=5432;Database=devlivery;Username=...;Password=...
```

#### 🔧 Alternativa: Manual (Mais controle)

Se preferir controle manual, você pode:
1. Comentar o job `apply-migrations` no workflow
2. Executar via SSH:

```bash
# SSH no servidor
export DATABASE_CONNECTION_STRING="Host=prod-db;Database=devlivery;Username=user;Password=***"

# Executar script
./scripts/apply-migrations.sh

# OU comandos diretos
cd src/Devlivery.WebApi
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c ApplicationIdentityDbContext
```

---

## 🎨 Fluxo Visual

```
┌─────────────────────────────────────────────────────────────────┐
│                    DESENVOLVIMENTO LOCAL                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Código alterado → 2. make migration-db VERSION=002 →        │
│     3. make migration-update-db                                 │
│                                                                  │
│  ✅ Auto-apply no startup (Development mode)                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

                              ↓ git push origin main

┌─────────────────────────────────────────────────────────────────┐
│                       CI/CD PIPELINE                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. ✅ Build & Test                                             │
│  2. ✅ Apply Migrations (ApplicationDbContext)                  │
│  3. ✅ Apply Migrations (ApplicationIdentityDbContext)          │
│  4. ✅ Build & Push Docker image                                │
│  5. ✅ Create GitHub Release                                    │
│                                                                  │
│  ⚠️ Requer: DATABASE_CONNECTION_STRING configurado              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

                              ↓

┌─────────────────────────────────────────────────────────────────┐
│                          PRODUÇÃO                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ✅ Migrations já aplicadas pelo CI/CD                          │
│  ✅ Docker image atualizado disponível                          │
│  ✅ Release criado automaticamente                              │
│                                                                  │
│  Próximo passo: Deploy da nova imagem Docker                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔑 Pontos-Chave

### ✅ O que está configurado

- [x] Makefile com comandos facilitados
- [x] Scripts bash e PowerShell para aplicar migrations
- [x] Workflow CI/CD com build e deploy
- [x] ✅ **Job de migrations ATIVO** - aplica automaticamente na main
- [x] Auto-migration em Development

### ⚠️ O que você DEVE fazer antes do primeiro deploy

- [ ] **CRÍTICO**: Configurar secret `DATABASE_CONNECTION_STRING` no GitHub
  - Settings → Secrets and variables → Actions → New repository secret
  - Name: `DATABASE_CONNECTION_STRING`
  - Value: Connection string de produção

### ⚙️ Opções disponíveis

- ✅ **Atual**: Migrations automáticas via CI/CD (ativo)
- 🔧 **Alternativa**: Comentar o job e usar execução manual (mais controle)

---

## 📞 Troubleshooting

### ❌ "dotnet ef not found"

```bash
dotnet tool install --global dotnet-ef
```

### ❌ "Permission denied" nos scripts

```bash
chmod +x scripts/*.sh
```

### ❌ Connection string inválida

Verifique o formato:
```
Host=localhost;Port=5432;Database=devlivery;Username=postgres;Password=***
```

### ❌ Migrations falhando no CI/CD

1. Verifique se o secret `DATABASE_CONNECTION_STRING` está configurado
2. Teste a connection string localmente
3. Verifique os logs do GitHub Actions
4. Confirme que o banco de dados está acessível do runner

---

## 🎯 Próximos Passos

### 1. Configurar Secret (OBRIGATÓRIO)

```bash
# Acesse: GitHub → Settings → Secrets → New repository secret
Name: DATABASE_CONNECTION_STRING
Value: Host=your-prod-db;Port=5432;Database=devlivery;Username=user;Password=pass
```

### 2. Testar Localmente

```bash
# Criar uma migration de teste
make migration-db VERSION=002

# Aplicar localmente
make migration-update-db

# Verificar status
make migration-status
```

### 3. Push para Main

```bash
git add .
git commit -m "feat: configure automatic migrations"
git push origin main
```

### 4. Monitorar CI/CD

- Acesse GitHub Actions
- Veja o workflow executando
- Confirme que migrations foram aplicadas
- Verifique a criação da release

---

**📘 Documentação completa:** [docs/MIGRATIONS.md](./MIGRATIONS.md)
