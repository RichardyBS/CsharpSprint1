#!/bin/bash

echo "🚀 Iniciando aplicação MottoSprint..."

# Navegar para diretório da aplicação
cd /var/www/mottonotificacao/MottoSprint

# Definir variáveis de ambiente
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
export ConnectionStrings__DefaultConnection="Server=sqlserver-mottonotificacao.database.windows.net,1433;Database=bd-mottonotificacao;User Id=admsql;Password=devops@Fiap2tds;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Restaurar dependências
echo "📦 Restaurando dependências..."
dotnet restore

# Executar migrations (se necessário)
echo "🗄️ Executando migrations..."
dotnet ef database update --no-build || echo "⚠️ Migrations não executadas (pode ser normal se não houver)"

# Compilar aplicação
echo "🔨 Compilando aplicação..."
dotnet build --configuration Release

# Iniciar aplicação
echo "▶️ Iniciando aplicação..."
echo "🌐 Aplicação estará disponível em: http://[IP-DA-VM]"
echo "📊 Swagger UI: http://[IP-DA-VM]/swagger"
echo ""
echo "Para parar a aplicação, pressione Ctrl+C"
echo ""

dotnet run --configuration Release --no-build