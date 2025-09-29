# 🚀 Como Acessar a API - Passo a Passo

## 🔧 Problema Inicial: "Ovo e Galinha"
Você precisa de token para acessar os endpoints, mas precisa fazer login para obter o token. Solução: **endpoint público para criar o primeiro admin**.

## 📝 Passo 1: Criar o Primeiro Usuário Admin

### Endpoint Público (sem autenticação necessária):
```
POST /api/auth/setup-first-admin
Content-Type: application/json
```

### Body JSON:
```json
{
  "name": "Administrador",
  "email": "admin@empresa.com",
  "password": "Admin123!",
  "avatar": "admin-avatar.jpg", 
  "department": "TI",
  "role": "admin"
}
```

### Exemplo com curl:
```bash
curl -X POST "https://localhost:5001/api/auth/setup-first-admin" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Administrador",
    "email": "admin@empresa.com", 
    "password": "Admin123!",
    "avatar": "admin-avatar.jpg",
    "department": "TI",
    "role": "admin"
  }'
```

**⚠️ IMPORTANTE:** Este endpoint só funciona quando **NÃO** existe nenhum usuário no sistema. É um endpoint de segurança para o primeiro admin.

---

## 🔑 Passo 2: Fazer Login

### Endpoint Público:
```
POST /api/auth/login
Content-Type: application/json
```

### Body JSON:
```json
{
  "email": "admin@empresa.com",
  "password": "Admin123!"
}
```

### Resposta esperada:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "507f1f77bcf86cd799439011",
    "name": "Administrador",
    "email": "admin@empresa.com",
    "role": "admin",
    "department": "TI",
    "avatar": "admin-avatar.jpg",
    "lastUpdated": "2025-09-29T10:30:00Z"
  }
}
```

---

## 🛡️ Passo 3: Usar o Token no Swagger

### No Swagger UI:
1. **Acesse:** `https://localhost:5001/swagger`
2. **Clique em "Authorize" 🔒** (canto superior direito)
3. **Cole o token** no formato: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
4. **Clique "Authorize"** depois **"Close"**

### Para chamadas HTTP diretas:
```bash
# Exemplo de uso do token
curl -X GET "https://localhost:5001/api/users" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 📋 Endpoints Disponíveis Após Login

### 🔐 **AuthController** (alguns públicos, outros protegidos):
- `POST /api/auth/login` - ✅ **PÚBLICO** (fazer login)
- `POST /api/auth/setup-first-admin` - ✅ **PÚBLICO** (criar primeiro admin)
- `POST /api/auth/forgot-password` - ✅ **PÚBLICO** (recuperar senha)
- `POST /api/auth/reset-password` - ✅ **PÚBLICO** (resetar senha)
- `GET /api/auth/me` - 🔒 **PROTEGIDO** (dados do usuário atual)
- `POST /api/auth/validate-token` - 🔒 **PROTEGIDO** (validar token)
- `POST /api/auth/change-password` - 🔒 **PROTEGIDO** (alterar senha)

### 👥 **UsersController** (todos protegidos):
- `GET /api/users` - 🔒 **Admin/Manager** (listar usuários)
- `GET /api/users/{id}` - 🔒 **Admin/Manager/próprio usuário** (buscar por ID)
- `POST /api/users` - 🔒 **Admin** (criar usuário)
- `PUT /api/users/{id}` - 🔒 **Admin/próprio usuário** (atualizar)
- `DELETE /api/users/{id}` - 🔒 **Admin** (desativar usuário)
- `PATCH /api/users/{id}/activate` - 🔒 **Admin** (reativar usuário)
- `GET /api/users/department/{dept}` - 🔒 **Admin/Manager** (por departamento)
- `GET /api/users/role/{role}` - 🔒 **Admin** (por role)

---

## 🗂️ Estrutura dos DTOs

### UserCreateDto (para criar usuários):
```json
{
  "name": "string",
  "email": "string", 
  "password": "string",
  "avatar": "string",
  "department": "string",
  "role": "user|manager|admin"
}
```

### LoginDto:
```json
{
  "email": "string",
  "password": "string"
}
```

---

## ⚡ Teste Rápido no Swagger

1. **Criar primeiro admin:** Use `POST /api/auth/setup-first-admin`
2. **Fazer login:** Use `POST /api/auth/login`
3. **Copiar token** da resposta
4. **Authorize no Swagger** com `Bearer {token}`
5. **Testar endpoints protegidos** como `GET /api/users`

---

## 🚨 Troubleshooting

### "Invalid token":
- Verifique se copiou o token completo
- Certifique-se de incluir "Bearer " antes do token
- Verifique se o token não expirou (24h)

### "Unauthorized":
- Token ausente ou inválido
- Role insuficiente para o endpoint (ex: user tentando acessar endpoint de admin)

### "BadRequest" no setup-first-admin:
- Já existe usuário no sistema (use login normal)
- Campos obrigatórios faltando no JSON

---

**🎉 Agora você pode acessar todos os endpoints da API com seu token de admin!**