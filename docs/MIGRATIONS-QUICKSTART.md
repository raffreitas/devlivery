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

**Pré-requisito OBRIGATÓRIO:**
⚠️ Configure o secret `DATABASE_CONNECTION_STRING` no GitHub antes do primeiro push:

```bash
GitHub → Settings → Secrets and variables → Actions → New repository secret
Name: DATABASE_CONNECTION_STRING
Value: Host=prod-db;Port=5432;Database=devlivery;Username=...;Password=...
```

**O workflow automaticamente:**
1. Build & Test da aplicação
2. ✅ **Aplica migrations em ambos os contextos** (ApplicationDbContext e ApplicationIdentityDbContext)
3. Migrations aplicadas com sucesso = deploy pode prosseguir

### Produção

#### ✅ Método Atual: Automatizado via CI/CD

Migrations são aplicadas automaticamente a cada push na branch `main`.

**Como funciona:**
- Job `apply-migrations` executa após build bem-sucedido
- Aplica migrations em ambos os contextos (ApplicationDbContext e ApplicationIdentityDbContext)
- Se migrations falharem, o workflow para e não prossegue

**⚠️ Importante:** Configure o secret `DATABASE_CONNECTION_STRING` antes do primeiro deploy!

#### 🔧 Alternativa: Execução Manual

Para casos especiais (rollback, troubleshooting, janela de manutenção):

```bash
# Conecte via SSH no servidor e execute:
cd src/Devlivery

# Aplicar migrations
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c ApplicationIdentityDbContext

# Fazer rollback para uma migration específica
dotnet ef database update v001 -c ApplicationDbContext
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
│  ✅ Seed de dados aplicado automaticamente                      │
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
│                                                                  │
│  ⚠️ Requer: SECRET DATABASE_CONNECTION_STRING configurado       │
│                                                                  │
│  Se migrations falharem → workflow PARA aqui                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

                              ↓

┌─────────────────────────────────────────────────────────────────┐
│                          PRODUÇÃO                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ✅ Banco de dados atualizado com últimas migrations            │
│  ✅ Aplicação pronta para deployment                            │
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

### ⚠️ CRÍTICO: Configure antes do primeiro deploy

- [ ] **OBRIGATÓRIO**: Configurar secret `DATABASE_CONNECTION_STRING` no GitHub
  ```
  GitHub → Settings → Secrets and variables → Actions → New repository secret
  Name: DATABASE_CONNECTION_STRING
  Value: Host=prod-db;Port=5432;Database=devlivery;Username=...;Password=...
  ```

> **⚠️ Sem este secret configurado, o job de migrations falhará!**

### ⚙️ Configuração Atual

- ✅ **Migrations automáticas ATIVAS** via CI/CD
- ✅ Executadas em todo push na branch `main`
- ✅ Aplicadas ANTES da aplicação ser deployada
- 🔧 **Alternativa**: Execução manual via SSH (para casos especiais)

---

## 📞 Troubleshooting Comum

### ❌ "dotnet ef not found"

```bash
# Instale o EF Core tools globalmente
dotnet tool install --global dotnet-ef
```

### ❌ "No DbContext was found"

```bash
# Sempre especifique o contexto
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c ApplicationIdentityDbContext
```

### ❌ Connection string inválida

Formato correto:
```
Host=localhost;Port=5432;Database=devlivery;Username=postgres;Password=sua_senha
```

### ❌ Migrations falhando no CI/CD

**Checklist:**
1. ✅ Secret `DATABASE_CONNECTION_STRING` está configurado no GitHub?
2. ✅ Connection string está correta?
3. ✅ Banco de dados está acessível?
4. 📋 Verifique os logs detalhados no GitHub Actions

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
