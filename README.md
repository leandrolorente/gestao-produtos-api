# Gestão Produtos API

## 🚀 **Backend em C# com Clean Architecture, DDD e MongoDB**

API REST desenvolvida para gerenciar produtos e clientes, usando as melhores práticas de arquitetura e desenvolvimento.

## 🏗️ **Arquitetura**

### **Clean Architecture + DDD**
```
GestaoProdutos.API/          # Camada de apresentação (Web API)
├── Controllers/             # Controladores REST
└── Program.cs              # Configuração da aplicação

GestaoProdutos.Application/  # Camada de aplicação
├── DTOs/                   # Data Transfer Objects
├── Interfaces/             # Contratos de serviços
└── Services/               # Implementação dos serviços

GestaoProdutos.Domain/       # Camada de domínio (Core)
├── Entities/               # Entidades de negócio
├── Enums/                  # Enumerações
├── ValueObjects/           # Objetos de valor
└── Interfaces/             # Contratos de repositórios

GestaoProdutos.Infrastructure/ # Camada de infraestrutura
├── Data/                   # Contexto do MongoDB
└── Repositories/           # Implementação dos repositórios
```

## 🛠️ **Tecnologias Utilizadas**

- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - API REST
- **MongoDB** - Banco de dados NoSQL
- **MongoDB.Driver** - Driver oficial do MongoDB
- **Swagger/OpenAPI** - Documentação automática
- **CORS** - Habilitado para frontend Angular

## 📊 **Modelos de Dados**

### **Produto**
```csharp
- Id: string (ObjectId)
- Nome: string
- Sku: string (único)
- Quantidade: int
- Preco: decimal
- Categoria: string?
- EstoqueMinimo: int?
- Status: StatusProduto
- DataCriacao/Atualizacao: DateTime
```

### **Cliente**
```csharp
- Id: string (ObjectId)
- Nome: string
- Email: Email (Value Object)
- Telefone: string
- CpfCnpj: CpfCnpj (Value Object)
- Endereco: Endereco (Value Object)
- Tipo: TipoCliente (PF/PJ)
- UltimaCompra: DateTime?
- Ativo: bool
```

### **Usuário**
```csharp
- Id: string (ObjectId)
- Nome: string
- Email: Email (Value Object)
- Role: UserRole (Admin/Manager/User)
- Departamento: string
- UltimoLogin: DateTime?
- Ativo: bool
```

## 🌐 **Endpoints Principais**

### **Produtos** (`/api/produtos`)
- `GET /` - Listar todos os produtos
- `GET /{id}` - Obter produto por ID
- `GET /sku/{sku}` - Obter produto por SKU
- `GET /estoque-baixo` - Produtos com estoque baixo
- `POST /` - Criar novo produto
- `PUT /{id}` - Atualizar produto
- `DELETE /{id}` - Excluir produto (soft delete)
- `PATCH /{id}/estoque` - Atualizar estoque

### **Clientes** (`/api/clientes`)
- `GET /` - Listar todos os clientes
- `GET /{id}` - Obter cliente por ID
- `GET /cpf-cnpj/{cpfCnpj}` - Obter cliente por documento
- `GET /tipo/{tipo}` - Clientes por tipo (PF/PJ)
- `GET /compra-recente` - Clientes com compra recente
- `POST /` - Criar novo cliente
- `PUT /{id}` - Atualizar cliente
- `DELETE /{id}` - Excluir cliente (soft delete)
- `PATCH /{id}/toggle-status` - Alternar status
- `PATCH /{id}/registrar-compra` - Registrar compra

## ⚙️ **Configuração e Execução**

### **Pré-requisitos**
- .NET 8 SDK
- MongoDB (local ou MongoDB Atlas)
- Visual Studio Code ou Visual Studio

### **1. Configurar MongoDB**
```bash
# Instalar MongoDB localmente ou usar MongoDB Atlas
# String de conexão padrão: mongodb://localhost:27017
```

### **2. Configurar appsettings.json**
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "GestaoProdutosDB"
  }
}
```

### **3. Restaurar pacotes e executar**
```bash
# Restaurar dependências
dotnet restore

# Executar a API
dotnet run --project GestaoProdutos.API

# API executará em:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
# - Swagger: https://localhost:5001 (raiz)
```

## 🔄 **Integração com Frontend Angular**

### **CORS Configurado**
- `http://localhost:4200` (dev padrão)
- `http://localhost:4201` (dev alternativa)

### **Formato de DTOs compatível**
Os DTOs foram criados seguindo exatamente a estrutura do frontend Angular:

```typescript
// Frontend Angular (TypeScript)
interface Product {
  id: number;
  name: string;
  sku: string;
  quantity: number;
  price: number;
  lastUpdated: Date;
}

// Backend C# (DTO)
public record ProdutoDto {
  public string Id { get; init; }
  public string Name { get; init; }
  public string Sku { get; init; }
  public int Quantity { get; init; }
  public decimal Price { get; init; }
  public DateTime LastUpdated { get; init; }
}
```

## 📋 **Features Implementadas**

### **Domain-Driven Design**
- ✅ Entidades com métodos de negócio
- ✅ Value Objects (Email, CpfCnpj, Endereco)
- ✅ Enums tipados
- ✅ Validações de domínio

### **Clean Architecture**
- ✅ Separação clara de responsabilidades
- ✅ Inversão de dependências
- ✅ Testes unitários preparados
- ✅ Facilidade de manutenção

### **MongoDB Integration**
- ✅ Índices automáticos
- ✅ Soft delete
- ✅ Consultas otimizadas
- ✅ ObjectId como chave primária

### **API Features**
- ✅ Documentação Swagger automática
- ✅ Tratamento de erros padronizado
- ✅ Validação de modelos
- ✅ CORS configurado
- ✅ Logs estruturados

## 🧪 **Próximos Passos**

- [ ] Implementar autenticação JWT
- [ ] Adicionar testes unitários e integração
- [ ] Implementar cache com Redis
- [ ] Adicionar logs estruturados (Serilog)
- [ ] Implementar paginação
- [ ] Adicionar métricas e monitoramento
- [ ] Deploy com Docker

---

**Desenvolvido com Clean Architecture, DDD e MongoDB para integração perfeita com o frontend Angular existente!** 🚀