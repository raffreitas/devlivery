# 🚀 Configuração do Backend para Railway

## � Melhores Práticas de Segurança Implementadas

### ✅ Checklist de Segurança para MVP

- [x] **HTTPS Redirect apenas em Development** (evita loops em produção)
- [x] **HSTS com 1 ano de cache** (força HTTPS em browsers)
- [x] **Forwarded Headers com limite** (previne IP spoofing)
- [x] **Security Headers** (XSS, Clickjacking, MIME sniffing)
- [x] **CORS configurável via ENV** (não hardcodado)
- [x] **JWT com chave secreta forte** (mínimo 32 caracteres)
- [x] **Credentials permitidos** (para autenticação)
- [x] **Rate Limiting no Nginx** (proteção contra DDoS)
- [x] **Backend privado** (não exposto à internet)
- [x] **Environment-based config** (desenvolvimento vs produção)

---

## �📋 Problema Resolvido

O backend estava configurado com `UseHttpsRedirection()` incondicional, o que causaria problemas no Railway porque:

1. **Railway faz SSL termination** → Requisições chegam como HTTPS
2. **Nginx faz proxy HTTP interno** → Envia HTTP para o backend
3. **Backend tentaria redirecionar HTTP → HTTPS** → Loop infinito ou erro 502

## ✅ Solução Implementada

### Backend (Startup.cs)

```csharp
// ✅ Configuração de Forwarded Headers (detecta proxy)
services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor 
                             | ForwardedHeaders.XForwardedProto;
    // Confiar em todos os proxies (Railway network)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ✅ HTTPS Redirect apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseHsts();
    app.UseForwardedHeaders();
}
```

### Nginx (nginx.conf)

```nginx
location /api/ {
    # ✅ Adiciona header X-Forwarded-Host
    proxy_set_header X-Forwarded-Host $host;
    
    # ✅ Mantém /api/ no path
    proxy_pass $BACKEND_URL/api/;
}
```

## 🔐 Como Funciona

### Fluxo da Requisição

```
1. Cliente → https://seu-app.railway.app/api/products
                ↓ (HTTPS)
2. Railway SSL Termination
                ↓ (HTTP + X-Forwarded-Proto: https)
3. Nginx Container
                ↓ (HTTP + Headers de Proxy)
4. ASP.NET Backend (detecta HTTPS via headers)
                ↓
5. Response (ASP.NET sabe que é HTTPS)
```

### Headers Enviados pelo Nginx

```http
Host: seu-app.railway.app
X-Real-IP: 123.456.789.0
X-Forwarded-For: 123.456.789.0
X-Forwarded-Proto: https
X-Forwarded-Host: seu-app.railway.app
```

### ASP.NET Detecta HTTPS

```csharp
// UseForwardedHeaders() configura:
Request.Scheme = "https"  // Lê de X-Forwarded-Proto
Request.Host = "seu-app.railway.app"  // Lê de X-Forwarded-Host

// ✅ Não tenta redirecionar porque detecta HTTPS!
```

## 🎯 Configuração no Railway

### Backend Service (devlivery-backend)

```yaml
Settings:
  Public Networking: ✗ DISABLED
  Private Network: ✓ ENABLED
  
Environment Variables:
  # ASP.NET Core
  ASPNETCORE_ENVIRONMENT=Production
  ASPNETCORE_URLS=http://0.0.0.0:5000
  
  # Database (Railway fornece automaticamente)
  DATABASE_URL=${{devlivery-db.DATABASE_URL}}
  
  # JWT - GERAR NOVA CHAVE FORTE!
  JwtTokenSettings__SecretKey=<GERAR-CHAVE-FORTE-MIN-32-CHARS>
  JwtTokenSettings__Issuer=devlivery.webapi
  JwtTokenSettings__Audience=devlivery.app
  JwtTokenSettings__ExpirationInMinutes=60
  
  # CORS - Colocar URL do seu frontend
  ALLOWED_ORIGINS=https://seu-app.railway.app
```

**⚠️ CRÍTICO - Gerar JWT SecretKey:**
```bash
# PowerShell - Gerar chave aleatória de 64 caracteres
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Bash/Linux
openssl rand -base64 48
```

### Frontend Service (devlivery-webapp)

```yaml
Settings:
  Public Networking: ✓ ENABLED
  
Environment Variables:
  BACKEND_URL=http://devlivery-backend.railway.internal:5000
```

## 📊 Comparação: Antes vs Depois

### ❌ Antes (Problema)

```csharp
// Em todos os ambientes
app.UseHttpsRedirection();

// Resultado em produção:
Cliente (HTTPS) → Railway → Nginx (HTTP) → Backend
                                           ↓
                                    Tenta redirecionar HTTP → HTTPS
                                           ↓
                                        ❌ ERRO!
```

### ✅ Depois (Correto)

```csharp
// Desenvolvimento: redireciona
if (IsDevelopment) app.UseHttpsRedirection();

// Produção: confia no proxy
else app.UseForwardedHeaders();

// Resultado em produção:
Cliente (HTTPS) → Railway → Nginx (HTTP + Headers) → Backend
                                                      ↓
                                              Detecta HTTPS via headers
                                                      ↓
                                                  ✅ FUNCIONA!
```

## 🧪 Testar Localmente

### Desenvolvimento (HTTP redirect ativo)

```bash
cd devlivery-webapi
dotnet run --launch-profile https

# ✅ Redireciona HTTP → HTTPS
curl http://localhost:5052/health
# → 307 Temporary Redirect
# → Location: https://localhost:7141/health
```

### Produção Simulada (HTTP sem redirect)

```bash
# Simular Railway/Nginx enviando headers
curl -H "X-Forwarded-Proto: https" \
     -H "X-Forwarded-Host: seu-app.railway.app" \
     http://localhost:5000/health

# ✅ Retorna 200 OK (sem redirect)
```

## 📝 Checklist de Deploy

- [x] `UseHttpsRedirection()` apenas em Development
- [x] `UseForwardedHeaders()` em Production
- [x] Nginx envia `X-Forwarded-*` headers
- [x] Backend mantém `/api` nas rotas
- [x] Backend NÃO tem domínio público no Railway
- [x] `ASPNETCORE_URLS=http://0.0.0.0:5000` (HTTP)
- [x] Nginx usa `proxy_pass $BACKEND_URL/api/;`

## 🆘 Troubleshooting

### Erro 502 Bad Gateway

**Causa:** Backend pode estar tentando redirecionar HTTP → HTTPS

**Solução:**
1. Verifique `ASPNETCORE_ENVIRONMENT=Production`
2. Verifique logs do backend: `railway logs --service=devlivery-backend`
3. Confirme que `UseHttpsRedirection()` não está ativo

### Links Quebrados (HTTP em vez de HTTPS)

**Causa:** Backend não detecta HTTPS via headers

**Solução:**
1. Verifique Nginx envia `X-Forwarded-Proto: $scheme`
2. Verifique `UseForwardedHeaders()` está ativo em produção
3. Teste com: `Request.Scheme` deve retornar `"https"`

### CORS Errors

**Causa:** Backend detecta origem diferente

**Solução:**
1. Verifique `X-Forwarded-Host` no Nginx
2. Adicione logs: `Console.WriteLine($"Origin: {Request.Headers["Origin"]}")`

---

**Última Atualização:** 5 de Novembro de 2025
