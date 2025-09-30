# 🚀 Deploy Rápido - Render

## ⚡ Passos Rápidos

### 1. Preparar Repositório
```bash
git add .
git commit -m "feat: Docker setup e configuração para deploy"
git push origin main
```

### 2. Configurar MongoDB Atlas (GRÁTIS)
1. Acesse [MongoDB Atlas](https://www.mongodb.com/atlas)
2. Crie conta gratuita
3. Crie cluster grátis (512MB)
4. Configure usuário: `gestao_user` / `senha_forte_123`
5. IP Whitelist: `0.0.0.0/0` (todos IPs)
6. Copie a Connection String:
```
mongodb+srv://gestao_user:senha_forte_123@cluster0.xxxxx.mongodb.net/GestaoProdutosDB?retryWrites=true&w=majority
```

### 3. Deploy no Render
1. Acesse [Render](https://render.com)
2. Crie conta gratuita (GitHub OAuth)
3. **New** → **Web Service**
4. Conecte seu repositório GitHub
5. Configure:
   - **Name**: `gestao-produtos-api`
   - **Environment**: `Docker`
   - **Branch**: `main`
   - **Docker Command**: (deixar vazio)

### 4. Variáveis de Ambiente (CRÍTICO!)
Configure no Render dashboard:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__MongoDB=mongodb+srv://gestao_user:senha_forte_123@cluster0.xxxxx.mongodb.net/GestaoProdutosDB?retryWrites=true&w=majority
MongoDB__DatabaseName=GestaoProdutosDB
JWT__Secret=GERE_UMA_CHAVE_SEGURA_DE_64_CARACTERES_AQUI
JWT__Issuer=GestaoProdutosAPI
JWT__Audience=GestaoProdutosApp
CORS__AllowedOrigins=https://seu-frontend.netlify.app,https://seu-frontend.vercel.app
```

### 5. Gerar JWT Secret Seguro
```bash
# Linux/Mac
openssl rand -base64 48

# PowerShell
[System.Web.Security.Membership]::GeneratePassword(64, 0)

# Online
https://www.allkeysgenerator.com/Random/Security-Encryption-Key-Generator.aspx
```

### 6. Primeiro Deploy
- Clique **Deploy**
- Aguarde ~5-10 minutos
- Acesse sua URL: `https://gestao-produtos-api.onrender.com`

### 7. Testar API
```bash
# Health Check
curl https://gestao-produtos-api.onrender.com/health

# Swagger (apenas em dev)
https://gestao-produtos-api.onrender.com/swagger
```

## 🔧 Configuração do Frontend

### Atualizar URLs no Angular
```typescript
// environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://gestao-produtos-api.onrender.com/api'
};
```

### Configurar CORS
No Render, atualize a variável:
```env
CORS__AllowedOrigins=https://seu-app.netlify.app,https://seu-app.vercel.app
```

## ⚠️ Limitações do Plano Free

- **Render Free**: Dorme após 15min inativo, cold start ~30s
- **MongoDB Atlas Free**: 512MB storage, 100 conexões
- **Solução**: Upgrade para planos pagos se necessário

## 🎯 URLs Finais

Após deploy bem-sucedido:
- **API**: `https://gestao-produtos-api.onrender.com`
- **Health**: `https://gestao-produtos-api.onrender.com/health`
- **Swagger**: Disponível apenas em desenvolvimento local

## 🐛 Troubleshooting

### Deploy Failed
1. Verificar Dockerfile
2. Verificar variáveis de ambiente
3. Verificar logs no Render dashboard

### Erro de Conexão MongoDB
1. Verificar Connection String
2. Verificar IP whitelist (0.0.0.0/0)
3. Verificar usuário/senha MongoDB

### Erro de CORS
1. Verificar variável `CORS__AllowedOrigins`
2. Incluir todas as URLs do frontend (com https://)

### Cold Start Lento
- Normal no plano free (15-30s primeiro acesso)
- Considere upgrade para plano pago

## ✅ Checklist Final

- [ ] Código no GitHub
- [ ] MongoDB Atlas configurado
- [ ] Render Web Service criado
- [ ] Variáveis de ambiente configuradas
- [ ] JWT Secret gerado seguramente
- [ ] CORS configurado com URLs do frontend
- [ ] Deploy realizado com sucesso
- [ ] Health check funcionando
- [ ] Frontend apontando para nova API

🎉 **API pronta para produção!**