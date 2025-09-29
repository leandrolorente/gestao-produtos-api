# 🚀 Setup Completo - Gestão Produtos API

## 📋 Checklist de Configuração

### 1. **Pré-requisitos** ✅
- [ ] .NET 9 SDK instalado
- [ ] MongoDB instalado (local) ou MongoDB Atlas configurado
- [ ] Visual Studio Code ou Visual Studio
- [ ] Postman ou Thunder Client (opcional)

### 2. **Configuração do Projeto** ⚙️

#### **2.1 Clonar e Restaurar**
```bash
# Restaurar dependências
dotnet restore

# Verificar se tudo está OK
dotnet build
```

#### **2.2 Configurar appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "GestaoProdutosDB"
  },
  "JWT": {
    "SecretKey": "SuaChaveSecretaSuperSegura123456789MinimoPara256Bits",
    "Issuer": "GestaoProdutosAPI",
    "Audience": "GestaoProdutosClient", 
    "ExpirationHours": 24
  }
}
```

⚠️ **IMPORTANTE**: A `SecretKey` deve ter pelo menos 32 caracteres para HS256!

#### **2.3 Configurar MongoDB**
```bash
# Se usando MongoDB local
mongod --dbpath="C:\data\db"

# Ou usar MongoDB Compass para interface gráfica
# Download: https://www.mongodb.com/products/compass
```

### 3. **Executar a API** 🚀

```bash
# Executar em modo development
dotnet run --project GestaoProdutos.API

# Ou com hot reload
dotnet watch --project GestaoProdutos.API
```

**API estará disponível em:**
- HTTP: `http://localhost:5278` (development)
- HTTPS: `https://localhost:5001` (production)
- **Swagger**: `http://localhost:5278` (raiz)

### 4. **Primeiro Uso - Configurar Autenticação** 🔐

#### **4.1 Acessar Swagger**
1. Abra `http://localhost:5278`
2. Clique em **"Authorize"** (cadeado no canto superior direito)

#### **4.2 Registrar Primeiro Usuário**
```http
POST /api/auth/register
{
  "name": "Admin Sistema",
  "email": "admin@empresa.com", 
  "password": "Admin123!",
  "department": "TI",
  "role": "Admin"
}
```

#### **4.3 Fazer Login**
```http
POST /api/auth/login
{
  "email": "admin@empresa.com",
  "password": "Admin123!"
}
```

#### **4.4 Copiar Token JWT**
- Copie o `token` da resposta
- Cole no Swagger Authorization: `Bearer seuTokenAqui`
- Agora você pode acessar todos os endpoints! 🎉

### 5. **Testar Endpoints** 🧪

#### **5.1 Criar um Produto**
```http
POST /api/produtos
{
  "name": "Notebook Dell",
  "sku": "NB-DELL-001", 
  "description": "Notebook Dell Inspiron 15",
  "category": "Informatica",
  "quantity": 10,
  "price": 2500.00,
  "minimumStock": 5
}
```

#### **5.2 Criar um Cliente**
```http
POST /api/clientes
{
  "name": "João Silva",
  "email": "joao@email.com",
  "phone": "(11) 99999-9999",
  "customerType": "PF",
  "document": "123.456.789-00"
}
```

### 6. **Integração com Frontend Angular** 🔄

#### **6.1 CORS Já Configurado**
- `http://localhost:4200` ✅
- `http://localhost:4201` ✅

#### **6.2 Exemplo de Service Angular**
```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5278/api/auth';
  
  constructor(private http: HttpClient) {}
  
  login(email: string, password: string) {
    return this.http.post(`${this.apiUrl}/login`, { email, password });
  }
  
  getAuthHeaders() {
    const token = localStorage.getItem('jwt-token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }
}
```

### 7. **Desenvolvimento e Debug** 🔧

#### **7.1 Executar Testes**
```bash
# Rodar todos os testes
dotnet test

# Rodar com coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### **7.2 Logs e Debug**
- Logs aparecem no console durante execução
- Use breakpoints no Visual Studio/VS Code
- Swagger mostra todos os requests/responses

#### **7.3 MongoDB Compass**
- Conecte em `mongodb://localhost:27017`
- Database: `GestaoProdutosDB`
- Collections: `produtos`, `clientes`, `usuarios`

### 8. **Solução de Problemas** 🚨

#### **Erro: "Unable to connect to MongoDB"**
```bash
# Verificar se MongoDB está rodando
mongod --version

# Verificar string de conexão no appsettings.json
```

#### **Erro: "JWT token is invalid"**
- Verificar se SecretKey tem 32+ caracteres
- Verificar se token não expirou (24h padrão)
- Verificar formato: `Bearer tokenaqui`

#### **Erro: "CORS policy"**
- Verificar se frontend está em `localhost:4200/4201`
- Adicionar nova origem em `Program.cs` se necessário

### 9. **Próximos Passos** 📈

- [ ] Implementar refresh tokens
- [ ] Adicionar roles de usuário avançadas  
- [ ] Implementar cache Redis
- [ ] Configurar logs estruturados (Serilog)
- [ ] Adicionar testes de integração para Auth
- [ ] Deploy com Docker

---

**🎯 API configurada com sucesso! Pronta para desenvolvimento e integração!** 🚀