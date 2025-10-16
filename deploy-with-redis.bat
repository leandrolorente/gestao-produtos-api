@echo off
echo 🚀 Iniciando deploy otimizado (API + Redis)...

echo 📋 Parando containers existentes...
docker-compose down --remove-orphans

echo 🔨 Fazendo build da aplicação...
docker-compose build --no-cache

echo ▶️ Iniciando serviços...
docker-compose up -d

echo ⏳ Aguardando serviços iniciarem...
timeout /t 10 /nobreak > nul

echo 📊 Status dos containers:
docker-compose ps

echo 🔍 Testando conectividade...
echo Redis:
docker exec gestao-produtos-redis redis-cli ping

echo.
echo ✅ Deploy concluído!
echo 🌐 API disponível em: http://localhost:5000
echo � Redis funcionando localmente
echo �🗄️ MongoDB: Atlas (remoto)
echo � Swagger: http://localhost:5000/swagger

echo.
echo 📋 Comandos úteis:
echo   Ver logs da API: docker-compose logs -f api
echo   Ver logs do Redis: docker-compose logs -f redis
echo   Testar API: curl http://localhost:5000/api/produtos
echo   Parar tudo: docker-compose down
echo   Limpar volumes: docker-compose down -v

pause