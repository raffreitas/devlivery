# Service Layer Pattern com DTO Mapping

**Data:** 2025-12-26  
**Status:** Aceito  
**Contexto:** Padrão de Design / Camada de Aplicação

## Contexto e Problema

O frontend precisa comunicar com a API backend que retorna DTOs (Data Transfer Objects) em formato diferente dos tipos de domínio usados na aplicação. A decisão fundamental é: devemos usar DTOs diretamente no código ou mapear para tipos de domínio?

A estrutura do repositório revela esta decisão através da organização:

```
src/features/products/
├── services/
│   └── product-service.ts    # ← Mapeamento DTO → Domain
├── types/
│   └── index.ts              # ← Tipos de domínio (Product)
└── hooks/
    └── use-products.ts       # ← Usa tipos de domínio
```

**Problema:** Como isolar o código da aplicação das estruturas de dados da API?

## Opções Consideradas

* **Usar DTOs diretamente** - Tipos da API são usados em toda a aplicação
* **Mapeamento explícito** - Services fazem conversão DTO → Domain Type
* **Code generation** - Gerar tipos TypeScript a partir do OpenAPI/Swagger

## Decisão

**Escolhida:** "Mapeamento explícito", porque:

1. **Isolamento:** Código da aplicação não depende de estruturas da API
2. **Flexibilidade:** API pode mudar sem impactar toda a aplicação
3. **Tipos de Domínio:** Aplicação usa tipos semânticos (ex: `Date` ao invés de `string`)
4. **Transformações:** Conversões complexas (ex: snake_case → camelCase, string → Date) ficam centralizadas
5. **Testabilidade:** Services podem ser testados isoladamente com mocks de DTOs

### Implementação Técnica

A decisão se materializa em:

1. **Services fazem mapeamento:** Cada service tem função `mapXxxDto()` privada
2. **Tipos separados:** DTOs são interfaces locais ao service, tipos de domínio são exportados
3. **Transformações centralizadas:** Conversões de formato (datas, casos) ficam no service

```typescript
// src/features/products/services/product-service.ts
interface ProductDto {
  id: string;
  name: string;
  price: number;
  createdAt: string;        // ← API retorna string
  updatedAt: string;        // ← API retorna string
}

function mapProductDto(dto: ProductDto): Product {
  return {
    id: dto.id,
    name: dto.name,
    price: dto.price,
    createdAt: new Date(dto.createdAt),  // ← Conversão string → Date
    updatedAt: new Date(dto.updatedAt),  // ← Conversão string → Date
  };
}

export const productService = {
  getAll: async (): Promise<Product[]> => {
    const res = await api.get<ApiResponse<ProductDto[]>>("/api/products");
    const list = res.data ?? [];
    return list.map(mapProductDto);  // ← Mapeamento automático
  },
  // ...
};

// src/features/products/types/index.ts
export interface Product {
  id: string;
  name: string;
  price: number;
  createdAt: Date;          // ← Tipo de domínio usa Date
  updatedAt: Date;          // ← Tipo de domínio usa Date
}
```

**Padrão de Service:**
- Interface `XxxDto` privada ao service (não exportada)
- Função `mapXxxDto()` privada faz conversão
- Métodos do service retornam tipos de domínio
- Hooks e componentes usam apenas tipos de domínio

### Consequências

* ✅ **Bom:** Aplicação isolada de mudanças na API
* ✅ **Bom:** Tipos de domínio semânticos (Date, enums) melhoram DX
* ✅ **Bom:** Transformações centralizadas facilitam manutenção
* ✅ **Bom:** Services são pontos únicos de integração com API
* ⚠️ **Neutro:** Pequeno overhead de mapeamento (aceitável para benefícios)
* ⚠️ **Ruim:** Pode ser tentador pular mapeamento em casos simples (requer disciplina)
* ⚠️ **Ruim:** Mudanças na API requerem atualização do mapeamento (mas isola o resto)

