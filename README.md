# Sistema de Estacionamento - Microserviços

Sistema completo de gerenciamento de estacionamento implementado com arquitetura de microserviços usando .NET 8.

## Arquitetura

O sistema é composto por:

- **API Gateway**: Ponto de entrada único para todas as requisições
- **Analytics Service**: Análise de dados e relatórios (PostgreSQL)
- **Billing Service**: Faturamento e pagamentos (MongoDB)
- **Notification Service**: Notificações em tempo real (Redis + SignalR)
- **Event Bus**: Comunicação assíncrona entre serviços (RabbitMQ)

## Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **PostgreSQL**: Banco de dados para Analytics
- **MongoDB**: Banco de dados para Billing
- **Redis**: Cache e armazenamento de notificações
- **RabbitMQ**: Message broker para eventos
- **SignalR**: Comunicação em tempo real
- **Docker**: Containerização
- **JWT**: Autenticação e autorização

## Como Executar

### Pré-requisitos

- Docker e Docker Compose
- .NET 8 SDK (para desenvolvimento)
- PowerShell (para scripts de automação)

### 🚀 Setup Rápido (Recomendado)

1. Clone o repositório
2. Execute o script de setup:

```powershell
.\setup-environment.ps1
```

Este script irá:
- Criar o arquivo `.env` a partir do template
- Verificar dependências (Docker, .NET)
- Restaurar pacotes NuGet
- Compilar o projeto
- Subir os containers Docker
- Mostrar status dos serviços

### 🔧 Setup Manual

1. **Configure as variáveis de ambiente:**
```bash
cp .env.example .env
# Edite o arquivo .env com suas configurações
```

2. **Restaure as dependências:**
```bash
dotnet restore EstacionamentoMicroservices.sln
```

3. **Compile o projeto:**
```bash
dotnet build EstacionamentoMicroservices.sln
```

4. **Execute com Docker:**
```bash
docker-compose up -d
```

### 🔐 Configuração de Segurança

O projeto utiliza variáveis de ambiente para configurações sensíveis. Consulte:
- **`.env.example`** - Template de configuração
- **`ENVIRONMENT_SETUP.md`** - Guia detalhado de configuração

**IMPORTANTE**: Nunca commite o arquivo `.env` no Git!

### Serviços Disponíveis

- **API Gateway**: http://localhost:5000
- **Analytics Service**: http://localhost:5001
- **Billing Service**: http://localhost:5002
- **Notification Service**: http://localhost:5003
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Bancos de Dados

- **PostgreSQL**: localhost:5432 (postgres/postgres123)
- **MongoDB**: localhost:27017 (admin/admin123)
- **Redis**: localhost:6379

## Funcionalidades

### Analytics Service
- Registro de eventos de ocupação/liberação de vagas
- Relatórios de uso por período
- Estatísticas de ocupação
- Análise de receita

### Billing Service
- Geração automática de faturas
- Processamento de pagamentos
- Histórico de transações
- Relatórios financeiros

### Notification Service
- Notificações em tempo real via SignalR
- Notificações por email
- Cache de notificações no Redis
- Configurações personalizadas

## Eventos do Sistema

- `VagaOcupadaEvent`: Disparado quando uma vaga é ocupada
- `VagaLiberadaEvent`: Disparado quando uma vaga é liberada
- `PagamentoProcessadoEvent`: Disparado quando um pagamento é processado

## Desenvolvimento

### Estrutura do Projeto

```
src/
├── ApiGateway/              # Gateway de entrada
├── Services/
│   ├── Analytics.Service/   # Serviço de análise
│   ├── Billing.Service/     # Serviço de faturamento
│   └── Notification.Service/ # Serviço de notificações
└── Shared/
    ├── Shared.Contracts/    # Contratos compartilhados
    └── Shared.EventBus/     # Event Bus compartilhado
```

### Executando em Desenvolvimento

1. Inicie a infraestrutura:
```bash
docker-compose up postgres mongodb redis rabbitmq -d
```

2. Execute cada serviço individualmente:
```bash
cd src/Services/Analytics.Service
dotnet run

cd src/Services/Billing.Service
dotnet run

cd src/Services/Notification.Service
dotnet run

cd src/ApiGateway
dotnet run
```

## Monitoramento

- Health checks disponíveis em `/health` para cada serviço
- Logs estruturados para observabilidade
- Métricas de performance integradas

## Segurança

- Autenticação JWT em todos os serviços
- Validação de tokens
- CORS configurado
- Comunicação segura entre serviços

## Funcionalidades

### Gestão de Clientes
- ✅ CRUD completo de clientes
- ✅ Validação de CPF e email únicos
- ✅ Paginação e filtros de busca
- ✅ Soft delete para preservar histórico

### Gestão de Motocicletas
- ✅ CRUD completo de motos
- ✅ Relacionamento com clientes
- ✅ Validação de placa única
- ✅ Filtros por marca, modelo e ano

### Gestão de Vagas
- ✅ CRUD completo de vagas
- ✅ Sistema de ocupação e liberação
- ✅ Cálculo automático de valores
- ✅ Controle de tempo de estacionamento
- ✅ Relatórios de ocupação

### Recursos Avançados
- ✅ **Paginação** em todas as listagens
- ✅ **HATEOAS** (Hypermedia as the Engine of Application State)
- ✅ **Swagger/OpenAPI** com documentação completa
- ✅ **Logging** estruturado
- ✅ **Validações** robustas com Data Annotations
- ✅ **Status codes HTTP** apropriados
- ✅ **CORS** configurado
- ✅ **Entity Framework Core** com migrations

## Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - Para criação da API REST
- **Entity Framework Core** - ORM para acesso a dados
- **SQL Server** - Banco de dados
- **Swagger/OpenAPI** - Documentação da API
- **FluentValidation** - Validações avançadas
- **Serilog** - Logging estruturado

## 📋 Pré-requisitos

- .NET 8 SDK
- SQL Server (LocalDB ou instância completa)
- Visual Studio 2022 ou VS Code

## 🚀 Como Executar

### 1. Clone o repositório
```bash
git clone <url-do-repositorio>
cd MotoApiAdvanced
```

### 2. Configure a string de conexão
Edite o arquivo ppsettings.json e ajuste a string de conexão conforme seu ambiente:

```json
{
  \"ConnectionStrings\": {
    \"DefaultConnection\": \"Server=(localdb)\\mssqllocaldb;Database=MotoApiAdvancedDb;Trusted_Connection=true;MultipleActiveResultSets=true\"
  }
}
`

### 3. Execute as migrations
```bash
dotnet ef database update
```

### 4. Execute a aplicação
```bash
dotnet run
```

### 5. Acesse a documentação
Abra seu navegador e acesse: http://localhost:5297/swagger

## Documentação da API

### Endpoints Principais

#### Clientes
- GET /api/clientes - Lista clientes com paginação
- GET /api/clientes/{id} - Obtém cliente específico
- POST /api/clientes - Cria novo cliente
- PUT /api/clientes/{id} - Atualiza cliente
- DELETE /api/clientes/{id} - Remove cliente (soft delete)

#### Motos
- GET /api/motos - Lista motos com paginação
- GET /api/motos/{id} - Obtém moto específica
- POST /api/motos - Cadastra nova moto
- PUT /api/motos/{id} - Atualiza moto
- DELETE /api/motos/{id} - Remove moto (soft delete)

#### Vagas
- GET /api/vagas - Lista vagas com paginação
- GET /api/vagas/{id} - Obtém vaga específica
- POST /api/vagas - Cria nova vaga
- PUT /api/vagas/{id} - Atualiza vaga
- DELETE /api/vagas/{id} - Remove vaga (soft delete)
- POST /api/vagas/{id}/ocupar - Ocupa vaga com uma moto
- POST /api/vagas/{id}/liberar - Libera vaga e calcula valor

### Parâmetros de Paginação

Todos os endpoints de listagem suportam os seguintes parâmetros:

- pageNumber (int): Número da página (padrão: 1)
- pageSize (int): Itens por página (padrão: 10, máximo: 100)
- search (string): Termo de busca
- sortBy (string): Campo para ordenação
- sortDescending (bool): Ordenação decrescente

## Arquitetura

### Estrutura do Projeto

`
MotoApiAdvanced/
├── Controllers/          # Controllers da API
│   ├── ClientesController.cs
│   ├── MotosController.cs
│   └── VagasController.cs
├── Data/                # Contexto do Entity Framework
│   └── AppDbContext.cs
├── DTOs/                # Data Transfer Objects
│   ├── ClienteDto.cs
│   ├── MotoDto.cs
│   ├── VagaDto.cs
│   └── CommonDto.cs
├── Models/              # Entidades do domínio
│   ├── Cliente.cs
│   ├── Moto.cs
│   └── Vaga.cs
├── Migrations/          # Migrations do EF Core
├── Program.cs           # Configuração da aplicação
└── appsettings.json     # Configurações
`

### Padrões Utilizados

- **Repository Pattern** (através do Entity Framework)
- **DTO Pattern** para transferência de dados
- **HATEOAS** para navegação hipermídia
- **Soft Delete** para preservar dados históricos
- **Paginação** para performance em grandes volumes

## 🔧 Configurações

### Logging
O projeto utiliza logging estruturado com diferentes níveis:
- **Information**: Operações normais
- **Warning**: Situações de atenção
- **Error**: Erros de aplicação

### CORS
CORS está configurado para permitir qualquer origem em desenvolvimento. Para produção, configure adequadamente no Program.cs.

### Swagger
A documentação Swagger inclui:
- Descrições detalhadas de todos os endpoints
- Exemplos de request/response
- Códigos de status HTTP
- Modelos de dados

## Banco de Dados

### Modelo de Dados

`
Cliente (1) -----> (*) Moto (*) -----> (0..1) Vaga
`

#### Cliente
- Id, Nome, CPF, Email, Telefone, DataNascimento, DataCadastro, Ativo

#### Moto
- Id, Placa, Marca, Modelo, Ano, Cor, Cilindrada, ClienteId, DataCadastro, Ativa

#### Vaga
- Id, Numero, Setor, Ocupada, ValorPorHora, DataEntrada, DataSaida, ValorTotal, MotoId, DataCriacao, Ativa

### Dados de Exemplo

O projeto inclui dados de exemplo que são inseridos automaticamente na primeira execução:

- **2 Clientes** com informações completas
- **2 Motos** associadas aos clientes
- **5 Vagas** em diferentes setores

## 🛠️ Scripts de Manutenção

### Setup do Ambiente
```powershell
.\setup-environment.ps1
```
- Configura o ambiente completo
- Verifica dependências
- Compila e executa o projeto

### Limpeza do Ambiente
```powershell
.\cleanup-environment.ps1
```
- Para e remove containers
- Limpa volumes e imagens
- Remove arquivos de build

### Comandos Docker Úteis

```bash
# Ver logs dos serviços
docker-compose logs -f

# Reiniciar um serviço específico
docker-compose restart notification-service

# Parar todos os serviços
docker-compose down

# Rebuild e restart
docker-compose up -d --build
```

## 📁 Arquivos de Configuração

- **`.env`** - Variáveis de ambiente (não commitado)
- **`.env.example`** - Template de configuração
- **`docker-compose.yml`** - Configuração dos containers
- **`ENVIRONMENT_SETUP.md`** - Guia detalhado de configuração

## Deploy

### Preparação para Produção

1. **Configure as variáveis de ambiente** para o ambiente de produção
2. **Ajuste o CORS** para permitir apenas origens autorizadas
3. **Configure logs** para um provedor adequado (Application Insights, etc.)
4. **Desabilite o Swagger** em produção (opcional)
5. **Use chaves JWT seguras** e diferentes para cada ambiente

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (git checkout -b feature/AmazingFeature)
3. Commit suas mudanças (git commit -m 'Add some AmazingFeature')
4. Push para a branch (git push origin feature/AmazingFeature)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.

## Contato

Para dúvidas ou sugestões, entre em contato através dos issues do GitHub.

