# Configuração Docker - MottoSprint

Este documento descreve como executar o sistema MottoSprint usando Docker.

## Pré-requisitos

- Docker Desktop instalado e em execução
- Docker Compose v2.0 ou superior

## Estrutura dos Serviços

O sistema utiliza os seguintes serviços:

### Aplicação Principal
- **mottosprint**: Aplicação principal integrada (porta 5000)

### Bancos de Dados e Infraestrutura
- **postgres**: Banco de dados PostgreSQL (porta 5432)
- **redis**: Cache e sessões (porta 6379)
- **rabbitmq**: Message broker (porta 5672, management: 15672)

## Como Executar

### 1. Configurar Variáveis de Ambiente

Copie o arquivo `.env` e ajuste as configurações conforme necessário:

```bash
cp .env .env.local
```

### 2. Executar com Docker Compose

```bash
# Construir e executar todos os serviços
docker-compose up --build

# Executar em background
docker-compose up -d --build

# Executar apenas a infraestrutura (bancos de dados)
docker-compose up postgres redis rabbitmq -d

# Executar apenas a aplicação
docker-compose up mottosprint --build
```

### 3. Verificar Status dos Serviços

```bash
# Ver logs de todos os serviços
docker-compose logs

# Ver logs de um serviço específico
docker-compose logs mottosprint

# Ver status dos containers
docker-compose ps
```

### 4. Parar os Serviços

```bash
# Parar todos os serviços
docker-compose down

# Parar e remover volumes (CUIDADO: remove dados)
docker-compose down -v
```

## Acessos

Após executar os serviços, você pode acessar:

- **MottoSprint**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Desenvolvimento

### Construir apenas a imagem da aplicação

```bash
docker build -t mottosprint:latest ./MottoSprint
```

### Executar apenas a aplicação localmente

```bash
# Executar infraestrutura
docker-compose up postgres redis rabbitmq -d

# Executar aplicação localmente
cd MottoSprint
dotnet run
```

## Troubleshooting

### Problemas Comuns

1. **Erro de conexão com banco de dados**
   - Verifique se os containers estão rodando: `docker-compose ps`
   - Verifique os logs: `docker-compose logs postgres`

2. **Porta já em uso**
   - Altere as portas no arquivo `.env`
   - Ou pare outros serviços que estejam usando as mesmas portas

3. **Problemas de build**
   - Limpe o cache do Docker: `docker system prune -a`
   - Reconstrua as imagens: `docker-compose build --no-cache`

### Comandos Úteis

```bash
# Limpar containers parados
docker container prune

# Limpar imagens não utilizadas
docker image prune

# Limpar volumes não utilizados
docker volume prune

# Ver uso de espaço
docker system df
```

## Estrutura de Arquivos

```
├── docker-compose.yml          # Configuração dos serviços
├── .env                        # Variáveis de ambiente
├── MottoSprint/
│   ├── Dockerfile             # Imagem da aplicação
│   └── .dockerignore          # Arquivos ignorados no build
└── DOCKER.md                  # Esta documentação
```

## Variáveis de Ambiente Importantes

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `MOTTOSPRINT_PORT` | Porta da aplicação | 5000 |
| `POSTGRES_PORT` | Porta do PostgreSQL | 5432 |
| `REDIS_PORT` | Porta do Redis | 6379 |
| `RABBITMQ_PORT` | Porta do RabbitMQ | 5672 |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | Development |

Para mais detalhes sobre as variáveis, consulte o arquivo `.env`.