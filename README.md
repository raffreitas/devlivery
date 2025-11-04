# Devlivery WebAPI

Backend desenvolvido com .NET 9 seguindo as melhores práticas de desenvolvimento.

## 🏗️ Arquitetura

- **Vertical Slice Architecture**: Cada feature é auto-contida com sua própria lógica de negócio
- **Minimal APIs**: APIs leves e performáticas
- **Entity Framework Core**: ORM para PostgreSQL
- **FluentValidation**: Validação de dados robusta

## 🎯 Princípios Aplicados

- **KISS** (Keep It Simple, Stupid): Código simples e direto
- **YAGNI** (You Aren't Gonna Need It): Implementado apenas o necessário
- **DRY** (Don't Repeat Yourself): Evitar duplicação de código
- **SOLID**: Princípios de design orientado a objetos

## 📁 Estrutura do Projeto

```
Devlivery.WebApi/
├── Database/
│   ├── Entities/          # Entidades do EF Core
│   └── AppDbContext.cs    # Contexto do banco
├── Features/              # Features organizadas por domínio
│   ├── Auth/
│   ├── Products/
│   ├── Orders/
│   └── Dashboard/
└── Infrastructure/        # Serviços de infraestrutura
```

## 🚀 Como Executar

### Pré-requisitos

- .NET 9 SDK
- PostgreSQL
- Docker (opcional)

### 1. Configurar o Banco de Dados

Execute o PostgreSQL via Docker:

```bash
docker run --name devlivery-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres
```

Ou use uma instância local do PostgreSQL.

### 2. Atualizar Connection String

Edite `appsettings.Development.json` se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=devlivery;Username=postgres;Password=postgres"
  }
}
```

### 3. Restaurar Dependências

```bash
dotnet restore
```

### 4. Executar o Projeto

```bash
dotnet run
```

O projeto irá:
- Criar o banco de dados automaticamente
- Aplicar as migrations
- Popular dados iniciais (seed)
- Iniciar em `http://localhost:5000` ou `https://localhost:5001`

## 📊 Migrations

### Criar uma nova migration

```bash
dotnet ef migrations add NomeDaMigration
```

### Aplicar migrations

```bash
dotnet ef database update
```

### Reverter migration

```bash
dotnet ef database update NomeDaMigrationAnterior
```

## 🔌 Endpoints

### Auth
- `POST /api/auth/login` - Login

### Products
- `GET /api/products` - Listar produtos
- `GET /api/products/{id}` - Buscar produto por ID
- `POST /api/products` - Criar produto
- `PUT /api/products/{id}` - Atualizar produto
- `DELETE /api/products/{id}` - Deletar produto

### Orders
- `GET /api/orders` - Listar pedidos
- `GET /api/orders/{id}` - Buscar pedido por ID
- `POST /api/orders` - Criar pedido
- `PATCH /api/orders/{id}/status` - Atualizar status do pedido
- `DELETE /api/orders/{id}` - Deletar pedido

### Dashboard
- `GET /api/dashboard/stats` - Estatísticas do dashboard

## 🧪 Testando a API

Use o arquivo `Devlivery.WebApi.http` no Visual Studio Code com a extensão REST Client ou no Rider.

## 📦 Dependências

- Microsoft.EntityFrameworkCore (9.0.0)
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.0)
- BCrypt.Net-Next (4.0.3)
- FluentValidation (11.11.0)

## 🔐 Credenciais de Teste

```
Email: admin@pizza.com
Senha: 123456
```

## 🛠️ Tecnologias

- .NET 9
- PostgreSQL
- Entity Framework Core
- Minimal APIs
- FluentValidation
- BCrypt
