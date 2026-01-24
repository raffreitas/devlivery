# Devlivery Monorepo

Uma plataforma de delivery com arquitetura de monorepo contendo aplicações web e API backend robustas.

## 📦 Estrutura do Projeto

```
devlivery/
├── app/
│   ├── api/          # Backend API (.NET Core)
│   └── web/          # Frontend Web (React)
├── README.md         # Este arquivo
└── .git/
```

## 🏗️ Aplicações

### [`app/api`](./app/api) - Backend API
Aplicação backend construída com .NET Core seguindo princípios de arquitetura limpa.

**Stack:**
- .NET 8+
- ASP.NET Core Minimal APIs
- Entity Framework Core + Dapper
- PostgreSQL
- CQRS + Mediator Pattern
- FluentValidation
- OpenTelemetry
- JWT Authentication

**Recursos principais:**
- Multi-tenancy com Row-Level Security
- Arquitetura Vertical Slice
- Domain-Driven Design (DDD) tático
- Comprehensive testing com TestContainers
- CI/CD pipeline automatizado

📖 [Detalhes da API](./app/api/README.md)

### [`app/web`](./app/web) - Frontend Web
Aplicação web construída com React para interação com a API.

**Stack:**
- React
- TypeScript
- Vite / Next.js
- Estado gerenciado (Redux/Zustand/Context API)
- Componentes reutilizáveis

📖 [Detalhes do Web](./app/web/README.md)

## 🚀 Getting Started

### Pré-requisitos
- Node.js 18+ (para web)
- .NET 8+ SDK (para API)
- Docker & Docker Compose (recomendado)
- PostgreSQL 15+ (ou via Docker)

### Setup Local

#### Backend API
```bash
cd app/api
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

A API estará disponível em `http://localhost:5000`

#### Frontend Web
```bash
cd app/web
npm install
npm run dev
```

A aplicação estará disponível em `http://localhost:3000`

### Com Docker Compose
```bash
docker-compose up -d
```

## 📋 Documentação

- **API**: [OpenAPI/Swagger](./app/api/docs) documentação disponível em `/swagger`
- **Arquitetura**: [Architectural Decision Records](./app/api/docs/architectural-decision-records)
- **Concorrência**: [Análise de Concorrência](./app/api/docs/CONCURRENCY_ANALYSIS.md)

## 🧪 Testes

### API
```bash
cd app/api
dotnet test
```

### Web
```bash
cd app/web
npm test
```

## 🔄 CI/CD

Pipelines automatizadas configuradas em:
- **API**: `.github/workflows/` (build, test, deploy)
- **Web**: `.github/workflows/` (build, test, deploy)

## 📝 Commits e Histórico

Este monorepo foi criado integrando dois repositórios separados:
- **devlivery-webapi**: Backend API original
- **devlivery-webapp**: Frontend Web original

O histórico completo de ambos foi preservado usando `git subtree`. Você pode visualizar o histórico específico de cada aplicação:

```bash
# Histórico da API
git log --follow -- app/api/

# Histórico do Web
git log --follow -- app/web/
```

## 🤝 Contributing

1. Crie uma branch para sua feature: `git checkout -b feature/sua-feature`
2. Commit suas mudanças: `git commit -m 'Add: descrição da mudança'`
3. Push para a branch: `git push origin feature/sua-feature`
4. Abra um Pull Request

## 📄 License

[Adicione a licença apropriada aqui]

## 👥 Contato

Para questões ou sugestões, abra uma issue no repositório.
