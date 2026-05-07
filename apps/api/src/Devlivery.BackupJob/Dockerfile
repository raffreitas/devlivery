FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Devlivery.BackupJob/Devlivery.BackupJob.csproj", "src/Devlivery.BackupJob/"]
RUN dotnet restore "src/Devlivery.BackupJob/Devlivery.BackupJob.csproj"
COPY . .
WORKDIR "/src/src/Devlivery.BackupJob"
RUN dotnet build "./Devlivery.BackupJob.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Devlivery.BackupJob.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Devlivery.BackupJob.dll"]
