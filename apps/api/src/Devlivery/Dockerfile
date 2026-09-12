FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Devlivery/Devlivery.csproj", "src/Devlivery/"]
RUN dotnet restore "src/Devlivery/Devlivery.csproj"
COPY . .
WORKDIR "/src/src/Devlivery"
RUN dotnet build "./Devlivery.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Devlivery.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build AS migrations
ARG BUILD_CONFIGURATION=Release
RUN dotnet tool restore
RUN dotnet ef migrations bundle \
    --project "./Devlivery.csproj" \
    --context ApplicationDbContext \
    --configuration $BUILD_CONFIGURATION \
    --output /app/migrations/migrate-application
RUN dotnet ef migrations bundle \
    --project "./Devlivery.csproj" \
    --context ApplicationIdentityDbContext \
    --configuration $BUILD_CONFIGURATION \
    --output /app/migrations/migrate-identity
RUN chmod +x /app/migrations/migrate-application /app/migrations/migrate-identity

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=migrations /app/migrations ./migrations
ENTRYPOINT ["dotnet", "Devlivery.dll"]
