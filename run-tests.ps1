# xUnit v3 uses Microsoft.Testing.Platform and must be run as an executable.
# On .NET 10 SDK 10.0.4xx, use 'dotnet run' rather than 'dotnet test'.
dotnet run --project tests/ReconciliationDashboard.Tests/ReconciliationDashboard.Tests.csproj @args
