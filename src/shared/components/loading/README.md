# Loading Components - Guia de Uso

Este guia define os padrões de uso dos componentes de loading na aplicação.

## Componentes Disponíveis

### 1. `LoadingState`
**Quando usar**: Loading inicial (primeira carga, sem dados ainda)
- Mostra skeleton ou spinner centralizado
- Usado quando `isLoading === true` e `data.length === 0`

```tsx
<LoadingState
  isLoading={loading && products.length === 0}
  skeleton={<GridSkeleton items={10} columns={5} />}
>
  <ProductsList products={products} />
</LoadingState>
```

### 2. `LoadingOverlay`
**Quando usar**: Refetch em background (quando há dados existentes e está atualizando)

#### Variantes:

##### `top-bar` (Progress Bar) - **PADRÃO RECOMENDADO**
**Quando usar**:
- Refetch de páginas completas
- Atualizações em background que afetam o conteúdo principal
- Quando o usuário pode continuar interagindo com a UI

**Exemplo**:
```tsx
<LoadingOverlay isFetching={isFetching} position="top-bar" />
```

**Características**:
- Barra sutil no topo da página (3px)
- Não bloqueia a UI
- Animação shimmer discreta
- Ideal para refetch automático (React Query)

##### `inline` (Spinner)
**Quando usar**:
- Indicadores em seções específicas (headers, cards)
- Quando precisa mostrar feedback em um local específico
- Ações que não afetam a página inteira

**Exemplo**:
```tsx
{isFetching && (
  <LoadingOverlay
    isFetching={isFetching}
    position="inline"
    className="hidden sm:flex mr-2"
  />
)}
```

##### `badge` (Badge com Spinner)
**Quando usar**:
- Quando precisa de mais destaque que o inline
- Feedback de ações importantes mas não críticas
- Quando quer mostrar mensagem de texto junto

**Exemplo**:
```tsx
<LoadingOverlay
  isFetching={isSyncing}
  position="badge"
  message="Sincronizando..."
/>
```

### 3. `LoadingButton`
**Quando usar**: Mutações e ações do usuário (submit, save, delete)

```tsx
<LoadingButton
  isLoading={isPending}
  loadingText="Salvando..."
>
  Salvar
</LoadingButton>
```

## Padrão Recomendado por Contexto

### Páginas (Full Page)
```tsx
// ✅ Progress bar no topo (padrão)
<LoadingOverlay isFetching={isFetching} position="top-bar" />
```

### Headers/Seções Específicas
```tsx
// ✅ Spinner inline discreto
{isFetching && (
  <LoadingOverlay isFetching={isFetching} position="inline" />
)}
```

### Formulários
```tsx
// ✅ LoadingButton no botão submit
<LoadingButton isLoading={isSubmitting} loadingText="Salvando...">
  Salvar
</LoadingButton>
```

### Listas/Tabelas (Loading Inicial)
```tsx
// ✅ LoadingState com skeleton apropriado
<LoadingState
  isLoading={loading && items.length === 0}
  skeleton={<TableSkeleton rows={5} columns={4} />}
>
  <Table items={items} />
</LoadingState>
```

## Regras de UX

1. **Progress Bar (`top-bar`)** é o padrão para refetch em background de páginas
2. **Spinner inline** apenas quando necessário mostrar feedback em local específico
3. **Skeletons** sempre para loading inicial (primeira carga)
4. **LoadingButton** sempre para mutações (ações do usuário)
5. **Nunca bloquear a UI** durante refetch - sempre usar progress bar ou inline

## Decisão Rápida

- **Refetch de página completa?** → `top-bar` (progress bar)
- **Ação do usuário (submit/save)?** → `LoadingButton`
- **Loading inicial (sem dados)?** → `LoadingState` com skeleton
- **Feedback em seção específica?** → `inline` ou `badge`

