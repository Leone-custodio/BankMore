FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything and restore
COPY . ./
RUN dotnet restore

# Build and publish
RUN dotnet publish src/BankMore.Api/BankMore.Api.csproj -c Release -o out

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "BankMore.Api.dll"]
