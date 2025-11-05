# 🚀 Guia de Deploy no Railway

Este documento explica as duas arquiteturas possíveis para deploy da aplicação devlivery no Railway.

## 📋 Arquiteturas Disponíveis

### **Opção 1: Nginx como Reverse Proxy (RECOMENDADA)** ⭐

#### Arquitetura
```
Internet
   │
   ▼
┌──────────────────────────────┐
│   NGINX Container (Público)  │
│   Port 8080                  │
│                              │
│  ┌────────────────────────┐  │
│  │  /        → Frontend   │  │
│  │  /api/*   → Backend    │  │
│  └────────────────────────┘  │
└──────────┬───────────────────┘
           │
      ┌────┴─────┐
      │          │
  [Static]  [ASP.NET API]
  [Files]   (Private - Railway Network)
             │
        [PostgreSQL]
        (Private)
```

#### Vantagens
- ✅ **Backend 100% privado** (não exposto à internet)
- ✅ **Sem CORS** (mesma origem para frontend e API)
- ✅ **Rate limiting centralizado** no Nginx
- ✅ **SSL termination** no proxy
- ✅ **Um único domínio público**
- ✅ **Maior segurança** (camada de proteção adicional)
- ✅ **Logs centralizados**

#### Como Configurar no Railway

##### 1. Criar Projeto e Serviços

```
Projeto: devlivery-system

Service 1: devlivery-db
  Type: PostgreSQL
  Private Network: ✓ Enabled

Service 2: devlivery-backend
  Source: GitHub repo backend
  Private Network: ✓ Enabled
  Generate Domain: ✗ DISABLED (não precisa ser público)
  Env Vars:
    - DATABASE_URL=${{devlivery-db.DATABASE_URL}}
    - ASPNETCORE_URLS=http://0.0.0.0:5000
    - ASPNETCORE_ENVIRONMENT=Production

Service 3: devlivery-webapp
  Source: GitHub repo frontend
  Public Domain: ✓ Enabled
  Env Vars:
    - VITE_API_URL=/api
    - BACKEND_URL=http://devlivery-backend.railway.internal:5000
```

##### 2. Configuração de Rede Privada

O Railway cria URLs internas automaticamente:
- `http://devlivery-backend.railway.internal:5000`

Isso permite que o Nginx se comunique com o backend SEM expô-lo publicamente.

##### 3. Variáveis de Ambiente

**Frontend (devlivery-webapp):**
```env
VITE_API_URL=/api
BACKEND_URL=http://devlivery-backend.railway.internal:5000
```

**Backend (devlivery-backend):**
```env
DATABASE_URL=${{devlivery-db.DATABASE_URL}}
ASPNETCORE_URLS=http://0.0.0.0:5000
ASPNETCORE_ENVIRONMENT=Production
```

#### Alterações no Código Frontend

**Arquivo `.env.local` (desenvolvimento):**
```env
VITE_API_URL=http://localhost:5000/api
```

**Arquivo `.env.production` (produção - criar este arquivo):**
```env
VITE_API_URL=/api
```

O Vite automaticamente usa o arquivo correto baseado no ambiente.

---

### **Opção 2: Backend Público (Arquitetura Tradicional)**

#### Arquitetura
```
Internet
   │
   ├────────────┬──────────────┐
   │            │              │
[NGINX]    [ASP.NET API]  [PostgreSQL]
(Public)   (Public CORS)  (Private)
```

#### Vantagens
- ✅ Mais simples de configurar
- ✅ Backend pode ser usado por outros clientes (mobile)
- ✅ Escalabilidade independente

#### Desvantagens
- ❌ Backend exposto à internet
- ❌ Precisa configurar CORS
- ❌ Dois domínios públicos

#### Como Configurar

```
Service 1: devlivery-db
  Type: PostgreSQL
  Private Network: ✓ Enabled

Service 2: devlivery-backend
  Source: GitHub repo backend
  Public Domain: ✓ Enabled
  Env Vars:
    - DATABASE_URL=${{devlivery-db.DATABASE_URL}}
    - ALLOWED_ORIGINS=https://devlivery-webapp.railway.app
    - ASPNETCORE_URLS=http://0.0.0.0:5000

Service 3: devlivery-webapp
  Source: GitHub repo frontend
  Public Domain: ✓ Enabled
  Env Vars:
    - VITE_API_URL=https://devlivery-backend.railway.app
```

#### CORS no Backend (ASP.NET)

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
            ?? "http://localhost:5173";

        policy.WithOrigins(origins.Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors();
```

---

## 🔐 Considerações de Segurança

### Opção 1 (Reverse Proxy)
- Backend completamente isolado
- Rate limiting no Nginx
- WAF possível no futuro
- Logs centralizados
- Headers de segurança no proxy

### Opção 2 (Backend Público)
- CORS bem configurado
- Rate limiting no ASP.NET
- JWT authentication (já implementado)
- HTTPS (Railway fornece)
- Input validation

---

## 📊 Comparação

| Aspecto | Opção 1 (Proxy) | Opção 2 (Público) |
|---------|----------------|-------------------|
| Segurança | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| Complexidade | ⭐⭐⭐ | ⭐⭐ |
| Performance | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Escalabilidade | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Multi-plataforma | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Custo Railway | $ | $$ |

---

## 🎯 Recomendação Final

**Use a Opção 1 (Nginx Reverse Proxy)** se:
- Segurança é prioridade máxima
- Aplicação web é o único cliente
- Quer rate limiting robusto
- Quer um único domínio público

**Use a Opção 2 (Backend Público)** se:
- Vai ter app mobile no futuro
- Quer escalabilidade independente
- Precisa de simplicidade inicial
- Backend precisa ser acessado por múltiplos clientes

---

## 🧪 Testar Localmente (Opção 1)

### 1. Build do Frontend
```bash
pnpm build
```

### 2. Rodar Backend Local
```bash
cd ../devlivery-backend
dotnet run
```

### 3. Build e Rodar Docker com Proxy
```bash
docker build -t devlivery-webapp .
docker run -p 8080:8080 \
  -e BACKEND_URL=http://host.docker.internal:5000 \
  devlivery-webapp
```

### 4. Acessar
```
http://localhost:8080
```

As requisições para `/api/*` serão automaticamente redirecionadas para `http://host.docker.internal:5000/api/*`

---

## 📝 Checklist de Deploy

- [ ] Criar projeto no Railway
- [ ] Adicionar PostgreSQL
- [ ] Adicionar serviço backend (configurar private network)
- [ ] Adicionar serviço frontend
- [ ] Configurar variáveis de ambiente
- [ ] Testar conectividade entre serviços
- [ ] Configurar domínio customizado (opcional)
- [ ] Monitorar logs e performance

---

## 🆘 Troubleshooting

### Backend não conecta ao banco
- Verifique `DATABASE_URL` está referenciando corretamente
- Verifique private network está habilitado

### Nginx não encontra backend
- Verifique `BACKEND_URL` usa `.railway.internal`
- Verifique backend está rodando (logs)
- Porta do backend deve ser 5000 ou a configurada

### Frontend não carrega
- Verifique build completou com sucesso
- Verifique `VITE_API_URL=/api` está configurado
- Limpe cache do browser

### Erro 502 Bad Gateway
- Backend pode estar down
- Health check endpoint `/health` no backend
- Verifique logs do Nginx e do backend
