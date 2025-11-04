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

### CI/CD (Atual)

✅ **Já configurado!** O workflow automaticamente:
1. Instala EF Core tools
2. Gera migration bundles
3. Faz upload como artifact no GitHub Actions

### Produção (Opções)

#### Opção 1: Manual (Recomendado) 🎯

```bash
# 1. Baixar bundles do GitHub Release
# 2. Executar no servidor:
./efbundle-db --connection "sua-connection-string"
./efbundle-identity --connection "sua-connection-string"
```

#### Opção 2: Automatizado via CI/CD

Descomente o job `apply-migrations` no workflow e configure os secrets:
- `DATABASE_CONNECTION_STRING`
- `IDENTITY_CONNECTION_STRING`

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

                              ↓ git push

┌─────────────────────────────────────────────────────────────────┐
│                       CI/CD PIPELINE                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Build & Test                                                │
│  2. dotnet tool install dotnet-ef                               │
│  3. generate-migration-bundle.sh                                │
│     ├─ efbundle-db (ApplicationDbContext)                       │
│     └─ efbundle-identity (ApplicationIdentityDbContext)         │
│  4. Upload artifacts                                            │
│  5. Build & Push Docker image                                   │
│  6. Create GitHub Release                                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

                              ↓

┌─────────────────────────────────────────────────────────────────┐
│                     PRODUÇÃO (Escolha 1)                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  📦 Download manual dos bundles                                 │
│                                                                  │
│  ./efbundle-db --connection "..."                               │
│  ./efbundle-identity --connection "..."                         │
│                                                                  │
│  ✅ Controle total, executar em janela de manutenção           │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

                              OU

┌─────────────────────────────────────────────────────────────────┐
│                     PRODUÇÃO (Escolha 2)                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  🤖 Job automatizado no GitHub Actions                          │
│                                                                  │
│  - Usa secrets configurados                                     │
│  - Aplica migrations automaticamente                            │
│  - Antes do deploy da aplicação                                 │
│                                                                  │
│  ⚠️ Requer configuração de secrets                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔑 Pontos-Chave

### ✅ O que está pronto

- [x] Makefile com comandos facilitados
- [x] Scripts bash e PowerShell para aplicar migrations
- [x] Workflow CI/CD gerando migration bundles
- [x] Bundles disponíveis como artifacts
- [x] Auto-migration em Development

### ⚙️ O que você precisa decidir

- [ ] **Produção**: Manual ou automatizado?
- [ ] **Secrets**: Configurar connection strings (se automatizado)
- [ ] **Downtime**: Planejar janela de manutenção (se necessário)

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

---

**📘 Documentação completa:** [docs/MIGRATIONS.md](./MIGRATIONS.md)
