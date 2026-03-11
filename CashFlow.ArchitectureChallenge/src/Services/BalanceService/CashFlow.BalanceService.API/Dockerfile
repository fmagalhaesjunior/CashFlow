# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/BalanceService/CashFlow.BalanceService.API/CashFlow.BalanceService.API.csproj", "src/Services/BalanceService/CashFlow.BalanceService.API/"]
COPY ["src/Services/BalanceService/CashFlow.BalanceService.Application/CashFlow.BalanceService.Application.csproj", "src/Services/BalanceService/CashFlow.BalanceService.Application/"]
COPY ["src/BuildingBlocks/CashFlow.BuildingBlocks.Contracts/CashFlow.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/CashFlow.BuildingBlocks.Contracts/"]
COPY ["src/Services/BalanceService/CashFlow.BalanceService.Domain/CashFlow.BalanceService.Domain.csproj", "src/Services/BalanceService/CashFlow.BalanceService.Domain/"]
COPY ["src/Services/TransactionService/CashFlow.TransactionService.Domain/CashFlow.TransactionService.Domain.csproj", "src/Services/TransactionService/CashFlow.TransactionService.Domain/"]
COPY ["src/BuildingBlocks/CashFlow.BuildingBlocks.Domain/CashFlow.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/CashFlow.BuildingBlocks.Domain/"]
COPY ["src/Services/BalanceService/CashFlow.BalanceService.Infrastructure/CashFlow.BalanceService.Infrastructure.csproj", "src/Services/BalanceService/CashFlow.BalanceService.Infrastructure/"]
RUN dotnet restore "./src/Services/BalanceService/CashFlow.BalanceService.API/CashFlow.BalanceService.API.csproj"
COPY . .
WORKDIR "/src/src/Services/BalanceService/CashFlow.BalanceService.API"
RUN dotnet build "./CashFlow.BalanceService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CashFlow.BalanceService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.BalanceService.API.dll"]