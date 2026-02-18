FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/CommerceHub.InventoryService/CommerceHub.InventoryService.csproj", "CommerceHub.InventoryService/"]
COPY ["src/CommerceHub.BuildingBlocks/CommerceHub.BuildingBlocks.csproj", "CommerceHub.BuildingBlocks/"]
COPY ["src/CommerceHub.Contracts/CommerceHub.Contracts.csproj", "CommerceHub.Contracts/"]
RUN dotnet restore "CommerceHub.InventoryService/CommerceHub.InventoryService.csproj"
COPY . .
WORKDIR "/src/CommerceHub.InventoryService"
RUN dotnet build "CommerceHub.InventoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.InventoryService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.InventoryService.dll"]
