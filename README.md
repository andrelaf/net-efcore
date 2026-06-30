# net-efcore — Demonstração avançada de EF Core 10

Projeto-vitrine que exercita, de ponta a ponta, os principais recursos do
**Entity Framework Core 10** sobre **.NET 10 + SQLite**, com um front-end **React
(Vite + TypeScript)** que, para cada técnica, mostra **o SQL realmente enviado ao
banco**, os parâmetros e o tempo de execução.

> Domínio escolhido: uma **livraria/e-commerce**. Ele cobre naturalmente todos os
> tipos de relacionamento e cenários de mapeamento.

---

## 🚀 Como executar

Pré-requisitos: **.NET SDK 10** e **Node 20+**.

### 1. API (.NET)

```bash
dotnet run --project src/EfCoreDemo.Api
```

Na primeira execução a API aplica as *migrations* e popula o banco
(`efcoredemo.db`) automaticamente. Suba por padrão em `http://localhost:5222`
(veja `src/EfCoreDemo.Api/Properties/launchSettings.json`).

- Catálogo de demos: `GET http://localhost:5222/api/demos`
- OpenAPI: `http://localhost:5222/openapi/v1.json`

### 2. Front-end (React)

```bash
cd client
npm install
npm run dev
```

Abre em `http://localhost:5173`. Se a API estiver em outra porta, ajuste
`client/.env` (`VITE_API_BASE`).

---

## 🏛️ Arquitetura

```
src/
├─ EfCoreDemo.Domain/          # Entidades, value objects, enums (sem dependência de EF)
├─ EfCoreDemo.Infrastructure/  # DbContext, Fluent API, interceptors, migrations, seed
└─ EfCoreDemo.Api/             # Minimal API: 1 endpoint por técnica + envelope com SQL
client/                        # React + Vite + TypeScript
```

Separação em camadas: o domínio é **persistence-ignorant**; todo o mapeamento vive
na Infrastructure via classes `IEntityTypeConfiguration<T>`.

### O envelope de demonstração

Toda chamada devolve o mesmo formato, o que permite ao front mostrar o SQL:

```jsonc
{
  "title": "...",
  "technique": "...",
  "explanation": "...",     // por que / como funciona
  "data": { ... },          // o resultado da consulta
  "sql": [                  // SQL capturado por um DbCommandInterceptor
    { "sql": "SELECT ...", "parameters": { "@p0": 10 }, "elapsedMs": 0.7 }
  ],
  "sqlCount": 1
}
```

---

## 🗺️ Modelo de domínio × recurso demonstrado

| Entidade | Recursos do EF Core que ela exibe |
|---|---|
| `Customer` | PK **UUID v7**, **Complex Type** `Address`, 1:1, 1:N, auditoria, soft delete, concorrência |
| `CustomerProfile` | lado dependente do **1:1**, **coleção primitiva** (`Interests`) em JSON |
| `Category` | PK `int` (IDENTITY), **auto-relacionamento** (árvore pai/filhos) |
| `Book` (abstract) | **Herança TPH**, **Complex Type em JSON** (`Metadata`), coleção primitiva (`Tags`), N:1 |
| `PhysicalBook` / `EBook` | subtipos TPH; `PhysicalBook` usa **Owned Type** (`Dimensions`) |
| `Author` ↔ `Book` | **N:N com payload** via entidade de junção `BookAuthor` (PK composta, enum `Role`) |
| `Order` | **Owned Type** (`ShippingAddress`), enum→string, Complex Type (`Total`), concorrência |
| `OrderItem` | N:1 duplo (Order e Book), propriedade calculada ignorada |
| `Payment` (abstract) | **Herança TPT**, 1:1 com `Order` |
| `CreditCardPayment` / `PixPayment` / `BoletoPayment` | subtipos TPT (uma tabela cada) |
| `AuditLog` | gravado automaticamente pelo **SaveChangesInterceptor** |

---

## 🧪 Técnicas demonstradas (1 endpoint cada)

**Relacionamentos** — `/api/relationships/*`
- `one-to-one`, `one-to-many`, `many-to-many` (com payload), `self-referencing`
- `inheritance-tph` (Table-per-Hierarchy), `inheritance-tpt` (Table-per-Type)

**Carregamento** — `/api/loading/*`
- `eager` (Include/ThenInclude), `lazy` (proxies — observe o N+1!), `explicit`
- `split-query` (`AsSplitQuery`), `projection` (Select→DTO), `no-tracking`

**Consultas** — `/api/querying/*`
- `filtered` (Global Query Filter nomeado), `ignore-filters` (incl. desabilitar por nome — EF 10)
- `raw-sql-entities` (`FromSql`), `raw-sql-scalar` (`SqlQuery<T>`)
- `compiled-query` (`EF.CompileAsyncQuery`), `left-join` (operador `LeftJoin` do .NET 10)

**Modificações** — `/api/mutations/*`
- `batch-update` (`ExecuteUpdateAsync` — atômico, sem tracking)
- `batch-delete` (`ExecuteDeleteAsync`)
- `transaction` (BeginTransaction + rollback)
- `concurrency` (token de concorrência → `DbUpdateConcurrencyException`)

**Auditoria** — `/api/audit/logs` (trilha gerada pelo interceptor)

### Interceptors implementados
- `AuditableEntityInterceptor` (`SaveChangesInterceptor`): timestamps/usuário, soft
  delete (transforma DELETE em UPDATE), regeneração do token de concorrência e
  geração de `AuditLog` com *diff* em JSON.
- `SqlCaptureInterceptor` (`DbCommandInterceptor`): captura o SQL por requisição
  para a UI.

---

## ⚠️ Observações sobre o SQLite

O SQLite é ótimo para uma demo autocontida, mas tem limites que o projeto contorna
de forma didática:

- **Sem `rowversion` nativo** → a concorrência otimista usa um token `Guid`
  marcado com `IsConcurrencyToken()` e regenerado pelo interceptor a cada save.
- **`Guid` é gravado como TEXT em maiúsculas** → ao montar SQL cru com Guids,
  interpole o `Guid` diretamente (não `.ToString()`) para o provider serializar
  igual à coluna.
- **`decimal`** é armazenado como TEXT; `HasPrecision` é informativo.
- **Complex Types não são suportados em hierarquias TPT** → por isso `Payment`
  usa `decimal Amount` simples, enquanto `Money` (Complex Type) aparece em
  `Book`, `Order` e `OrderItem`.

---

## 🛠️ Migrations

```bash
# criar nova migration
dotnet ef migrations add <Nome> \
  --project src/EfCoreDemo.Infrastructure \
  --startup-project src/EfCoreDemo.Api

# aplicar (a API também aplica no startup)
dotnet ef database update \
  --project src/EfCoreDemo.Infrastructure \
  --startup-project src/EfCoreDemo.Api
```

Para recriar o banco do zero, apague `efcoredemo.db*` e rode a API novamente.
