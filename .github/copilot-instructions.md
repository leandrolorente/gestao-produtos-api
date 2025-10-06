# Gestão Produtos API - Instruções para IA

## Arquitetura Geral
Este projeto segue **Clean Architecture + DDD** com 4 camadas distintas:
- `GestaoProdutos.API` - Controllers e configuração da Web API
- `GestaoProdutos.Application` - Services, DTOs, interfaces de aplicação **+ Cache Services**
- `GestaoProdutos.Domain` - Entidades, ValueObjects, Enums e interfaces de domínio
- `GestaoProdutos.Infrastructure` - Repositórios e acesso a dados MongoDB

## Tecnologias de Cache

### Redis Cache (Primário)
- **Redis 7.x** via Docker: `docker run -d --name redis-gestao -p 6379:6379 redis:alpine`
- **StackExchange.Redis** + **Microsoft.Extensions.Caching.StackExchangeRedis**
- **ICacheService**: Interface unificada com 9 métodos (GetAsync, SetAsync, RemoveAsync, etc.)
- **RedisCacheService**: Implementação principal com serialização JSON camelCase
- **TTL configurável**: Dev (1-5min), Prod (30min), específico por tipo

### Memory Cache (Fallback)
- **MemoryCacheService**: Implementação IMemoryCache para fallback
- **HybridCacheService**: Strategy pattern (Redis primary + Memory fallback)
- **Logs visuais**: Console logs para debugging (🚀 CACHE HIT, 🗄️ DATABASE, 💾 REDIS SET)

### Padrões de Cache
- **SEMPRE** usar cache em Services para dados frequentes (produtos, clientes, dashboard)
- **Chaves**: padrão `gp:{entity}:{operation}` (ex: `gp:produtos:all`, `gp:dashboard:main`)
- **TTL**: Configurável via appsettings.json por tipo de dados
- **Invalidação**: Automática em CUD operations via RemoveAsync ou RemovePatternAsync

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
- Services: `IProdutoService`, `IClienteService`, `IAuthService` registrados como Scoped
- Repository: `IUnitOfWork` como Scoped
- MongoDB: `MongoDbContext` como Singleton com connection string e database name
- JWT: `IConfiguration` para configurações de autenticação
- **Cache**: `ICacheService` como Scoped (HybridCacheService), RedisCacheService + MemoryCacheService
- **Redis**: `AddStackExchangeRedisCache()` configurado via ConnectionStrings:Redis

### Controllers
- Use `[ApiController]` e `[Route("api/[controller]")]`
- Todos os métodos são async e retornam `ActionResult<T>`
- Tratamento de erro padrão: try-catch com `StatusCode(500)` e mensagem estruturada
- Documentação XML: `/// <summary>` para endpoints
- **Autenticação JWT**: Use `[Authorize]` para endpoints protegidos
- Claims de usuário: acessíveis via `HttpContext.User.Claims`
- **Logs visuais**: Adicionar logs de performance para identificar cache hits vs database
- **Cache timing**: Usar DateTime.UtcNow para medir performance < 50ms = cache, > 50ms = database

### MongoDB
- Collections: `produtos`, `clientes`, `usuarios` (nomes em português)
- Índices automáticos em `Sku`, `Nome`, `Categoria` via `CreateIndexesAsync()`
- Usa MongoDB.Driver oficial do .NET

### Testes
- **xUnit + FluentAssertions** para todos os testes
- Estrutura: `Unit/`, `Integration/`, organizada por camada
- Pattern: Arrange-Act-Assert com comentários
- Naming: `Method_Scenario_ExpectedResult`
- **Testes Redis**: 181 testes total (14 específicos para Redis, 100% passando)
- **Mocks**: Usar Moq para isolamento, aceitar falhas de mock se funcionalidade real OK

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

## Configuração Redis
- Connection string padrão: `localhost:6379`
- Docker Redis: `docker run -d --name redis-gestao -p 6379:6379 --restart unless-stopped redis:alpine`
- Verificar funcionamento: `docker exec redis-gestao redis-cli ping`
- Configurável via `appsettings.json` ConnectionStrings:Redis

## Autenticação JWT
- **Pacotes**: `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- **Security**: Senhas com SHA256 + Salt via `AuthService.HashPassword()`
- **Claims**: Id, Name, Email, Role, Department no token
- **Configuração**: JWT SecretKey, Issuer, Audience em `appsettings.json`
- **Swagger**: Integração com Bearer token para teste de endpoints

## CORS
Configurado para Angular em `localhost:4200` e `4201` - atualizar se necessário.

## Desenvolvimento
- .NET 9.0 com Nullable habilitado
- Swagger disponível em desenvolvimento
- Use Soft Delete (campo `Ativo`) em vez de exclusão física
- StatusCode 500 para erros não tratados, com details em development
- **Redis logs**: Console logs visuais para debugging de cache (🚀🗄️💾)