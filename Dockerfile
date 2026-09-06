# syntax=docker/dockerfile:1

# Сборка: SDK нужен только здесь.
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder

WORKDIR /src

# Версия SDK пинится тем же global.json, что и на хосте: если базовый образ
# уедет ниже пина, сборка упадет здесь, а не соберется на другом SDK молча.
COPY global.json ./

# Сначала только csproj - слой с restore переиспользуется, пока не менялись зависимости.
COPY src/Messenger.Api/Messenger.Api.csproj src/Messenger.Api/
RUN dotnet restore src/Messenger.Api/Messenger.Api.csproj

COPY src/Messenger.Api/ src/Messenger.Api/
RUN dotnet publish src/Messenger.Api/Messenger.Api.csproj -c Release -o /app --no-restore

# Запуск: рантайм ASP.NET Core без SDK.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runner

WORKDIR /app
COPY --from=builder /app ./

# Kestrel в контейнере слушает 8080 по умолчанию; у нас 3001, как у снятого Nest.
ENV ASPNETCORE_HTTP_PORTS=3001

USER $APP_UID
EXPOSE 3001

ENTRYPOINT ["dotnet", "Messenger.Api.dll"]
