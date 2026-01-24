# Devlivery WebApp

Devlivery WebApp é a interface web para gerenciamento de entregas, permitindo controle de pedidos, produtos, fluxo de caixa e visualização de métricas importantes através de um dashboard intuitivo.

## 🚀 Tecnologias

Este projeto foi desenvolvido utilizando as seguintes tecnologias:

- **Core**: [React 19](https://react.dev/), [Vite](https://vitejs.dev/)
- **Linguagem**: [TypeScript](https://www.typescriptlang.org/)
- **Estilização**: [Tailwind CSS 4](https://tailwindcss.com/)
- **Roteamento**: [React Router 7](https://reactrouter.com/)
- **Gerenciamento de Estado/Server State**: [TanStack Query](https://tanstack.com/query/latest)
- **Formulários**: [React Hook Form](https://react-hook-form.com/) + [Zod](https://zod.dev/)
- **UI Components**: [Radix UI](https://www.radix-ui.com/), [Lucide React](https://lucide.dev/), [Sonner](https://sonner.emilkowal.ski/)
- **Gráficos**: [Recharts](https://recharts.org/)
- **Linting & Formatting**: [Biome](https://biomejs.dev/)

## ✨ Funcionalidades

O sistema está modularizado em:

- **Auth**: Autenticação e controle de acesso.
- **Dashboard**: Visão geral e métricas de desempenho.
- **Orders**: Gerenciamento de pedidos de entrega.
- **Products**: Cadastro e manutenção de produtos/menu.
- **Cash**: Controle de fluxo de caixa.

## 📦 Como rodar o projeto

### Pré-requisitos

Certifique-se de ter o [Node.js](https://nodejs.org/) instalado em sua máquina.

### Instalação

1. Clone o repositório:
```bash
git clone https://github.com/raffreitas/devlivery-webapp.git
cd devlivery-webapp
```

2. Instale as dependências:
```bash
npm install
# ou
pnpm install
# ou
yarn
```

3. Configure as variáveis de ambiente:
Crie um arquivo `.env` na raiz do projeto baseado no `.env.example`.
```bash
cp .env.example .env
```
Ajuste a variável `VITE_API_URL` para apontar para o seu backend.

4. Inicie o servidor de desenvolvimento:
```bash
npm run dev
```

O projeto estará rodando em `http://localhost:5173`.

## 🛠 Scripts Disponíveis

- `npm run dev`: Inicia o servidor de desenvolvimento.
- `npm run build`: Compila o projeto para produção.
- `npm run preview`: Visualiza a versão de produção localmente.
- `npm run lint`: Verifica erros de linting com Biome.
- `npm run format`: Formata o código com Biome.

## 📂 Estrutura do Projeto

```
src/
├── features/       # Módulos de negócio (Auth, Orders, etc.)
├── shared/         # Componentes e utilitários compartilhados
├── app-routes.tsx  # Definição das rotas da aplicação
└── ...
```
