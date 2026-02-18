FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CommerceHub.IdentityService/CommerceHub.IdentityService.csproj", "src/CommerceHub.IdentityService/"]
COPY ["src/CommerceHub.BuildingBlocks/CommerceHub.BuildingBlocks.csproj", "src/CommerceHub.BuildingBlocks/"]
RUN dotnet restore "src/CommerceHub.IdentityService/CommerceHub.IdentityService.csproj"
COPY . .
WORKDIR "/src/src/CommerceHub.IdentityService"
RUN dotnet build "./CommerceHub.IdentityService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CommerceHub.IdentityService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.IdentityService.dll"]
