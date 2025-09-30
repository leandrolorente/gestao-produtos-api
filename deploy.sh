#!/bin/bash
# Script de deploy para produção

echo "🚀 Iniciando processo de deploy..."

# Verificar se Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Inicie o Docker Desktop primeiro."
    exit 1
fi

echo "✅ Docker está rodando"

# Build da imagem
echo "📦 Fazendo build da imagem..."
docker build -t gestao-produtos-api:latest .

if [ $? -ne 0 ]; then
    echo "❌ Erro no build da imagem"
    exit 1
fi

echo "✅ Build concluído com sucesso"

# Verificar se a imagem foi criada
if ! docker images | grep -q "gestao-produtos-api"; then
    echo "❌ Imagem não foi encontrada após o build"
    exit 1
fi

echo "✅ Imagem gestao-produtos-api criada com sucesso"

# Mostrar tamanho da imagem
echo "📏 Tamanho da imagem:"
docker images gestao-produtos-api:latest

# Testar a imagem localmente (opcional)
echo "🧪 Deseja testar a imagem localmente? (y/n)"
read -r test_local

if [ "$test_local" = "y" ] || [ "$test_local" = "Y" ]; then
    echo "🧪 Iniciando teste local..."
    
    # Parar container existente se houver
    docker stop gestao-produtos-api-test 2>/dev/null
    docker rm gestao-produtos-api-test 2>/dev/null
    
    # Executar container de teste
    docker run -d \
        --name gestao-produtos-api-test \
        -p 8080:8080 \
        -e ASPNETCORE_ENVIRONMENT=Production \
        -e ConnectionStrings__MongoDB="mongodb://localhost:27017/GestaoProdutosDB" \
        -e JWT__Secret="teste-jwt-secret-key-32-caracteres-min" \
        -e JWT__Issuer="GestaoProdutosAPI" \
        -e JWT__Audience="GestaoProdutosApp" \
        gestao-produtos-api:latest
    
    if [ $? -eq 0 ]; then
        echo "✅ Container de teste iniciado"
        echo "🌐 API disponível em: http://localhost:8080"
        echo "❤️ Health check: http://localhost:8080/health"
        echo ""
        echo "Pressione Enter para parar o teste e continuar..."
        read -r
        
        # Parar container de teste
        docker stop gestao-produtos-api-test
        docker rm gestao-produtos-api-test
        echo "✅ Container de teste removido"
    else
        echo "❌ Erro ao iniciar container de teste"
        exit 1
    fi
fi

# Instruções para deploy no Render
echo ""
echo "🎯 PRÓXIMOS PASSOS PARA DEPLOY NO RENDER:"
echo "1. Faça push do código para o GitHub"
echo "2. No Render, crie um novo Web Service"
echo "3. Conecte seu repositório GitHub"
echo "4. Configure Environment: Docker"
echo "5. Configure as variáveis de ambiente:"
echo "   - ASPNETCORE_ENVIRONMENT=Production"
echo "   - ASPNETCORE_URLS=http://+:8080"
echo "   - ConnectionStrings__MongoDB=<sua_mongodb_atlas_url>"
echo "   - JWT__Secret=<sua_chave_jwt_segura>"
echo "   - CORS__AllowedOrigins=<url_do_seu_frontend>"
echo ""
echo "📋 Para gerar JWT Secret seguro:"
echo "   openssl rand -base64 48"
echo ""
echo "🎉 Deploy local concluído! Pronto para produção."