FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PollyResilience.ExternalService/PollyResilience.ExternalService.csproj", "PollyResilience.ExternalService/"]
RUN dotnet restore "PollyResilience.ExternalService/PollyResilience.ExternalService.csproj"
COPY . .
WORKDIR "/src/PollyResilience.ExternalService"
RUN dotnet build "PollyResilience.ExternalService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PollyResilience.ExternalService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PollyResilience.ExternalService.dll"]
ENV ASPNETCORE_URLS=http://+:5000
