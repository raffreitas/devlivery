# Arquitetura de Monólito Modular Baseado em Features

**Data:** 2025-12-17  
**Status:** Aceito  
**Contexto:** C4 Model - Nível 2 (Container)

## Contexto e Problema

O sistema Devlivery precisa de uma arquitetura que permita evolução rápida, facilite a colaboração entre desenvolvedores e mantenha baixa complexidade operacional. A decisão fundamental é: devemos estruturar a aplicação como microserviços distribuídos, monólito tradicional em camadas, ou uma abordagem híbrida?

A estrutura do repositório revela esta decisão através da organização:

```
webapi/
├── src/
│   └── Devlivery/              # ← Único projeto de aplicação
│       ├── Features/
│       │   ├── Auth/
│       │   ├── CashRegister/
│       │   ├── Orders/
│       │   ├── Products/
│       │   └── Users/
│       └── Shared/
└── test/
    └── Devlivery.Tests/        # ← Único projeto de testes
```

**Problema:** Como organizar a aplicação para maximizar modularidade sem incorrer em complexidade de distribuição prematura?

## Opções Consideradas

* **Microserviços desde o início** - Separar Auth, Orders, Products em serviços independentes
* **Monólito em camadas tradicional** - Organização por tipo técnico (Controllers, Services, Repositories)
* **Monólito modular baseado em features** - Organização vertical por capacidade de negócio dentro de um único deployable

## Decisão

**Escolhida:** "Monólito modular baseado em features", porque:

1. **Simplicidade Operacional:** Um único processo, um deployment, um container Docker
2. **Baixa Latência:** Comunicação in-process entre features (chamadas de método vs HTTP/gRPC)
3. **Transações ACID:** Operações cross-feature usam transações de banco de dados nativas
4. **Facilidade de Refatoração:** Features podem ser extraídas para serviços independentes no futuro se necessário
5. **Developer Experience:** Build, teste e debug unificados

### Implementação Técnica

A decisão se materializa em:

1. **Projeto Único:** `Devlivery.csproj` contém toda a lógica de aplicação
2. **Features Coesas:** Cada pasta em `Features/` encapsula um bounded context
3. **Shared Kernel:** `Shared/` contém infraestrutura cross-cutting (Persistence, Identity, Tenancy)
4. **Deployment Atômico:** Um único `Dockerfile` gera uma imagem deployável

```
src/Devlivery/
├── Features/                    # Módulos de domínio
│   ├── Orders/
│   │   ├── Domain/             # Entidades e regras de negócio
│   │   ├── Commands/           # Casos de uso de escrita
│   │   ├── Queries/            # Casos de uso de leitura
│   │   ├── Infrastructure/     # Repositórios e persistência
│   │   └── OrdersFeature.cs    # Bootstrap e endpoints
│   └── Products/
│       └── [mesma estrutura]
└── Shared/                      # Infraestrutura compartilhada
    ├── Infrastructure/
    │   ├── Persistence/        # DbContext, Migrations
    │   ├── Identity/           # Autenticação
    │   └── Tenancy/            # Multi-tenancy
    └── Application/            # Behaviors, Errors
```

**Ponto de Entrada Único:**
- `Program.cs` → `Startup.ConfigureBuilder()` registra todas as features
- `Startup.ConfigureApp()` mapeia todos os endpoints via `MapXxxEndpoints()`

### Consequências

* ✅ **Bom:** Complexidade operacional mínima (um deploy, um processo, um log)
* ✅ **Bom:** Refatorações cross-feature são simples (IDE pode rastrear referências)
* ✅ **Bom:** Transações distribuídas não são necessárias
* ✅ **Bom:** Startup e build times rápidos comparados a microserviços
* ⚠️ **Neutro:** Escalabilidade horizontal replica toda a aplicação (aceitável para a maioria dos cenários)
* ⚠️ **Ruim:** Features não podem escalar independentemente
* ⚠️ **Ruim:** Deploy de uma feature requer deploy completo da aplicação
* ⚠️ **Ruim:** Dependências entre features podem criar acoplamento não desejado (mitigado por design consciente)

### Estratégia de Migração Futura

Se necessário, features individuais podem ser extraídas para microserviços:
1. Definir contratos de API explícitos (já existem via Minimal APIs)
2. Mover pasta `Features/X/` para novo projeto
3. Substituir chamadas in-process por HTTP clients
4. Implementar padrões de resiliência (retry, circuit breaker)

**Princípio:** "Start with a monolith, evolve to microservices when pain justifies complexity."
