# 🔐 Configurações de Segurança - MottoSprint

## ⚠️ IMPORTANTE - Antes de Usar em Produção

Este projeto foi desenvolvido para fins educacionais (Challenge FIAP 2025). Antes de usar em produção, configure adequadamente:

## 🔧 Configurações Necessárias

### 1. Variáveis de Ambiente (.env)

Copie o arquivo `.env.example` para `.env` e configure:

```bash
cp .env.example .env
```

**Configure as seguintes variáveis:**

- `JWT_SECRET_KEY`: Use uma chave forte de pelo menos 32 caracteres
- `JAVA_API_BASE_URL`: URL da sua API Java
- Outras configurações conforme necessário

### 2. Configurações de Produção

#### appsettings.json
- ✅ **JWT SecretKey**: Configurada via variável de ambiente
- ✅ **Connection Strings**: Usando SQLite local (seguro para desenvolvimento)
- ⚠️ **Java API URL**: Configure para sua API real

#### Banco de Dados
- ✅ **SQLite**: Usado para desenvolvimento (sem credenciais)
- ✅ **Oracle Scripts**: Não contêm senhas hardcoded

### 3. URLs e Endpoints

#### URLs Genéricas Configuradas:
- ✅ Java API: `http://your-java-api-url:8080/api`
- ✅ Produção: `https://your-production-url.com`
- ✅ Email: `contato@exemplo.com`

## 🛡️ Boas Práticas Implementadas

### ✅ **Segurança Implementada:**
- Arquivo `.env` no `.gitignore`
- Configurações sensíveis via variáveis de ambiente
- URLs genéricas nos arquivos de configuração
- Sem senhas hardcoded no código
- JWT com configuração externa

### ✅ **Arquivos Seguros para GitHub Público:**
- Todos os arquivos de código
- Documentação completa
- Scripts de banco (sem credenciais)
- Testes automatizados
- Configurações de exemplo

## 🚀 Para Desenvolvimento Local

1. **Copie o arquivo de exemplo:**
   ```bash
   cp .env.example .env
   ```

2. **Configure suas URLs:**
   - Edite `.env` com suas configurações
   - Configure a URL da API Java se necessário

3. **Execute o projeto:**
   ```bash
   cd MottoSprint
   dotnet run
   ```

## 🎯 Para Produção

1. **Configure variáveis de ambiente no servidor**
2. **Use HTTPS em produção**
3. **Configure banco de dados real (Oracle/PostgreSQL)**
4. **Use secrets manager para informações sensíveis**

## 📋 Checklist de Segurança

- ✅ Arquivo `.env` no `.gitignore`
- ✅ URLs genéricas nos arquivos de configuração
- ✅ Sem senhas hardcoded
- ✅ JWT configurado via variáveis
- ✅ Documentação de segurança criada
- ✅ Arquivo `.env.example` disponível

**Projeto seguro para GitHub público! 🎉**