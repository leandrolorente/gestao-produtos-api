# Gestão Produtos API - Instruções para IA

## Arquitetura Geral
Este projeto segue **Clean Architecture + DDD** com 4 camadas distintas:
- `GestaoProdutos.API` - Controllers e configuração da Web API
- `GestaoProdutos.Application` - Services, DTOs e interfaces de aplicação  
- `GestaoProdutos.Domain` - Entidades, ValueObjects, Enums e interfaces de domínio
- `GestaoProdutos.Infrastructure` - Repositórios e acesso a dados MongoDB

## Padrões de Código Específicos

### Entidades de Domínio
- Todas herdam de `BaseEntity` com Id (ObjectId), DataCriacao, DataAtualizacao, Ativo
- Use `[BsonId]` e `[BsonRepresentation(BsonType.ObjectId)]` para IDs
- Implementam métodos de domínio (ex: `EstaComEstoqueBaixo()`, `AtualizarEstoque()`)

### DTOs e Mapeamento  
- DTOs usam **nomes em inglês** para compatibilidade com frontend Angular (`Name`, `Quantity`, `Price`)
- Entidades usam **nomes em português** (`Nome`, `Quantidade`, `Preco`)
- Use `record` para DTOs: `ProdutoDto`, `CreateProdutoDto`, `UpdateProdutoDto`
- Mapeamento manual via método `MapToDto()` nos Services

### Dependency Injection
- Services: `IProdutoService`, `IClienteService` registrados como Scoped
- Repository: `IUnitOfWork` como Scoped
- MongoDB: `MongoDbContext` como Singleton com connection string e database name

### Controllers
- Use `[ApiController]` e `[Route("api/[controller]")]`
- Todos os métodos são async e retornam `ActionResult<T>`
- Tratamento de erro padrão: try-catch com `StatusCode(500)` e mensagem estruturada
- Documentação XML: `/// <summary>` para endpoints

### MongoDB
- Collections: `produtos`, `clientes`, `usuarios` (nomes em português)
- Índices automáticos em `Sku`, `Nome`, `Categoria` via `CreateIndexesAsync()`
- Usa MongoDB.Driver oficial do .NET

### Testes
- **xUnit + FluentAssertions** para todos os testes
- Estrutura: `Unit/`, `Integration/`, organizada por camada
- Pattern: Arrange-Act-Assert com comentários
- Naming: `Method_Scenario_ExpectedResult`

## Comandos Essenciais
```bash
# Build completo
dotnet build

# Rodar testes
dotnet test

# Executar API (porta 5000/5001)
dotnet run --project GestaoProdutos.API

# Restaurar dependências
dotnet restore
```

## Configuração MongoDB
- Connection string padrão: `mongodb://localhost:27017`  
- Database: `GestaoProdutosDB`
- Configurável via `appsettings.json` ou variáveis de ambiente

## CORS
Configurado para Angular em `localhost:4200` e `4201` - atualizar se necessário.

## Desenvolvimento
- .NET 9.0 com Nullable habilitado
- Swagger disponível em desenvolvimento
- Use Soft Delete (campo `Ativo`) em vez de exclusão física
- StatusCode 500 para erros não tratados, com details em development