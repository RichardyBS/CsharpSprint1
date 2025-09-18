# ========================================
# Script de Setup do Ambiente - Sistema de Estacionamento
# ========================================

Write-Host "🚀 Configurando ambiente do Sistema de Estacionamento..." -ForegroundColor Green

# Verificar se o arquivo .env existe
if (-not (Test-Path ".env")) {
    Write-Host "📋 Arquivo .env não encontrado. Copiando do template..." -ForegroundColor Yellow
    
    if (Test-Path ".env.example") {
        Copy-Item ".env.example" ".env"
        Write-Host "✅ Arquivo .env criado com sucesso!" -ForegroundColor Green
        Write-Host "⚠️  IMPORTANTE: Edite o arquivo .env com suas configurações reais!" -ForegroundColor Red
    } else {
        Write-Host "❌ Arquivo .env.example não encontrado!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✅ Arquivo .env já existe." -ForegroundColor Green
}

# Verificar se o Docker está rodando
Write-Host "🐳 Verificando se o Docker está rodando..." -ForegroundColor Cyan
try {
    docker version | Out-Null
    Write-Host "✅ Docker está rodando!" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker não está rodando. Inicie o Docker Desktop primeiro!" -ForegroundColor Red
    exit 1
}

# Verificar se o .NET SDK está instalado
Write-Host "🔧 Verificando .NET SDK..." -ForegroundColor Cyan
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK versão $dotnetVersion encontrado!" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK não encontrado. Instale o .NET 8 SDK!" -ForegroundColor Red
    exit 1
}

# Função para perguntar ao usuário
function Ask-User {
    param([string]$Question)
    $response = Read-Host "$Question (s/n)"
    return $response -eq 's' -or $response -eq 'S' -or $response -eq 'sim' -or $response -eq 'Sim'
}

# Perguntar se quer instalar dependências
if (Ask-User "📦 Deseja restaurar as dependências do projeto?") {
    Write-Host "📦 Restaurando dependências..." -ForegroundColor Cyan
    dotnet restore EstacionamentoMicroservices.sln
    Write-Host "✅ Dependências restauradas!" -ForegroundColor Green
}

# Perguntar se quer compilar o projeto
if (Ask-User "🔨 Deseja compilar o projeto?") {
    Write-Host "🔨 Compilando projeto..." -ForegroundColor Cyan
    dotnet build EstacionamentoMicroservices.sln
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Projeto compilado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro na compilação. Verifique os logs acima." -ForegroundColor Red
    }
}

# Perguntar se quer subir os containers
if (Ask-User "🐳 Deseja subir os containers Docker?") {
    Write-Host "🐳 Subindo containers..." -ForegroundColor Cyan
    docker-compose up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Containers iniciados com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🌐 Serviços disponíveis:" -ForegroundColor Yellow
        Write-Host "   • API Gateway: http://localhost:5000" -ForegroundColor White
        Write-Host "   • Analytics Service: http://localhost:5001" -ForegroundColor White
        Write-Host "   • Billing Service: http://localhost:5002" -ForegroundColor White
        Write-Host "   • Notification Service: http://localhost:5003" -ForegroundColor White
        Write-Host "   • RabbitMQ Management: http://localhost:15672" -ForegroundColor White
        Write-Host ""
        Write-Host "📊 Bancos de dados:" -ForegroundColor Yellow
        Write-Host "   • PostgreSQL: localhost:5432" -ForegroundColor White
        Write-Host "   • MongoDB: localhost:27017" -ForegroundColor White
        Write-Host "   • Redis: localhost:6379" -ForegroundColor White
    } else {
        Write-Host "❌ Erro ao iniciar containers. Verifique os logs." -ForegroundColor Red
    }
}

# Verificar status dos containers
if (Ask-User "📋 Deseja verificar o status dos containers?") {
    Write-Host "📋 Status dos containers:" -ForegroundColor Cyan
    docker-compose ps
}

Write-Host ""
Write-Host "🎉 Setup concluído!" -ForegroundColor Green
Write-Host ""
Write-Host "📚 Próximos passos:" -ForegroundColor Yellow
Write-Host "   1. Edite o arquivo .env com suas configurações" -ForegroundColor White
Write-Host "   2. Verifique se todos os serviços estão rodando" -ForegroundColor White
Write-Host "   3. Teste as APIs usando Swagger ou Postman" -ForegroundColor White
Write-Host ""
Write-Host "📖 Para mais informações, consulte:" -ForegroundColor Yellow
Write-Host "   • README.md - Documentação geral" -ForegroundColor White
Write-Host "   • ENVIRONMENT_SETUP.md - Configuração de ambiente" -ForegroundColor White
Write-Host ""
Write-Host "🆘 Em caso de problemas:" -ForegroundColor Yellow
Write-Host "   • Verifique os logs: docker-compose logs" -ForegroundColor White
Write-Host "   • Reinicie os containers: docker-compose restart" -ForegroundColor White
Write-Host "   • Pare tudo: docker-compose down" -ForegroundColor White