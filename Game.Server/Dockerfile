FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5214
EXPOSE 8081

ENV ConnectionStrings__Redis__ConnectionString=host.docker.internal:6379
ENV ASPNETCORE_URLS=http://+:5214

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Game.Server/Game.Server.csproj", "Game.Server/"]
COPY ["Game.Contracts/Game.Contracts.csproj", "Game.Contracts/"]
RUN dotnet restore "Game.Server/Game.Server.csproj"
COPY . .
WORKDIR "/src/Game.Server"
RUN dotnet build "Game.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Game.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Game.Server.dll"]
