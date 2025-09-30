@echo off
REM Script de deploy para Windows

echo 🚀 Iniciando processo de deploy...

REM Verificar se Docker está rodando
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker não está rodando. Inicie o Docker Desktop primeiro.
    exit /b 1
)

echo ✅ Docker está rodando

REM Build da imagem
echo 📦 Fazendo build da imagem...
docker build -t gestao-produtos-api:latest .

if errorlevel 1 (
    echo ❌ Erro no build da imagem
    exit /b 1
)

echo ✅ Build concluído com sucesso

REM Verificar se a imagem foi criada
docker images | findstr "gestao-produtos-api" >nul
if errorlevel 1 (
    echo ❌ Imagem não foi encontrada após o build
    exit /b 1
)

echo ✅ Imagem gestao-produtos-api criada com sucesso

REM Mostrar tamanho da imagem
echo 📏 Tamanho da imagem:
docker images gestao-produtos-api:latest

REM Testar a imagem localmente (opcional)
set /p test_local="🧪 Deseja testar a imagem localmente? (y/n): "

if /i "%test_local%"=="y" (
    echo 🧪 Iniciando teste local...
    
    REM Parar container existente se houver
    docker stop gestao-produtos-api-test >nul 2>&1
    docker rm gestao-produtos-api-test >nul 2>&1
    
    REM Executar container de teste
    docker run -d ^
        --name gestao-produtos-api-test ^
        -p 8080:8080 ^
        -e ASPNETCORE_ENVIRONMENT=Production ^
        -e ConnectionStrings__MongoDB="mongodb://localhost:27017/GestaoProdutosDB" ^
        -e JWT__Secret="teste-jwt-secret-key-32-caracteres-min" ^
        -e JWT__Issuer="GestaoProdutosAPI" ^
        -e JWT__Audience="GestaoProdutosApp" ^
        gestao-produtos-api:latest
    
    if errorlevel 0 (
        echo ✅ Container de teste iniciado
        echo 🌐 API disponível em: http://localhost:8080
        echo ❤️ Health check: http://localhost:8080/health
        echo.
        pause
        
        REM Parar container de teste
        docker stop gestao-produtos-api-test
        docker rm gestao-produtos-api-test
        echo ✅ Container de teste removido
    ) else (
        echo ❌ Erro ao iniciar container de teste
        exit /b 1
    )
)

REM Instruções para deploy no Render
echo.
echo 🎯 PRÓXIMOS PASSOS PARA DEPLOY NO RENDER:
echo 1. Faça push do código para o GitHub
echo 2. No Render, crie um novo Web Service
echo 3. Conecte seu repositório GitHub
echo 4. Configure Environment: Docker
echo 5. Configure as variáveis de ambiente:
echo    - ASPNETCORE_ENVIRONMENT=Production
echo    - ASPNETCORE_URLS=http://+:8080
echo    - ConnectionStrings__MongoDB=^<sua_mongodb_atlas_url^>
echo    - JWT__Secret=^<sua_chave_jwt_segura^>
echo    - CORS__AllowedOrigins=^<url_do_seu_frontend^>
echo.
echo 📋 Para gerar JWT Secret seguro use uma ferramenta online ou:
echo    PowerShell: [System.Web.Security.Membership]::GeneratePassword(64, 0)
echo.
echo 🎉 Deploy local concluído! Pronto para produção.
pause