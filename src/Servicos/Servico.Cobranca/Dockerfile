FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Billing.Service/Billing.Service.csproj", "src/Services/Billing.Service/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.EventBus/Shared.EventBus.csproj", "src/Shared/Shared.EventBus/"]
RUN dotnet restore "src/Services/Billing.Service/Billing.Service.csproj"
COPY . .
WORKDIR "/src/src/Services/Billing.Service"
RUN dotnet build "Billing.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Billing.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Billing.Service.dll"]