# ========================================
# Script de Limpeza do Ambiente - Sistema de Estacionamento
# ========================================

Write-Host "🧹 Script de Limpeza do Ambiente" -ForegroundColor Red

# Função para perguntar ao usuário
function Ask-User {
    param([string]$Question)
    $response = Read-Host "$Question (s/n)"
    return $response -eq 's' -or $response -eq 'S' -or $response -eq 'sim' -or $response -eq 'Sim'
}

# Parar containers
if (Ask-User "🛑 Deseja parar todos os containers?") {
    Write-Host "🛑 Parando containers..." -ForegroundColor Yellow
    docker-compose down
    Write-Host "✅ Containers parados!" -ForegroundColor Green
}

# Remover containers
if (Ask-User "🗑️  Deseja remover os containers?") {
    Write-Host "🗑️  Removendo containers..." -ForegroundColor Yellow
    docker-compose down --remove-orphans
    Write-Host "✅ Containers removidos!" -ForegroundColor Green
}

# Remover volumes (CUIDADO: isso apaga os dados dos bancos)
if (Ask-User "⚠️  CUIDADO: Deseja remover os volumes (isso apagará TODOS os dados dos bancos)?") {
    Write-Host "⚠️  Removendo volumes..." -ForegroundColor Red
    docker-compose down -v
    Write-Host "✅ Volumes removidos!" -ForegroundColor Green
}

# Remover imagens
if (Ask-User "🖼️  Deseja remover as imagens Docker do projeto?") {
    Write-Host "🖼️  Removendo imagens..." -ForegroundColor Yellow
    
    # Listar imagens relacionadas ao projeto
    $images = docker images --filter "reference=*estacionamento*" -q
    if ($images) {
        docker rmi $images -f
        Write-Host "✅ Imagens removidas!" -ForegroundColor Green
    } else {
        Write-Host "ℹ️  Nenhuma imagem do projeto encontrada." -ForegroundColor Cyan
    }
}

# Limpar cache do Docker
if (Ask-User "🧽 Deseja limpar o cache do Docker?") {
    Write-Host "🧽 Limpando cache do Docker..." -ForegroundColor Yellow
    docker system prune -f
    Write-Host "✅ Cache limpo!" -ForegroundColor Green
}

# Limpar arquivos de build do .NET
if (Ask-User "🗂️  Deseja limpar os arquivos de build do .NET?") {
    Write-Host "🗂️  Limpando arquivos de build..." -ForegroundColor Yellow
    dotnet clean EstacionamentoMicroservices.sln
    
    # Remover pastas bin e obj
    Get-ChildItem -Path . -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force
    Get-ChildItem -Path . -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force
    
    Write-Host "✅ Arquivos de build limpos!" -ForegroundColor Green
}

# Remover arquivo .env (opcional)
if (Test-Path ".env") {
    if (Ask-User "🔐 Deseja remover o arquivo .env? (você precisará reconfigurá-lo)") {
        Remove-Item ".env"
        Write-Host "✅ Arquivo .env removido!" -ForegroundColor Green
    }
}

# Verificar status final
Write-Host ""
Write-Host "📊 Status final dos containers:" -ForegroundColor Cyan
docker ps -a --filter "name=estacionamento"

Write-Host ""
Write-Host "💾 Volumes restantes:" -ForegroundColor Cyan
docker volume ls --filter "name=estacionamento"

Write-Host ""
Write-Host "🖼️  Imagens restantes:" -ForegroundColor Cyan
docker images --filter "reference=*estacionamento*"

Write-Host ""
Write-Host "🎉 Limpeza concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "📚 Para reconfigurar o ambiente:" -ForegroundColor Yellow
Write-Host "   1. Execute: .\setup-environment.ps1" -ForegroundColor White
Write-Host "   2. Configure o arquivo .env" -ForegroundColor White
Write-Host "   3. Suba os containers: docker-compose up -d" -ForegroundColor White