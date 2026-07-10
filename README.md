# net-efcore — Demonstração avançada de EF Core 10

Projeto-vitrine que exercita, de ponta a ponta, os principais recursos do
**Entity Framework Core 10** sobre **.NET 10 + SQL Server**, com um front-end **React
(Vite + TypeScript)** que, para cada técnica, mostra **o SQL realmente enviado ao
banco**, os parâmetros e o tempo de execução.

> Domínio escolhido: uma **livraria/e-commerce**. Ele cobre naturalmente todos os
> tipos de relacionamento e cenários de mapeamento.

---

## 🚀 Como executar

Pré-requisitos: **.NET SDK 10**, **Node 20+** e um **SQL Server**.

No Windows não é preciso instalar nada: a connection string padrão aponta para o
**LocalDB** (`(localdb)\MSSQLLocalDB`), que já vem com o Visual Studio / SQL Server
Express. Em outros sistemas, suba um container e ajuste
`ConnectionStrings:Default`:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Sua_Senha_F0rte" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### 1. API (.NET)

```bash
dotnet run --project src/EfCoreDemo.Api
```

Na primeira execução a API cria o banco `EfCoreDemo`, aplica as *migrations* e o
popula automaticamente. Sobe por padrão em `http://localhost:5222`
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
| `Category` | PK `int` gerada no cliente pelo **Hi/Lo** (`UseHiLo`), **auto-relacionamento** (árvore pai/filhos) |
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

**Geração de chaves** — `/api/keys/*`
- `hilo/insert` (`UseHiLo()`: ids atribuídos no `Add()`, INSERT sem `OUTPUT`, e os "buracos" após rollback)

**Auditoria** — `/api/audit/logs` (trilha gerada pelo interceptor)

### Interceptors implementados
- `AuditableEntityInterceptor` (`SaveChangesInterceptor`): timestamps/usuário, soft
  delete (transforma DELETE em UPDATE), regeneração do token de concorrência e
  geração de `AuditLog` com *diff* em JSON.
- `SqlCaptureInterceptor` (`DbCommandInterceptor`): captura o SQL por requisição
  para a UI.

---

## 🔢 Hi/Lo: gerando o id no cliente

`Category.Id` é gerado pela **aplicação**, não pelo banco:

```csharp
builder.Property(c => c.Id).UseHiLo("CategoryHiLoSequence");
```

Em vez de um round-trip por insert (IDENTITY), o EF reserva um **bloco** de ids com
um único `SELECT NEXT VALUE FOR` sobre uma `SEQUENCE` (criada pela migration com
`incrementBy: 10`) e distribui o `lo` em memória:

```
id = (hi - 1) * blockSize + lo        lo ∈ [1, blockSize]
```

Só o `hi` precisa ser único globalmente, e é ele que vem do banco. Efeitos
observáveis em `/api/keys/hilo/insert`:

- a entidade tem id utilizável **antes** do `SaveChanges` (é atribuído no `Add()`);
- o INSERT não precisa de `OUTPUT`/`SCOPE_IDENTITY()` para ler o id de volta;
- N inserts **não** geram N idas à sequence — o SQL capturado mostra o
  `NEXT VALUE FOR` aparecendo só quando o bloco vira;
- um grafo inteiro pode ir em um round-trip, pois as FKs já são conhecidas.

**O preço:** a reserva do bloco é independente da transação de negócio — senão um
rollback devolveria o `hi` e dois processos reusariam o bloco. Logo, um rollback
(ou um restart da aplicação) **queima os ids restantes**: a sequência tem buracos.
Isso é esperado, e é o trade-off do Hi/Lo. O endpoint de demo faz rollback de
propósito para você ver os buracos.

> O `UseHiLo()` é uma extensão **específica de provider**. Existe no SQL Server e no
> Npgsql (PostgreSQL), ambos apoiados em `SEQUENCE`. **Não existe no SQLite**, que
> não tem sequences — lá o Hi/Lo precisaria ser implementado à mão.

---

## ⚠️ Limitações que sobram (e não são do banco)

- **Complex Types não funcionam em hierarquias TPT.** Isso é uma limitação do
  **EF Core**, não do provider: as colunas até são criadas e o INSERT passa, mas a
  *consulta* estoura em `GenerateComplexPropertyShaperExpression`. Por isso
  `Payment` usa `decimal Amount` + `string Currency` simples, enquanto `Money`
  (Complex Type) aparece em `Book`, `Order` e `OrderItem`. Verificado no EF Core 10.

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

Para recriar o banco do zero:

```bash
dotnet ef database drop --force \
  --project src/EfCoreDemo.Infrastructure \
  --startup-project src/EfCoreDemo.Api
```

Em seguida rode a API novamente — ela recria, migra e popula.
