# 🍕 Pizza Delivery - Sistema PDV

Sistema de PDV (Ponto de Venda) para delivery de pizzas desenvolvido com React, TypeScript e TailwindCSS.

## ✨ Funcionalidades

### 📊 Dashboard
- Visão geral dos pedidos do dia
- Estatísticas de receita e pedidos
- Acompanhamento de pedidos ativos
- Ticket médio
- Status dos pedidos em tempo real

### 🍕 Produtos
- **CRUD completo de produtos**
  - Criar novos produtos
  - Editar produtos existentes
  - Excluir produtos
  - Marcar disponibilidade
- Filtros por categoria
- Busca por nome ou descrição
- Cards visuais com imagens

### 📋 Pedidos
- **Criar novos pedidos**
  - Adicionar múltiplos produtos
  - Especificar quantidades
  - Adicionar observações por item
  - Informações do cliente e entrega
- **Listar pedidos**
  - Visualização em cards
  - Filtro por status
  - Ordenação por data
- **Gerenciar status**
  - Pendente → Em Preparo → Pronto → Entregue
  - Cancelar pedidos
  - Excluir pedidos finalizados

## 🏗️ Arquitetura

O projeto segue uma estrutura **feature-based**:

```
src/
├── features/              # Funcionalidades principais
│   ├── dashboard/        # Dashboard e estatísticas
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   └── types/
│   ├── products/         # Gerenciamento de produtos
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   └── types/
│   └── orders/           # Gerenciamento de pedidos
│       ├── components/
│       ├── pages/
│       ├── services/
│       └── types/
├── shared/               # Recursos compartilhados
│   ├── components/       # Componentes reutilizáveis
│   ├── contexts/         # Context API
│   ├── hooks/
│   └── types/
└── routes/               # Configuração de rotas
```

## 🛠️ Tecnologias

- **React 19** - Interface de usuário
- **TypeScript** - Tipagem estática
- **TailwindCSS** - Estilização
- **React Router** - Navegação
- **Context API** - Gerenciamento de estado
- **LocalStorage** - Persistência de dados
- **Vite** - Build tool

## 🚀 Como Executar

1. **Instalar dependências:**
   ```bash
   pnpm install
   ```

2. **Iniciar servidor de desenvolvimento:**
   ```bash
   pnpm dev
   ```

3. **Acessar a aplicação:**
   - Abra o navegador em `http://localhost:5174`

## 📱 Navegação

- **/** - Dashboard com pedidos do dia
- **/products** - Gerenciamento de produtos
- **/orders** - Gerenciamento de pedidos

## 💾 Armazenamento

Os dados são persistidos no **LocalStorage** do navegador:
- `products` - Lista de produtos
- `orders` - Lista de pedidos

## 🎨 Componentes Compartilhados

- **Layout** - Layout principal com navegação
- **Button** - Botão reutilizável com variantes
- **Input** - Campo de entrada com label e erro
- **Card** - Container com sombra
- **Modal** - Modal responsivo e acessível

## 🔄 Fluxo de Trabalho

### Criar um Produto
1. Acesse "Produtos"
2. Clique em "+ Novo Produto"
3. Preencha o formulário
4. Salve

### Criar um Pedido
1. Acesse "Pedidos"
2. Clique em "+ Novo Pedido"
3. Preencha dados do cliente
4. Adicione produtos
5. Confirme o pedido

### Gerenciar Status do Pedido
1. No Dashboard ou em Pedidos
2. Clique no botão de próximo status
3. Acompanhe a evolução:
   - Pendente
   - Em Preparo
   - Pronto
   - Entregue

## 🔮 Próximos Passos

- [ ] Autenticação de usuários
- [ ] Backend com API REST
- [ ] Impressão de pedidos
- [ ] Notificações em tempo real
- [ ] Relatórios e gráficos
- [ ] Upload de imagens de produtos
- [ ] Histórico de pedidos
- [ ] Sistema de categorias dinâmico

## 📝 Notas

- A aplicação é totalmente funcional offline
- Os dados são salvos automaticamente
- Interface responsiva para mobile e desktop
- Sem autenticação por enquanto (conforme solicitado)

---

Desenvolvido com ❤️ para pizzarias! 🍕
