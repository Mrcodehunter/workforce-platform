# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (including shared project)
COPY ["Workforce.Shared/Workforce.Shared.csproj", "Workforce.Shared/"]
COPY ["WorkforceAPI/WorkforceAPI.csproj", "WorkforceAPI/"]
RUN dotnet restore "WorkforceAPI/WorkforceAPI.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/WorkforceAPI"
RUN dotnet build "WorkforceAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WorkforceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkforceAPI.dll"]
