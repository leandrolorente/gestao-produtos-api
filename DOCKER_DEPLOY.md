# 🐳 Docker Setup & Deploy - Gestão Produtos API

## 📋 Visão Geral

Este projeto foi configurado para ser executado em containers Docker, facilitando o deploy em plataformas como Render, Railway, Heroku e outras.

## 🛠️ Configuração Local com Docker

### Pré-requisitos
- Docker Desktop instalado
- Docker Compose instalado

### 🚀 Executar Localmente

```bash
# 1. Clone o repositório
git clone <seu-repositorio>
cd gestao-produtos-api

# 2. Executar com Docker Compose (inclui MongoDB)
docker-compose up -d

# 3. A API estará disponível em:
# http://localhost:5000 - API
# http://localhost:8081 - MongoDB Express (admin/pass)
```

### 🔍 Verificar Logs
```bash
# Logs da API
docker-compose logs -f api

# Logs do MongoDB
docker-compose logs -f mongodb

# Logs de todos os serviços
docker-compose logs -f
```

### 🛑 Parar os Serviços
```bash
docker-compose down

# Para remover volumes (apaga dados do banco)
docker-compose down -v
```

## 🏭 Deploy no Render

### 1. Configuração no Render

1. **Criar novo Web Service** no [Render](https://render.com)
2. **Conectar repositório** do GitHub
3. **Configurar as seguintes opções:**
   - **Environment**: `Docker`
   - **Dockerfile Path**: `Dockerfile` (na raiz)
   - **Build Command**: (deixar vazio, o Docker cuida)
   - **Start Command**: (deixar vazio, o Docker cuida)

### 2. Variáveis de Ambiente no Render

Configure as seguintes variáveis no painel do Render:

```env
# Ambiente
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# MongoDB (MongoDB Atlas recomendado)
ConnectionStrings__MongoDB=mongodb+srv://usuario:senha@cluster.mongodb.net/GestaoProdutosDB?retryWrites=true&w=majority
MongoDB__DatabaseName=GestaoProdutosDB

# JWT (GERE UMA CHAVE SEGURA!)
JWT__Secret=SUA_CHAVE_SUPER_SECRETA_COM_PELO_MENOS_32_CARACTERES_AQUI
JWT__Issuer=GestaoProdutosAPI
JWT__Audience=GestaoProdutosApp

# CORS (URL do seu frontend)
CORS__AllowedOrigins=https://seu-frontend.netlify.app,https://seu-frontend.vercel.app
```

### 3. MongoDB Atlas (Recomendado para Produção)

1. **Criar conta** no [MongoDB Atlas](https://www.mongodb.com/atlas)
2. **Criar cluster** gratuito
3. **Configurar IP whitelist** (0.0.0.0/0 para Render)
4. **Criar usuário** de banco de dados
5. **Obter connection string** e configurar na variável `ConnectionStrings__MongoDB`

### 4. Deploy Automático

Após configurar, cada push na branch `main` acionará um novo deploy automaticamente.

## 🔐 Segurança em Produção

### Variáveis Críticas
```env
# ⚠️ NUNCA use essas chaves em produção!
JWT__Secret=GERE_UMA_CHAVE_ALEATORIA_DE_64_CARACTERES
```

### Gerar Chave JWT Segura
```bash
# PowerShell
[System.Web.Security.Membership]::GeneratePassword(64, 0)

# Linux/Mac
openssl rand -base64 48
```

## 🏗️ Build Manual do Docker

```bash
# Build da imagem
docker build -t gestao-produtos-api .

# Executar container
docker run -d \
  --name api \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__MongoDB="sua_connection_string" \
  -e JWT__Secret="sua_chave_jwt" \
  gestao-produtos-api
```

## 📊 Monitoramento

### Health Check
A API possui um endpoint de health check:
```
GET /health
```
Retorna `200 OK` se a aplicação estiver funcionando.

### Logs
```bash
# Logs do container
docker logs gestao-produtos-api -f

# Logs estruturados
docker logs gestao-produtos-api --since=1h
```

## 🌐 URLs dos Serviços

### Local (Docker Compose)
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000
- **MongoDB Express**: http://localhost:8081
- **Health Check**: http://localhost:5000/health

### Produção (Render)
- **API**: https://seu-app.onrender.com
- **Swagger**: https://seu-app.onrender.com (apenas em dev)
- **Health Check**: https://seu-app.onrender.com/health

## 🔧 Configurações Adicionais

### Multi-stage Build
O Dockerfile usa multi-stage build para otimizar o tamanho da imagem:
- **Stage 1**: Build com SDK completo
- **Stage 2**: Runtime com apenas ASP.NET Core

### Otimizações de Performance
- Imagem base `mcr.microsoft.com/dotnet/aspnet:9.0`
- Usuário não-root para segurança
- Health checks configurados
- Logs estruturados

## 🐛 Troubleshooting

### Problemas Comuns

1. **Erro de conexão MongoDB**
   ```bash
   # Verificar se MongoDB está rodando
   docker-compose ps
   
   # Verificar logs do MongoDB
   docker-compose logs mongodb
   ```

2. **Erro de porta em uso**
   ```bash
   # Verificar portas em uso
   netstat -an | findstr :5000
   
   # Matar processos na porta
   npx kill-port 5000
   ```

3. **Erro de JWT Secret**
   ```bash
   # Verificar variáveis de ambiente
   docker exec -it gestao-produtos-api env | grep JWT
   ```

### Reconstruir Containers
```bash
# Rebuild forçado
docker-compose build --no-cache

# Rebuild e restart
docker-compose up --build
```

## 📝 Comandos Úteis

```bash
# Ver tamanho das imagens
docker images gestao-produtos-api

# Entrar no container
docker exec -it gestao-produtos-api bash

# Verificar recursos
docker stats gestao-produtos-api

# Remover todos os containers e imagens
docker system prune -a
```

## 🌟 Features do Docker Setup

- ✅ Multi-stage build otimizado
- ✅ Usuário não-root para segurança
- ✅ Health checks configurados
- ✅ Logs estruturados
- ✅ MongoDB local para desenvolvimento
- ✅ MongoDB Express para admin
- ✅ Suporte a MongoDB Atlas
- ✅ Deploy automático no Render
- ✅ Configuração de CORS flexível
- ✅ Variáveis de ambiente seguras

## 📞 Suporte

Para problemas relacionados ao Docker ou deploy:
1. Verificar logs dos containers
2. Validar variáveis de ambiente
3. Testar conexão com MongoDB
4. Verificar health check endpoint