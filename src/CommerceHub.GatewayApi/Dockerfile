FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CommerceHub.GatewayApi/CommerceHub.GatewayApi.csproj", "src/CommerceHub.GatewayApi/"]
COPY ["src/CommerceHub.BuildingBlocks/CommerceHub.BuildingBlocks.csproj", "src/CommerceHub.BuildingBlocks/"]

RUN dotnet restore "src/CommerceHub.GatewayApi/CommerceHub.GatewayApi.csproj"

COPY . .
WORKDIR "/src/src/CommerceHub.GatewayApi"
RUN dotnet publish "CommerceHub.GatewayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.GatewayApi.dll"]
