FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Analytics.Service/Analytics.Service.csproj", "src/Services/Analytics.Service/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.EventBus/Shared.EventBus.csproj", "src/Shared/Shared.EventBus/"]
RUN dotnet restore "src/Services/Analytics.Service/Analytics.Service.csproj"
COPY . .
WORKDIR "/src/src/Services/Analytics.Service"
RUN dotnet build "Analytics.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Analytics.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Service.dll"]