# Gestão Produtos API

## 🚀 **Backend em C# com Clean Architecture, DDD, MongoDB e JWT Authentication**

API REST completa para gerenciar produtos, clientes e usuários com sistema de autenticação JWT robusto, usando as melhores práticas de arquitetura e desenvolvimento.

## 🏗️ **Arquitetura**

### **Clean Architecture + DDD**
```
GestaoProdutos.API/          # Camada de apresentação (Web API)
├── Controllers/             # Controladores REST
│   ├── ProdutosController   # CRUD de produtos
│   ├── ClientesController   # CRUD de clientes  
│   └── AuthController       # Autenticação JWT
└── Program.cs              # Configuração da aplicação

GestaoProdutos.Application/  # Camada de aplicação
├── DTOs/                   # Data Transfer Objects
├── Interfaces/             # Contratos de serviços
└── Services/               # Implementação dos serviços
    ├── ProdutoService      # Lógica de produtos
    ├── ClienteService      # Lógica de clientes
    └── AuthService         # Lógica de autenticação

GestaoProdutos.Domain/       # Camada de domínio (Core)
├── Entities/               # Entidades de negócio
│   ├── Produto            # Produto com métodos de domínio
│   ├── Cliente            # Cliente com validações
│   └── Usuario            # Usuário com roles e permissões
├── Enums/                  # Enumerações
├── ValueObjects/           # Objetos de valor (Email, CpfCnpj)
└── Interfaces/             # Contratos de repositórios

GestaoProdutos.Infrastructure/ # Camada de infraestrutura
├── Data/                   # Contexto do MongoDB
└── Repositories/           # Implementação dos repositórios
```

## 🛠️ **Tecnologias Utilizadas**

- **.NET 9** - Framework principal
- **ASP.NET Core Web API** - API REST
- **JWT Bearer Authentication** - Autenticação com tokens JWT
- **MongoDB** - Banco de dados NoSQL
- **MongoDB.Driver** - Driver oficial do MongoDB
- **Swagger/OpenAPI** - Documentação automática com autenticação
- **CORS** - Habilitado para frontend Angular
- **xUnit + FluentAssertions** - Testes automatizados

## 🔐 **Sistema de Autenticação**

### **JWT (JSON Web Tokens)**
- ✅ **Login/Register** - Autenticação completa
- ✅ **Password Hash** - Senhas seguras com SHA256
- ✅ **Token Validation** - Middleware de validação
- ✅ **Claims** - ID, Nome, Email, Role, Departamento
- ✅ **Authorization** - Controle de acesso por roles
- ✅ **Password Reset** - Recuperação de senha
- ✅ **Swagger Integration** - Teste direto na documentação

### **Endpoints de Autenticação**
```
POST /api/auth/login          # Login no sistema
POST /api/auth/register       # Cadastrar usuário
POST /api/auth/forgot-password # Solicitar reset de senha
POST /api/auth/reset-password  # Resetar senha com token
POST /api/auth/change-password # Alterar senha (autenticado)
GET  /api/auth/me             # Dados do usuário logado
POST /api/auth/validate-token # Validar token JWT
```

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
- DataCriacao/Atualizacao: DateTime
```

### **Usuario**
```csharp
- Id: string (ObjectId)
- Nome: string
- Email: Email (Value Object)
- Role: UserRole (Admin/Manager/User)
- Avatar: string (URL da imagem)
- Departamento: string
- UltimoLogin: DateTime?
- SenhaHash: string (SHA256 + Salt)
- Ativo: bool
- DataCriacao/Atualizacao: DateTime
```

## 🌐 **Endpoints da API**

### **🔐 Autenticação** (`/api/auth`)
- `POST /login` - Login no sistema (retorna JWT)
- `POST /register` - Cadastrar novo usuário
- `POST /forgot-password` - Solicitar reset de senha
- `POST /reset-password` - Resetar senha com token
- `POST /change-password` - Alterar senha (🔒 autenticado)
- `GET /me` - Dados do usuário logado (🔒 autenticado)
- `POST /validate-token` - Validar token JWT (🔒 autenticado)

### **👥 Usuários** (`/api/users`)
- `GET /` - Listar todos os usuários (🔒 admin)
- `GET /{id}` - Obter usuário por ID (🔒 próprio usuário ou admin)
- `POST /` - Criar novo usuário (🔒 admin)
- `PUT /{id}` - Atualizar usuário (🔒 próprio usuário ou admin)
- `DELETE /{id}` - Desativar usuário (🔒 admin)
- `PATCH /{id}/activate` - Reativar usuário (🔒 admin)
- `GET /department/{department}` - Usuários por departamento (🔒 manager+)
- `GET /role/{role}` - Usuários por role (🔒 admin)

### **📦 Produtos** (`/api/produtos`)
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
- .NET 9 SDK
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
  },
  "JWT": {
    "SecretKey": "SuaChaveSecretaSuperSegura123456789",
    "Issuer": "GestaoProdutosAPI",
    "Audience": "GestaoProdutosClient",
    "ExpirationHours": 24
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
# - HTTP: http://localhost:5278 (development)
# - HTTPS: https://localhost:5001 (production)
# - Swagger: http://localhost:5278 (raiz) - Acesso com autenticação JWT
```

### **4. Acesso ao Swagger com Autenticação**
1. Acesse `http://localhost:5278`
2. Clique em **"Authorize"** no canto superior direito
3. Primeiro, registre um usuário via endpoint `/api/auth/register`
4. Depois faça login via `/api/auth/login` para obter o token
5. Cole o token JWT no campo Authorization (formato: `Bearer seutoken`)
6. Agora você pode acessar endpoints protegidos! 🔐

## 🔄 **Integração com Frontend Angular**

### **CORS Configurado**
- `http://localhost:4200` (dev padrão)
- `http://localhost:4201` (dev alternativa)

### **Autenticação JWT**
O sistema está configurado para receber tokens JWT no header Authorization:
```typescript
// Configuração no Angular (service)
const token = localStorage.getItem('jwt-token');
const headers = new HttpHeaders({
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
});
```

### **Formato de DTOs compatível**
Os DTOs foram criados seguindo exatamente a estrutura do frontend Angular:

```typescript
// Frontend Angular - Produto
interface Product {
  id: string;
  name: string;
  sku: string;
  quantity: number;
  price: number;
  lastUpdated: Date;
}

// Frontend Angular - Auth
interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  user: {
    id: string;
    name: string;
    email: string;
    role: string;
  };
}
```

## 📋 **Features Implementadas**

### **Autenticação e Segurança**
- ✅ **JWT Authentication** completo
- ✅ Sistema de login e registro
- ✅ Recuperação de senha
- ✅ Hash de senhas com SHA256 + Salt
- ✅ Autorização baseada em Claims
- ✅ Swagger UI com autenticação integrada

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

- [x] ~~Implementar autenticação JWT~~ ✅ **Concluído!**
- [ ] Adicionar testes unitários e integração para Auth
- [ ] Implementar cache com Redis
- [ ] Adicionar logs estruturados (Serilog)
- [ ] Implementar paginação
- [ ] Adicionar métricas e monitoramento
- [ ] Implementar refresh tokens
- [ ] Deploy com Docker

---

**Desenvolvido com Clean Architecture, DDD, MongoDB e JWT Authentication para integração perfeita com o frontend Angular existente!** 🚀