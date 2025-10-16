@echo off
echo 🚀 Iniciando deploy com Redis integrado...

echo 📋 Parando containers existentes...
docker-compose down

echo 🔨 Fazendo build da aplicação...
docker-compose build --no-cache

echo ▶️ Iniciando serviços...
docker-compose up -d

echo ⏳ Aguardando serviços iniciarem...
timeout /t 15 /nobreak > nul

echo 📊 Status dos containers:
docker-compose ps

echo 🔍 Testando conectividade...
echo Redis:
docker exec gestao-produtos-redis redis-cli ping

echo API:
curl -f http://localhost:5000/health 2>nul || echo API ainda não está respondendo

echo.
echo ✅ Deploy concluído!
echo 🌐 API disponível em: http://localhost:5000
echo 🗄️ MongoDB Express: http://localhost:8081 (admin/pass)
echo 🚀 Redis CLI: docker exec gestao-produtos-redis redis-cli

echo.
echo 📋 Comandos úteis:
echo   Ver logs da API: docker-compose logs -f api
echo   Ver logs do Redis: docker-compose logs -f redis
echo   Parar tudo: docker-compose down
echo   Limpar volumes: docker-compose down -v

pause