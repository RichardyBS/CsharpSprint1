# 🔐 Configuração de Variáveis de Ambiente

Este documento explica como configurar as variáveis de ambiente para o Sistema de Estacionamento.

## 📋 Configuração Inicial

### 1. Copiar o arquivo de template
```bash
cp .env.example .env
```

### 2. Editar as configurações
Abra o arquivo `.env` e substitua os valores pelos seus dados reais:

```bash
# Exemplo de configuração mínima
JWT_SECRET_KEY=SuaChaveSecretaMuitoSegura123!@#
POSTGRES_PASSWORD=suasenhapostgres
MONGODB_PASSWORD=suasenhamongo
SMTP_USERNAME=seu-email@gmail.com
SMTP_PASSWORD=sua-senha-app
```

## 🗄️ Bancos de Dados

### PostgreSQL (Analytics)
```env
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DATABASE=EstacionamentoAnalytics
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=SuaSenhaAqui
```

### MongoDB (Billing)
```env
MONGODB_HOST=localhost
MONGODB_PORT=27017
MONGODB_USERNAME=admin
MONGODB_PASSWORD=SuaSenhaAqui
MONGODB_DATABASE=billing_db
```

### Oracle (Legacy/Backup)
```env
ORACLE_HOST=oracle.fiap.com.br
ORACLE_PORT=1521
ORACLE_SERVICE=ORCL
ORACLE_USERNAME=SeuUsuarioAqui
ORACLE_PASSWORD=SuaSenhaAqui
```

## 🔧 Serviços de Infraestrutura

### Redis (Cache)
```env
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=
REDIS_DATABASE=0
```

### RabbitMQ (Message Broker)
```env
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
```

### SMTP (Email)
```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=seu-email@gmail.com
SMTP_PASSWORD=sua-senha-app
SMTP_ENABLE_SSL=true
```

## 🚀 Como Usar no Código

### 1. Instalar pacote DotNetEnv
```bash
dotnet add package DotNetEnv
```

### 2. Carregar variáveis no Program.cs
```csharp
using DotNetEnv;

// No início do Program.cs
Env.Load();

// Usar as variáveis
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
```

### 3. Exemplo de configuração no appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{POSTGRES_CONNECTION_STRING}#",
    "Redis": "#{REDIS_CONNECTION_STRING}#",
    "MongoDB": "#{MONGODB_CONNECTION_STRING}#"
  },
  "JwtSettings": {
    "SecretKey": "#{JWT_SECRET_KEY}#",
    "Issuer": "#{JWT_ISSUER}#",
    "Audience": "#{JWT_AUDIENCE}#",
    "ExpirationMinutes": "#{JWT_EXPIRATION_MINUTES}#"
  }
}
```

## 🔒 Segurança

### ⚠️ IMPORTANTE
- **NUNCA** commite o arquivo `.env` no Git
- Use senhas fortes para produção
- Rotacione as chaves secretas regularmente
- Use diferentes chaves para cada ambiente (dev, staging, prod)

### 🛡️ Boas Práticas
1. **Chaves JWT**: Use pelo menos 32 caracteres com símbolos especiais
2. **Senhas de BD**: Use senhas complexas com letras, números e símbolos
3. **SMTP**: Use senhas de aplicativo, não a senha principal
4. **Ambientes**: Mantenha configurações separadas para dev/prod

## 🐳 Docker

### docker-compose.yml
```yaml
version: '3.8'
services:
  api-gateway:
    environment:
      - JWT_SECRET_KEY=${JWT_SECRET_KEY}
      - POSTGRES_CONNECTION_STRING=${POSTGRES_CONNECTION_STRING}
    env_file:
      - .env
```

## 🧪 Testes

### Variáveis para Testes
```env
# Adicione ao .env para testes
ASPNETCORE_ENVIRONMENT=Testing
TEST_DATABASE_CONNECTION=InMemory
ENABLE_TEST_ENDPOINTS=true
```

## 📝 Checklist de Configuração

- [ ] Arquivo `.env` criado e configurado
- [ ] Senhas alteradas dos valores padrão
- [ ] Bancos de dados configurados
- [ ] Serviços de infraestrutura rodando
- [ ] Testes de conexão realizados
- [ ] Arquivo `.env` adicionado ao `.gitignore`

## 🆘 Troubleshooting

### Erro de Conexão com BD
1. Verifique se o serviço está rodando
2. Confirme host, porta e credenciais
3. Teste a conexão manualmente

### Erro de JWT
1. Verifique se a chave secreta está definida
2. Confirme se tem pelo menos 16 caracteres
3. Verifique se não há espaços extras

### Erro de SMTP
1. Use senha de aplicativo, não a senha principal
2. Verifique se 2FA está habilitado
3. Confirme as configurações do provedor

## 📞 Suporte

Para dúvidas sobre configuração:
1. Verifique este documento
2. Consulte os logs da aplicação
3. Teste as conexões individualmente