FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CommerceHub.BackofficeApi/CommerceHub.BackofficeApi.csproj", "src/CommerceHub.BackofficeApi/"]
COPY ["src/CommerceHub.BuildingBlocks/CommerceHub.BuildingBlocks.csproj", "src/CommerceHub.BuildingBlocks/"]
COPY ["src/CommerceHub.Contracts/CommerceHub.Contracts.csproj", "src/CommerceHub.Contracts/"]

RUN dotnet restore "src/CommerceHub.BackofficeApi/CommerceHub.BackofficeApi.csproj"

COPY . .
WORKDIR "/src/src/CommerceHub.BackofficeApi"
RUN dotnet publish "CommerceHub.BackofficeApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.BackofficeApi.dll"]
