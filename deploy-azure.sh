#!/bin/bash

echo "🚀 Iniciando configuração da VM para MottoSprint..."

# Atualizar sistema
echo "📦 Atualizando sistema..."
sudo apt update && sudo apt upgrade -y

# Instalar dependências básicas
echo "🔧 Instalando dependências básicas..."
sudo apt install -y curl wget git unzip

# Instalar .NET 8
echo "⚡ Instalando .NET 8..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0

# Instalar Docker
echo "🐳 Instalando Docker..."
sudo apt install -y docker.io docker-compose
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker $USER

# Instalar Nginx (proxy reverso)
echo "🌐 Instalando Nginx..."
sudo apt install -y nginx
sudo systemctl start nginx
sudo systemctl enable nginx

# Criar diretório para aplicação
echo "📁 Criando diretório da aplicação..."
sudo mkdir -p /var/www/mottonotificacao
sudo chown -R $USER:$USER /var/www/mottonotificacao

# Clonar repositório (se necessário) ou preparar para upload
echo "📥 Preparando ambiente para aplicação..."
cd /var/www/mottoSprint

# Configurar Nginx como proxy reverso
echo "⚙️ Configurando Nginx..."
sudo tee /etc/nginx/sites-available/mottoSprint > /dev/null <<EOF
server {
    listen 80;
    server_name _;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# Ativar site
sudo ln -sf /etc/nginx/sites-available/mottoSprint /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx

# Configurar firewall
echo "🔒 Configurando firewall..."
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 5000/tcp
sudo ufw --force enable

echo "✅ Configuração da VM concluída!"
echo "📋 Próximos passos:"
echo "1. Fazer upload da aplicação para /var/www/mottoSprint"
echo "2. Configurar as variáveis de ambiente"
echo "3. Executar dotnet run"
echo ""
echo "🔗 Informações importantes:"
echo "- Diretório da aplicação: /var/www/mottoSprint"
echo "- Nginx configurado como proxy na porta 80"
echo "- Aplicação deve rodar na porta 5000"
echo "- Logs do Nginx: /var/log/nginx/"