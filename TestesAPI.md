# Guia de Testes da API MottoSprint

**Aluno:** Richardy Borges Santana - RM: 557883  
**Professor:** Este guia foi criado para facilitar os testes da nossa API

## Como Acessar e Testar a API

### Passo 1: Executar a Aplicacao

1. Abra o terminal na pasta do projeto
2. Execute o comando:
   ```bash
   cd MottoSprint
   dotnet run
   ```
3. Aguarde a mensagem: "Now listening on: http://localhost:5003"
4. Abra o navegador e acesse: http://localhost:5003/swagger

### Passo 2: Interface do Swagger

Quando abrir o Swagger, voce vera:
- Lista de todos os endpoints organizados por categoria
- Botao "Try it out" em cada endpoint
- Exemplos de requisicao e resposta
- Documentacao detalhada de cada campo

## Testes Basicos - Passo a Passo

### Teste 1: Listar Vagas do Estacionamento

**Endpoint:** GET /api/parking/spots

1. Localize a secao "Parking" no Swagger
2. Clique em "GET /api/parking/spots"
3. Clique no botao "Try it out"
4. Clique em "Execute"
5. **Resultado esperado:** Lista de vagas com status (ocupada/livre)

**O que voce deve ver:**
```json
{
  "data": [
    {
      "id": 1,
      "spotNumber": "A001",
      "isOccupied": false,
      "createdAt": "2024-01-15T10:30:00Z",
      "links": [...]
    }
  ],
  "links": [...]
}
```

### Teste 2: Criar uma Nova Moto

**Endpoint:** POST /api/motos

1. Localize a secao "Motos" no Swagger
2. Clique em "POST /api/motos"
3. Clique no botao "Try it out"
4. No campo "Request body", cole este JSON:
   ```json
   {
     "placa": "ABC1234",
     "modelo": "Honda CB600F",
     "ano": 2023,
     "cor": "Azul",
     "status": "Ativa"
   }
   ```
5. Clique em "Execute"
6. **Resultado esperado:** Status 201 (Created) com dados da moto criada

### Teste 3: Buscar Moto por Placa

**Endpoint:** GET /api/motos/{placa}

1. Localize "GET /api/motos/{placa}"
2. Clique no botao "Try it out"
3. No campo "placa", digite: ABC1234
4. Clique em "Execute"
5. **Resultado esperado:** Dados completos da moto com links HATEOAS

### Teste 4: Entrada de Moto no Estacionamento

**Endpoint:** POST /api/motos/entrada

1. Localize "POST /api/motos/entrada"
2. Clique no botao "Try it out"
3. No campo "Request body", cole:
   ```json
   {
     "placa": "ABC1234",
     "linha": 1,
     "coluna": 2
   }
   ```
4. Clique em "Execute"
5. **Resultado esperado:** Confirmacao de entrada com detalhes da vaga

### Teste 5: Listar Notificacoes

**Endpoint:** GET /api/notification

1. Localize a secao "Notification"
2. Clique em "GET /api/notification"
3. Clique no botao "Try it out"
4. Clique em "Execute"
5. **Resultado esperado:** Lista de notificacoes do sistema

### Teste 6: Saida de Moto do Estacionamento

**Endpoint:** POST /api/motos/retirarVaga/{placa}

1. Localize "POST /api/motos/retirarVaga/{placa}"
2. Clique no botao "Try it out"
3. No campo "placa", digite: ABC1234
4. Clique em "Execute"
5. **Resultado esperado:** Confirmacao de saida

## Testes Avancados

### Teste 7: Paginacao

**Endpoint:** GET /api/notification?page=1&size=5

1. Va para "GET /api/notification"
2. Clique em "Try it out"
3. Preencha os parametros:
   - page: 1
   - size: 5
4. Clique em "Execute"
5. **Resultado esperado:** 5 notificacoes por pagina com links de navegacao

### Teste 8: Filtros

**Endpoint:** GET /api/notification?isRead=false

1. Va para "GET /api/notification"
2. Clique em "Try it out"
3. No parametro "isRead", selecione "false"
4. Clique em "Execute"
5. **Resultado esperado:** Apenas notificacoes nao lidas

## Entendendo as Respostas HATEOAS

Todas as respostas incluem links que mostram as acoes possiveis:

```json
{
  "data": {...},
  "links": [
    {
      "rel": "self",
      "href": "http://localhost:5003/api/motos/ABC1234",
      "method": "GET",
      "description": "Obter dados desta moto"
    },
    {
      "rel": "update",
      "href": "http://localhost:5003/api/motos/ABC1234",
      "method": "PUT",
      "description": "Atualizar esta moto"
    }
  ]
}
```

**Como usar os links:**
- "self": Link para o proprio recurso
- "update": Link para atualizar
- "delete": Link para deletar
- "collection": Link para a lista completa

## Codigos de Status HTTP

- **200 OK**: Operacao realizada com sucesso
- **201 Created**: Recurso criado com sucesso
- **400 Bad Request**: Dados invalidos na requisicao
- **404 Not Found**: Recurso nao encontrado
- **500 Internal Server Error**: Erro interno do servidor

## Dicas para o Professor

### 1. Ordem Recomendada de Testes
1. Primeiro: Listar vagas (GET /api/parking/spots)
2. Segundo: Criar moto (POST /api/motos)
3. Terceiro: Buscar moto criada (GET /api/motos/{placa})
4. Quarto: Fazer entrada no estacionamento
5. Quinto: Verificar notificacoes geradas
6. Sexto: Fazer saida do estacionamento

### 2. Validando Funcionalidades
- **HATEOAS**: Verifique se todas as respostas tem links
- **Paginacao**: Teste com parametros page e size
- **Filtros**: Use parametros como isRead, status, etc.
- **Validacao**: Teste com dados invalidos para ver erros

### 3. Pontos de Atencao
- A API usa SQLite em memoria, dados sao perdidos ao reiniciar
- Todas as respostas seguem o padrao HATEOAS
- Links sao dinamicos baseados no estado atual
- Paginacao e automatica em listas grandes

### 4. Exemplos de Dados para Teste

**Motos validas:**
```json
{
  "placa": "XYZ9876",
  "modelo": "Yamaha MT-07",
  "ano": 2022,
  "cor": "Preta",
  "status": "Ativa"
}
```

**Entrada no estacionamento:**
```json
{
  "placa": "XYZ9876",
  "linha": 2,
  "coluna": 3
}
```

## Troubleshooting

### Problema: API nao inicia
**Solucao:** Verifique se a porta 5003 esta livre

### Problema: Erro 404 ao testar
**Solucao:** Certifique-se de que a URL esta correta: http://localhost:5003

### Problema: Dados nao aparecem
**Solucao:** Crie alguns dados primeiro usando os endpoints POST

### Problema: Links HATEOAS nao funcionam
**Solucao:** Copie e cole os links diretamente no Swagger

## Conclusao

Este guia cobre os principais cenarios de teste da API MottoSprint. A API foi desenvolvida seguindo as melhores praticas de REST e HATEOAS, facilitando a navegacao e descoberta de funcionalidades.

Para duvidas ou problemas, verifique:
1. Se a aplicacao esta rodando na porta 5003
2. Se o Swagger esta acessivel em /swagger
3. Se os dados de teste estao sendo criados corretamente

**Boa sorte com os testes, Professor!**