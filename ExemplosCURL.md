# Exemplos de Testes com cURL

**Aluno:** Richardy Borges Santana - RM: 557883  
**Para:** Testes alternativos da API MottoSprint

## Como Usar Este Guia

Se preferir testar via linha de comando ao invés do Swagger, use estes exemplos de cURL.

**Pré-requisito:** Aplicação rodando em http://localhost:5003

## Testes Básicos

### 1. Listar Vagas do Estacionamento

```bash
curl -X GET "http://localhost:5003/api/parking/spots" \
  -H "Content-Type: application/json"
```

**Resultado esperado:** Lista de vagas com status ocupada/livre

### 2. Criar Nova Moto

```bash
curl -X POST "http://localhost:5003/api/motos" \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "ABC1234",
    "modelo": "Honda CB600F",
    "ano": 2023,
    "cor": "Azul",
    "status": "Ativa"
  }'
```

**Resultado esperado:** Status 201 com dados da moto criada

### 3. Buscar Moto por Placa

```bash
curl -X GET "http://localhost:5003/api/motos/ABC1234" \
  -H "Content-Type: application/json"
```

### 4. Entrada no Estacionamento

```bash
curl -X POST "http://localhost:5003/api/motos/entrada" \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "ABC1234",
    "linha": 1,
    "coluna": 2
  }'
```

### 5. Listar Notificações

```bash
curl -X GET "http://localhost:5003/api/notification" \
  -H "Content-Type: application/json"
```

### 6. Saída do Estacionamento

```bash
curl -X POST "http://localhost:5003/api/motos/retirarVaga/ABC1234" \
  -H "Content-Type: application/json"
```

## Testes com Paginação

### Listar Notificações com Paginação

```bash
curl -X GET "http://localhost:5003/api/notification?page=1&size=5" \
  -H "Content-Type: application/json"
```

### Listar Vagas com Paginação

```bash
curl -X GET "http://localhost:5003/api/parking/spots?page=1&pageSize=3" \
  -H "Content-Type: application/json"
```

## Testes com Filtros

### Notificações Não Lidas

```bash
curl -X GET "http://localhost:5003/api/notification?isRead=false" \
  -H "Content-Type: application/json"
```

### Notificações Lidas

```bash
curl -X GET "http://localhost:5003/api/notification?isRead=true" \
  -H "Content-Type: application/json"
```

## Script Completo de Teste

Salve este script como `teste_completo.sh` e execute:

```bash
#!/bin/bash

echo "=== TESTE COMPLETO DA API MOTTOSPRINT ==="
echo "Aluno: Richardy Borges - RM: 557883"
echo ""

echo "1. Listando vagas disponíveis..."
curl -s -X GET "http://localhost:5003/api/parking/spots" | head -20
echo -e "\n"

echo "2. Criando nova moto..."
curl -s -X POST "http://localhost:5003/api/motos" \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "TEST123",
    "modelo": "Honda CB600F",
    "ano": 2023,
    "cor": "Azul",
    "status": "Ativa"
  }' | head -10
echo -e "\n"

echo "3. Buscando moto criada..."
curl -s -X GET "http://localhost:5003/api/motos/TEST123" | head -10
echo -e "\n"

echo "4. Fazendo entrada no estacionamento..."
curl -s -X POST "http://localhost:5003/api/motos/entrada" \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "TEST123",
    "linha": 1,
    "coluna": 1
  }' | head -10
echo -e "\n"

echo "5. Verificando notificações..."
curl -s -X GET "http://localhost:5003/api/notification" | head -15
echo -e "\n"

echo "6. Fazendo saída do estacionamento..."
curl -s -X POST "http://localhost:5003/api/motos/retirarVaga/TEST123" | head -10
echo -e "\n"

echo "=== TESTE CONCLUÍDO ==="
```

## Validação de Respostas

### Códigos de Status Esperados

- **200 OK**: GET requests bem-sucedidos
- **201 Created**: POST de criação bem-sucedido
- **400 Bad Request**: Dados inválidos
- **404 Not Found**: Recurso não encontrado

### Verificando Links HATEOAS

Todas as respostas devem incluir uma seção `links`:

```json
{
  "data": {...},
  "links": [
    {
      "rel": "self",
      "href": "http://localhost:5003/api/motos/ABC1234",
      "method": "GET"
    }
  ]
}
```

## Dicas para Debugging

### Ver Headers da Resposta

```bash
curl -I "http://localhost:5003/api/parking/spots"
```

### Ver Resposta Completa com Headers

```bash
curl -v "http://localhost:5003/api/parking/spots"
```

### Salvar Resposta em Arquivo

```bash
curl "http://localhost:5003/api/notification" > notificacoes.json
```

## Troubleshooting

### Erro: Connection Refused
**Solução:** Verifique se a API está rodando na porta 5003

### Erro: 404 Not Found
**Solução:** Verifique a URL e certifique-se de que o endpoint existe

### Erro: 400 Bad Request
**Solução:** Verifique o formato do JSON e os dados enviados

### Resposta Vazia
**Solução:** Primeiro crie alguns dados usando os endpoints POST

## Conclusão

Estes exemplos cobrem todos os cenários principais da API MottoSprint. Use o Swagger UI para testes interativos ou estes comandos cURL para testes automatizados.

**Recomendação:** Comece sempre com o Swagger UI para entender a API, depois use cURL para automação.