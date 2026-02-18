FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CommerceHub.CatalogService/CommerceHub.CatalogService.csproj", "src/CommerceHub.CatalogService/"]
COPY ["src/CommerceHub.BuildingBlocks/CommerceHub.BuildingBlocks.csproj", "src/CommerceHub.BuildingBlocks/"]
COPY ["src/CommerceHub.Contracts/CommerceHub.Contracts.csproj", "src/CommerceHub.Contracts/"]

RUN dotnet restore "src/CommerceHub.CatalogService/CommerceHub.CatalogService.csproj"

COPY . .
WORKDIR "/src/src/CommerceHub.CatalogService"
RUN dotnet publish "CommerceHub.CatalogService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.CatalogService.dll"]
