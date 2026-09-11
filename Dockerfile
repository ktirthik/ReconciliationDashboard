# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ReconciliationDashboard.Api/ReconciliationDashboard.Api.csproj", "src/ReconciliationDashboard.Api/"]
RUN dotnet restore "src/ReconciliationDashboard.Api/ReconciliationDashboard.Api.csproj"

COPY . .
RUN dotnet publish "src/ReconciliationDashboard.Api/ReconciliationDashboard.Api.csproj" \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReconciliationDashboard.Api.dll"]
