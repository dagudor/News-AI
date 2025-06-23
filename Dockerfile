FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
EXPOSE 10000

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Comando de inicio con logs
CMD ["sh", "-c", "echo 'Starting NewsAI...' && dotnet NewsAI.dll"]