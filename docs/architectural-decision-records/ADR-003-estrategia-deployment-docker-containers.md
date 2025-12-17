# Estratégia de Deployment via Docker e Containers

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** Empacotamento e Deployment da Aplicação

## Contexto e Problema

Aplicações .NET podem ser deployadas de diversas formas: diretamente no Windows Server com IIS, como self-contained executables, em VMs com Kestrel, ou em containers Docker. A escolha afeta portabilidade, reprodutibilidade de ambientes, e integração com plataformas de orquestração (Kubernetes, Docker Swarm, Azure Container Apps).

A estrutura do projeto revela a decisão de containerização:

```
webapi/
├── docker-compose.yml           # Orquestração local (Postgres)
└── src/Devlivery/
    └── Dockerfile               # Multi-stage build para produção
```

**Problema:** Como empacotar e deployar a aplicação garantindo consistência entre ambientes de dev, staging e produção?

## Opções Consideradas

* **Deployment tradicional (IIS/Windows Server)** - Publicar em servidor Windows com IIS
* **Self-contained executable** - Binário standalone sem runtime dependency
* **Docker container** - Aplicação empacotada em imagem OCI-compliant
* **Azure App Service direto (zip deploy)** - Deploy via Web Deploy ou GitHub Actions

## Decisão

**Escolhida:** "Docker container", porque:

1. **Portabilidade:** Mesma imagem roda em dev (Docker Desktop), CI (GitHub Actions), produção (qualquer orquestrador)
2. **Reprodutibilidade:** "Build once, run anywhere" — elimina discrepâncias de ambiente
3. **Infraestrutura como Código:** `Dockerfile` é versionado e auditável
4. **Integração com Orquestradores:** Kubernetes, Azure Container Apps, AWS ECS suportam nativamente
5. **Isolamento:** Aplicação e suas dependências empacotadas atomicamente

### Implementação Técnica

**Dockerfile Multi-Stage (src/Devlivery/Dockerfile):**

```dockerfile
# STAGE 1: Base runtime (imagem final mínima)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# STAGE 2: Build (SDK completo)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Devlivery/Devlivery.csproj", "src/Devlivery/"]
RUN dotnet restore "src/Devlivery/Devlivery.csproj"
COPY . .
WORKDIR "/src/src/Devlivery"
RUN dotnet build "./Devlivery.csproj" -c $BUILD_CONFIGURATION -o /app/build

# STAGE 3: Publish (artefatos otimizados)
FROM build AS publish
RUN dotnet publish "./Devlivery.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# STAGE 4: Final (runtime + artefatos)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Devlivery.dll"]
```

**Benefícios do Multi-Stage Build:**
- Imagem final contém apenas runtime ASP.NET (não o SDK completo)
- Build intermediário cacheado acelera rebuilds
- Tamanho final reduzido (~200MB vs ~2GB com SDK)

**Docker Compose para Desenvolvimento Local:**

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:latest
    container_name: devlivery-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: devlivery
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5
```

**Workflow de Desenvolvimento:**
```bash
# Subir banco de dados local
docker-compose up -d

# Build da aplicação
docker build -t devlivery:latest -f src/Devlivery/Dockerfile .

# Executar container
docker run -p 8080:8080 --env-file .env devlivery:latest
```

**CI/CD Pipeline (GitHub Actions):**
```yaml
# .github/workflows/main-build-deploy.yml
- name: Build Docker Image
  run: docker build -t ${{ env.REGISTRY }}/devlivery:${{ github.sha }} .
  
- name: Push to Container Registry
  run: docker push ${{ env.REGISTRY }}/devlivery:${{ github.sha }}
```

### Consequências

* ✅ **Bom:** Paridade dev/prod — elimina "funciona na minha máquina"
* ✅ **Bom:** Rollback trivial — redeployar imagem anterior
* ✅ **Bom:** Escalabilidade horizontal simples (replicar containers)
* ✅ **Bom:** Isolamento de rede e recursos via cgroups
* ✅ **Bom:** Integração nativa com plataformas cloud modernas
* ⚠️ **Neutro:** Requer Docker runtime em ambientes de deployment
* ⚠️ **Neutro:** Build times ligeiramente mais longos (mitigado por cache de layers)
* ⚠️ **Ruim:** Debugging pode ser mais complexo (requer attach a container)
* ⚠️ **Ruim:** Overhead de storage para imagens (mitigado por registry privado com garbage collection)

### Convenções de Tagging

- **Development:** `devlivery:latest`
- **Staging:** `devlivery:staging-<commit-sha>`
- **Production:** `devlivery:v1.2.3` (semantic versioning)
- **Feature Branches:** `devlivery:feature-<branch-name>`

### Segurança

- Imagens base oficiais da Microsoft (atualizadas automaticamente)
- Non-root user (`USER $APP_UID`)
- Secrets via variáveis de ambiente (não hardcoded)
- Scan de vulnerabilidades no CI (Trivy, Snyk)

**Princípio:** "Package your application and its runtime together; deploy the same artifact everywhere."
