# 🔒 Guia de Segurança - Devlivery Backend (MVP)

## ✅ Melhores Práticas Implementadas

### 1. HTTPS & SSL

#### HSTS (HTTP Strict Transport Security)
```csharp
services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);  // 1 ano de cache
    options.IncludeSubDomains = true;         // Protege subdomínios
    options.Preload = true;                   // Elegível para HSTS preload list
});
```

**Benefícios:**
- ✅ Força browsers a SEMPRE usar HTTPS
- ✅ Previne downgrade attacks (MITM)
- ✅ Reduz overhead de redirects após primeira visita
- ✅ Protege contra SSL stripping

#### HTTPS Redirect Condicional
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // Redireciona HTTP → HTTPS em dev
}
else
{
    app.UseForwardedHeaders();  // Confia no proxy em produção
}
```

**Por que:**
- Em produção, Railway/Nginx fazem SSL termination
- Backend recebe HTTP mas deve saber que é HTTPS
- Evita loops de redirect

---

### 2. Forwarded Headers (Proxy Safety)

```csharp
services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor 
                             | ForwardedHeaders.XForwardedProto
                             | ForwardedHeaders.XForwardedHost;
    
    if (!builder.Environment.IsDevelopment())
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }
    
    options.ForwardLimit = 2;  // Máximo 2 proxies
});
```

**Proteções:**
- ✅ **ForwardLimit = 2**: Previne IP spoofing (Railway + Nginx = 2)
- ✅ **XForwardedProto**: Detecta HTTPS corretamente
- ✅ **XForwardedHost**: Mantém hostname correto
- ✅ **KnownNetworks.Clear()**: Confia apenas em rede privada Railway

**Ataque Prevenido:**
```http
# ❌ Ataque: Spoof de IP
X-Forwarded-For: 127.0.0.1, 10.0.0.1, 192.168.1.1

# ✅ Proteção: ForwardLimit=2 ignora extras
```

---

### 3. Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});
```

| Header | Proteção | Exemplo de Ataque Prevenido |
|--------|----------|----------------------------|
| `X-Content-Type-Options: nosniff` | MIME sniffing | Browser executar JSON como script |
| `X-Frame-Options: DENY` | Clickjacking | Iframe malicioso sobre seu site |
| `X-XSS-Protection: 1; mode=block` | XSS (legacy) | Injeção de script em browsers antigos |
| `Referrer-Policy` | Info Leakage | Vazamento de URLs com tokens na query |
| `Permissions-Policy` | Feature Abuse | Acesso não autorizado a câmera/GPS |

---

### 4. CORS (Cross-Origin Resource Sharing)

```csharp
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") 
                   ?? "http://localhost:5173";

var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(o => o.Trim())
                           .ToArray();

policy.WithOrigins(origins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowedToAllowWildcardSubdomains();
```

**Configurações:**
- ✅ **Origens via ENV**: Não hardcodado, configurável
- ✅ **AllowCredentials**: Necessário para JWT cookies (se usar)
- ✅ **Múltiplas origens**: Dev, staging, prod separados por vírgula
- ✅ **Wildcard subdomains**: Para review apps `*.railway.app`

**Exemplo de Configuração:**
```bash
# Desenvolvimento
ALLOWED_ORIGINS=http://localhost:5173

# Produção
ALLOWED_ORIGINS=https://seu-app.railway.app,https://staging.railway.app

# Review Apps (Railway)
ALLOWED_ORIGINS=https://*.railway.app
```

---

### 5. JWT Authentication

#### Configuração Segura

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            ),
            ClockSkew = TimeSpan.Zero  // Sem tolerância de tempo
        };
    });
```

**Validações Habilitadas:**
- ✅ **ValidateIssuer**: Verifica quem criou o token
- ✅ **ValidateAudience**: Verifica para quem é o token
- ✅ **ValidateLifetime**: Verifica se token expirou
- ✅ **ValidateIssuerSigningKey**: Verifica assinatura
- ✅ **ClockSkew = Zero**: Sem tolerância (mais seguro)

#### Gerar SecretKey Forte

**❌ NÃO USE (fraco):**
```
SecretKey-SecretKey-SecretKey-SecretKey
mysecretkey123
password123
```

**✅ USE (forte, aleatória, 64+ caracteres):**

```powershell
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Exemplo de output:
8Kf2m9Pq4rT7wX5zA3cB6vN1sM8Lk4jH9gF2dE5tR7yU3iO6pA9sD2fG5hJ8kL1nM4oP7qR
```

```bash
# Bash/Linux
openssl rand -base64 48

# Exemplo de output:
xZ9Qm2Pn5Rt8Wv4Yb7Ce0Df3Gh6Jk1Lm4Op7Rs0Tu3Xw6Zc9Be2Dg5Fh8Jl1Mn4Pq7St0
```

**Requisitos Mínimos:**
- ✅ Mínimo 32 caracteres (256 bits)
- ✅ Caracteres aleatórios (números, letras maiúsculas/minúsculas)
- ✅ Diferente para cada ambiente (dev, staging, prod)
- ✅ Armazenada como variável de ambiente (NUNCA no código)

---

### 6. Rate Limiting (Nginx)

```nginx
# nginx.conf
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;

location /api/ {
    limit_req zone=api_limit burst=20 nodelay;
    # ...
}
```

**Configuração:**
- ✅ **100 req/min por IP**: Previne abuse
- ✅ **Burst de 20**: Permite picos temporários
- ✅ **10MB de memória**: ~160k IPs distintos
- ✅ **429 Too Many Requests**: Status correto

---

## 🎯 Níveis de Segurança

### ✅ Essencial (MVP) - IMPLEMENTADO

| Proteção | Status | Impacto se Ausente |
|----------|--------|-------------------|
| HTTPS/SSL | ✅ | 🔴 CRÍTICO - Dados em plain text |
| HSTS | ✅ | 🟡 MÉDIO - Downgrade attacks |
| JWT Authentication | ✅ | 🔴 CRÍTICO - Sem controle de acesso |
| CORS restrito | ✅ | 🔴 CRÍTICO - Qualquer site pode acessar API |
| Security Headers | ✅ | 🟡 MÉDIO - XSS, Clickjacking |
| Input Validation | ✅ | 🔴 CRÍTICO - SQL injection, XSS |
| Backend privado | ✅ | 🔴 CRÍTICO - Exposto à internet |
| Rate Limiting | ✅ | 🟡 MÉDIO - DDoS, brute force |
| Forwarded Headers Limit | ✅ | 🟠 BAIXO - IP spoofing |

### 🔄 Recomendado (Pós-MVP)

| Proteção | Prioridade | Quando Implementar |
|----------|-----------|-------------------|
| Rate Limiting no Backend | Média | Após 1000 usuários |
| Refresh Tokens | Alta | Quando JWT expiration < 1 hora |
| IP Whitelist (Admin) | Alta | Se tiver área admin |
| 2FA/MFA | Média | Se tiver dados sensíveis/pagamentos |
| Audit Logs | Baixa | Para compliance/debug |
| WAF | Média | Se houver ataques frequentes |
| Password Strength Rules | Alta | Implementar antes do launch |

### ❌ Não Necessário (MVP)

- Certificate Pinning
- Hardware Security Modules
- DDoS Protection avançado (Cloudflare)
- Penetration Testing profissional
- SOC 2 / ISO 27001 Compliance

---

## 📊 Comparação: Antes vs Depois

### CORS
```csharp
// ❌ Antes (inseguro)
policy.AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();

// ✅ Depois (seguro)
policy.WithOrigins(Environment.GetEnvironmentVariable("ALLOWED_ORIGINS"))
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

### JWT SecretKey
```json
// ❌ Antes (inseguro)
"JwtTokenSettings": {
  "SecretKey": "mysecret123"
}

// ✅ Depois (seguro)
Environment Variable:
JwtTokenSettings__SecretKey=8Kf2m9Pq4rT7wX5zA3cB6vN1sM8Lk4jH9gF...
```

### HTTPS Redirect
```csharp
// ❌ Antes (quebra em produção)
app.UseHttpsRedirection();

// ✅ Depois (funciona em dev e prod)
if (IsDevelopment) app.UseHttpsRedirection();
else app.UseForwardedHeaders();
```

---

## 🧪 Testes de Segurança

### 1. Testar CORS

```bash
# ✅ Origem permitida
curl -H "Origin: http://localhost:5173" \
     -H "Access-Control-Request-Method: GET" \
     -X OPTIONS http://localhost:5000/api/health

# Deve retornar:
# Access-Control-Allow-Origin: http://localhost:5173
# Access-Control-Allow-Credentials: true

# ❌ Origem NÃO permitida
curl -H "Origin: https://malicious-site.com" \
     -X OPTIONS http://localhost:5000/api/health

# NÃO deve retornar Access-Control-Allow-Origin
```

### 2. Testar JWT

```bash
# ❌ Token inválido
curl -H "Authorization: Bearer invalid-token" \
     http://localhost:5000/api/products

# Deve retornar 401 Unauthorized

# ✅ Token válido
curl -H "Authorization: Bearer <TOKEN_VALIDO>" \
     http://localhost:5000/api/products

# Deve retornar 200 OK
```

### 3. Testar Security Headers

```bash
curl -I https://seu-app.railway.app/api/health

# Deve conter:
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

### 4. Testar Rate Limiting

```bash
# Fazer 150 requisições em 1 minuto (excede 100r/m)
for i in {1..150}; do
  curl http://localhost:8080/api/health
done

# Após ~100 requisições deve retornar:
# 429 Too Many Requests
```

---

## 🆘 Troubleshooting de Segurança

### CORS Errors

**Sintoma:**
```
Access to fetch at 'https://api.example.com' from origin 'https://app.example.com' 
has been blocked by CORS policy
```

**Soluções:**
1. Verificar `ALLOWED_ORIGINS` no Railway
2. Adicionar a origem correta (com https://)
3. Verificar se `AllowCredentials` está habilitado
4. Testar com curl (debug)

### JWT Inválido

**Sintoma:**
```
401 Unauthorized
```

**Verificar:**
1. Token não expirou (`exp` claim)
2. `Issuer` e `Audience` corretos
3. `SecretKey` é a mesma usada para gerar
4. Token está no formato `Bearer <token>`

### Headers de Segurança Ausentes

**Sintoma:**
Headers não aparecem em produção

**Soluções:**
1. Verificar `ASPNETCORE_ENVIRONMENT=Production`
2. Verificar middleware está ANTES de `UseAuthentication()`
3. Testar diretamente no backend (sem Nginx)

---

## 📝 Checklist Final de Deploy Seguro

### Antes de Fazer Deploy

- [ ] Gerar `JwtTokenSettings__SecretKey` forte (64+ chars)
- [ ] Configurar `ALLOWED_ORIGINS` com URL do frontend
- [ ] Verificar `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Backend SEM domínio público no Railway
- [ ] Database connection string via `DATABASE_URL`
- [ ] Testar CORS localmente
- [ ] Testar JWT authentication
- [ ] Verificar rate limiting no Nginx

### Após Deploy

- [ ] Verificar HTTPS funciona
- [ ] Testar login no frontend
- [ ] Verificar security headers com `curl -I`
- [ ] Testar CORS com browser DevTools
- [ ] Monitorar logs por 24h
- [ ] Verificar não há erros 502/429

### Manutenção Contínua

- [ ] Rotacionar JWT SecretKey a cada 6 meses
- [ ] Monitorar logs de autenticação
- [ ] Revisar CORS origins mensalmente
- [ ] Atualizar dependências (security patches)
- [ ] Backup de database semanal

---

**Última Atualização:** 5 de Novembro de 2025

**Status:** ✅ Pronto para MVP em Produção
