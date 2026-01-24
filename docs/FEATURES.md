# 📖 Guia de Merge de Features para o Monorepo

Este guia descreve como fazer merge das branches de feature dos repositórios separados para o monorepo quando chegar a janela de release.

## 📋 Branches Pendentes de Merge

As seguintes branches estão aguardando merge:

### Backend API
- `webapi/feature/0.5.1` - Feature da versão 0.5.1

### Frontend Web
- `webapp/feature/0.5.1` - Feature da versão 0.5.1
- `webapp/feature/ui-fixes` - Correções de UI

---

## 🚀 Procedimento de Merge

### Pré-requisitos
- Git instalado e configurado
- Acesso aos repositórios
- Estar no diretório do monorepo: `D:\projects\devlivery\devlivery`

### Opção 1: Merge via `git subtree pull` (RECOMENDADO)

Esta é a forma mais simples e mantém o histórico limpo usando squash.

#### Passo 1: Preparar o Monorepo
```bash
cd D:\projects\devlivery\devlivery

# Buscar as atualizações dos remotes
git fetch webapi
git fetch webapp

# Verificar que você está na branch main
git checkout main
```

#### Passo 2: Fazer Merge da API
```bash
git subtree pull --prefix app/api webapi feature/0.5.1 --squash
```

**O que acontece:**
- Busca a branch `feature/0.5.1` do remote `webapi`
- Merge com squash (combina todos os commits em um único commit)
- Coloca as mudanças sob `app/api/`

#### Passo 3: Fazer Merge do Web
```bash
# Feature 0.5.1
git subtree pull --prefix app/web webapp feature/0.5.1 --squash

# Feature ui-fixes
git subtree pull --prefix app/web webapp feature/ui-fixes --squash
```

#### Passo 4: Revisar as Mudanças
```bash
# Ver o histórico de commits
git log --oneline -15

# Ver quais arquivos foram alterados
git status

# Ver o diff detalhado (opcional)
git diff origin/main HEAD
```

#### Passo 5: Fazer Push
```bash
# Enviar todas as mudanças para o repositório principal
git push origin main
```

---

## ⚠️ Se Houver Conflitos

Se durante o merge ocorrerem conflitos, siga estes passos:

### 1. Identifique os Conflitos
```bash
git status
```

Você verá arquivos com status "both modified" ou "both added".

### 2. Resolva os Conflitos
- Abra cada arquivo com conflito
- Procure pelas marcações de conflito:
  ```
  <<<<<<< HEAD
  seu código
  =======
  código da feature
  >>>>>>> branch-name
  ```
- Edite e mantenha o código correto
- Remova as marcações de conflito

### 3. Finalize o Merge
```bash
# Adicionar os arquivos resolvidos
git add .

# Criar um commit de merge
git commit -m "merge: Resolver conflitos da feature 0.5.1"

# Fazer push
git push origin main
```

---

## 📊 Alternativa: Merge Manual (Se Preferir)

Se preferir ter mais controle e ver as branches criadas localmente:

### Passo 1: Criar Branches Locais
```bash
# Buscar updates
git fetch webapi
git fetch webapp

# Criar branches locais das features
git checkout -b feature/api-0.5.1 webapi/feature/0.5.1
git checkout -b feature/web-0.5.1 webapp/feature/0.5.1
git checkout -b feature/ui-fixes webapp/feature/ui-fixes
```

### Passo 2: Fazer Merge em Main
```bash
# Voltar para main
git checkout main

# Fazer merge da API
git merge feature/api-0.5.1

# Fazer merge do Web
git merge feature/web-0.5.1
git merge feature/ui-fixes

# Se houver conflitos, resolva conforme descrito acima
```

### Passo 3: Limpar Branches Locais (Opcional)
```bash
git branch -d feature/api-0.5.1
git branch -d feature/web-0.5.1
git branch -d feature/ui-fixes
```

### Passo 4: Fazer Push
```bash
git push origin main
```

---

## ✅ Verificação Pós-Merge

Após fazer o merge, verifique se tudo está correto:

```bash
# Verificar o log
git log --oneline -10

# Verificar que a estrutura está intacta
ls -la app/api/
ls -la app/web/

# Verificar se há mudanças não commitadas
git status
```

Você deve ver:
- Todos os commits do merge visíveis no log
- Diretórios `app/api/` e `app/web/` com as mudanças
- `git status` mostrando "nothing to commit"

---

## 🔍 Dicas e Boas Práticas

### ✅ Faça
- ✅ Fazer fetch antes de começar
- ✅ Revisar as mudanças antes de fazer push
- ✅ Usar squash para manter histórico limpo
- ✅ Testar depois do merge (build, testes)

### ❌ Evite
- ❌ Fazer merge sem fetch atualizado
- ❌ Fazer push sem revisar as mudanças
- ❌ Pular a resolução de conflitos
- ❌ Fazer merge para main sem testes

---

## 🆘 Troubleshooting

### Problema: "fatal: ambiguous argument"
```bash
# Solução: Fazer fetch primeiro
git fetch webapi
git fetch webapp
```

### Problema: Conflitos complexos
```bash
# Para abortar o merge e começar novamente
git merge --abort
# ou
git subtree pull --abort
```

### Problema: Precisa voltar o merge
```bash
# Desfazer o último commit
git reset --hard HEAD~1

# Ou se já fez push
git revert HEAD
git push origin main
```

---

## 📞 Checklist Final

Antes de fazer push, confirme:

- [ ] Fez `git fetch webapi` e `git fetch webapp`
- [ ] Está na branch `main`
- [ ] Fez o merge de todas as features necessárias
- [ ] Resolveu todos os conflitos (se houver)
- [ ] Revisou as mudanças com `git log` e `git status`
- [ ] Testou o merge localmente (build, testes)
- [ ] Fez `git push origin main`

---

## 📝 Histórico de Merges

Registre aqui quando fizer os merges:

| Data | Feature | Status | Notas |
|------|---------|--------|-------|
| | webapi/feature/0.5.1 | Pendente | |
| | webapp/feature/0.5.1 | Pendente | |
| | webapp/feature/ui-fixes | Pendente | |

---

## 🚀 Próximas Etapas Após Merge

Depois de fazer o merge com sucesso:

1. **Testar** - Executar testes completos da API e Web
2. **Build** - Compilar a API e fazer build do Web
3. **Deploy** - Se necessário, fazer deploy em staging/produção
4. **Documentação** - Atualizar docs se houver breaking changes
5. **Release Notes** - Criar notas de release com as mudanças

---

**Quando chegar a hora, é só seguir este guia! 🎯**