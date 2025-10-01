# MottoSprint - Sistema de Gerenciamento de Estacionamento de Motocicletas

## 📋 Sobre o Projeto

O **MottoSprint** é uma API REST desenvolvida em .NET 8 para gerenciamento de estacionamento de motocicletas, criada como parte do Challenge FIAP 2025. O sistema oferece funcionalidades completas para controle de entrada e saída de motos, notificações em tempo real e compatibilidade com APIs Java existentes.

### 🎯 Funcionalidades Principais

- **Gerenciamento de Motocicletas**: CRUD completo com validações
- **Sistema de Estacionamento**: Controle de vagas com posicionamento (linha/coluna)
- **Notificações em Tempo Real**: Sistema de notificações com SignalR
- **Compatibilidade Java**: Endpoints compatíveis com APIs Java existentes
- **HATEOAS**: Implementação completa de links relacionados
- **Documentação Swagger**: Interface interativa para testes
- **Autenticação JWT**: Sistema de segurança robusto

## 👥 Integrantes do Projeto

| Nome | RM | Responsabilidade |
|------|----|--------------------|
| **Ruan Lima Silva** | RM558775 | Java e DevOps |
| **Richardy Borges Santana** | RM557883 | .NET e Banco de Dados |
| **Marcos Vinicius Pereira de Oliveira** | RM557252 | Mobile e IoT |

## 🛠️ Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **Entity Framework Core** - ORM para acesso a dados
- **SQLite** - Banco de dados (desenvolvimento)
- **SignalR** - Comunicação em tempo real
- **Swagger/OpenAPI** - Documentação da API
- **JWT** - Autenticação e autorização
- **Docker** - Containerização
- **xUnit** - Testes unitários

## 📋 Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/) (opcional)

## 🚀 Como Executar o Projeto

### Opção 1: Execução Local (Recomendada)

1. **Clone o repositório:**
   ```bash
   git clone <url-do-repositorio>
   cd CsharpSprint1
   ```

2. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```

3. **Execute as migrações do banco de dados:**
   ```bash
   cd MottoSprint
   dotnet ef database update
   ```

4. **Execute a aplicação:**
   ```bash
   dotnet run
   ```

5. **Acesse a aplicação:**
   - **API**: http://localhost:5003
   - **Swagger UI**: http://localhost:5003/swagger
   - **HTTPS**: https://localhost:7000

### Opção 2: Execução com Docker

1. **Build e execute com Docker Compose:**
   ```bash
   docker-compose up --build
   ```

2. **Acesse a aplicação:**
   - **API**: http://localhost:5003
   - **Swagger UI**: http://localhost:5003/swagger

## 📚 Estrutura do Projeto

```
MottoSprint/
├── Controllers/           # Controladores da API
├── Models/               # Modelos de dados
├── DTOs/                 # Data Transfer Objects
├── Services/             # Lógica de negócio
├── Data/                 # Contexto do banco de dados
├── Configuration/        # Configurações da aplicação
├── Extensions/           # Métodos de extensão
├── Hubs/                 # SignalR Hubs
├── Migrations/           # Migrações do banco
└── Swagger/              # Configurações do Swagger

Database/                 # Scripts SQL
├── 00_SETUP_USER.sql
├── 01_CREATE_TABLES.sql
├── 02_INSERT_SAMPLE_DATA.sql
├── 03_PROCEDURES_FUNCTIONS.sql
└── 04_VIEWS.sql

MottoSprint.Tests/        # Testes unitários
docs/                     # Documentação adicional
```

## 🔧 Configuração

### Banco de Dados

O projeto utiliza SQLite por padrão para desenvolvimento. Para usar outro banco:

1. Modifique a string de conexão em `appsettings.json`
2. Execute as migrações: `dotnet ef database update`

### Variáveis de Ambiente

Configure as seguintes variáveis se necessário:

```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="Data Source=mottosprint.db"
```

## 📖 Documentação da API

### Guia de Testes Completo

Para facilitar os testes da API, criamos guias detalhados:

- **[TestesAPI.md](./TestesAPI.md)** - Guia completo com passo a passo para testar via Swagger UI
- **[ExemplosCURL.md](./ExemplosCURL.md)** - Exemplos de testes via linha de comando (cURL)

### Endpoints Principais

#### Motocicletas (Java Compatible)
- `GET /api/motos` - Listar motocicletas
- `POST /api/motos` - Criar motocicleta
- `GET /api/motos/{placa}` - Buscar por placa
- `PUT /api/motos/{placa}` - Atualizar motocicleta
- `DELETE /api/motos/{placa}` - Remover motocicleta
- `POST /api/motos/entrada` - Entrada no estacionamento
- `POST /api/motos/retirarVaga/{placa}` - Saída do estacionamento

#### Notificações
- `GET /api/notification` - Listar notificações
- `POST /api/notification` - Criar notificação
- `PUT /api/notification/{id}/read` - Marcar como lida

#### Estacionamento
- `GET /api/parking/spots` - Listar vagas
- `GET /api/parking/spots/available` - Vagas disponíveis

### Exemplo de Uso

```bash
# Criar uma motocicleta
curl -X POST "http://localhost:5003/api/motos" \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "ABC1234",
    "modelo": "Honda CB600F",
    "ano": 2023,
    "cor": "Azul",
    "status": "Ativa"
  }'

# Entrada no estacionamento
curl -X POST "http://localhost:5003/api/motos/entrada" \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "ABC1234",
    "linha": 1,
    "coluna": 2
  }'
```

## 🧪 Executando os Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test --filter "ClassName=JavaCompatibleControllerTests"
```

## 🐳 Docker

### Build da Imagem
```bash
docker build -t mottosprint .
```

### Executar Container
```bash
docker run -p 5003:8080 mottosprint
```

## 📊 Monitoramento e Logs

A aplicação inclui:
- Logs estruturados com Serilog
- Health checks em `/health`
- Métricas de performance
- Swagger UI para documentação interativa

## 🔒 Segurança

- Autenticação JWT implementada
- Validação de entrada em todos os endpoints
- CORS configurado adequadamente
- Headers de segurança aplicados

## 🚀 Deploy

### Ambiente de Produção

1. Configure as variáveis de ambiente de produção
2. Use um banco de dados robusto (SQL Server, PostgreSQL)
3. Configure HTTPS com certificados válidos
4. Implemente monitoramento e logs centralizados

## 📞 Suporte

Para dúvidas ou problemas:

1. Verifique a documentação no Swagger UI
2. Consulte os logs da aplicação
3. Execute os testes para validar o ambiente
4. Entre em contato com a equipe de desenvolvimento

## 📄 Licença

Este projeto foi desenvolvido para fins acadêmicos como parte do Challenge FIAP 2025.

---

**Desenvolvido com ❤️ pela equipe MottoSprint**

