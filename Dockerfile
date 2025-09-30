# Usar a imagem oficial do .NET SDK para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar arquivos de projeto e restaurar dependências
COPY *.sln ./
COPY GestaoProdutos.API/*.csproj ./GestaoProdutos.API/
COPY GestaoProdutos.Application/*.csproj ./GestaoProdutos.Application/
COPY GestaoProdutos.Domain/*.csproj ./GestaoProdutos.Domain/
COPY GestaoProdutos.Infrastructure/*.csproj ./GestaoProdutos.Infrastructure/
COPY GestaoProdutos.Tests/*.csproj ./GestaoProdutos.Tests/

# Restaurar dependências
RUN dotnet restore

# Copiar todo o código fonte
COPY . .

# Build da aplicação
RUN dotnet build --configuration Release --no-restore

# Publicar a aplicação
RUN dotnet publish GestaoProdutos.API/GestaoProdutos.API.csproj --configuration Release --no-build --output /app/publish

# Usar a imagem runtime do .NET para execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Instalar ferramentas de diagnóstico (opcional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Criar usuário não-root para segurança
RUN addgroup --system --gid 1001 nodejs \
    && adduser --system --uid 1001 --ingroup nodejs nodejs

# Mudar ownership dos arquivos
RUN chown -R nodejs:nodejs /app
USER nodejs

# Expor a porta
EXPOSE 8080

# Configurar variáveis de ambiente para produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Ponto de entrada
ENTRYPOINT ["dotnet", "GestaoProdutos.API.dll"]