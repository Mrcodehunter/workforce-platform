# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (including shared project)
COPY ["Workforce.Shared/Workforce.Shared.csproj", "Workforce.Shared/"]
COPY ["WorkerService.AuditLogger/WorkerService.AuditLogger.csproj", "WorkerService.AuditLogger/"]
RUN dotnet restore "WorkerService.AuditLogger/WorkerService.AuditLogger.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/WorkerService.AuditLogger"
RUN dotnet build "WorkerService.AuditLogger.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WorkerService.AuditLogger.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerService.AuditLogger.dll"]
