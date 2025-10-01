# Solução para Mover Vaga com Linha e Coluna

## Problema Identificado

O endpoint `/api/motos/moverVaga` existente espera um `idVaga`, mas você estava enviando `linha` e `coluna`. 

### Estrutura Esperada pelo Endpoint Antigo:
```json
{
  "placa": "TST1234",
  "idVaga": 15
}
```

### Estrutura que Você Estava Enviando:
```json
{
  "placa": "TST1234", 
  "linha": "A",
  "coluna": "1"
}
```

## Solução Implementada

Criei um novo endpoint que aceita linha e coluna diretamente:

### Novo Endpoint: `/api/motos/moverVagaPorPosicao`

**Método:** POST  
**URL:** `http://localhost:5003/api/motos/moverVagaPorPosicao`

### Estrutura do JSON para o Novo Endpoint:
```json
{
  "placa": "TST1234",
  "linha": "A", 
  "coluna": "1"
}
```

### Exemplo de Uso no Postman:

1. **Método:** POST
2. **URL:** `{{baseUrl}}/api/motos/moverVagaPorPosicao`
3. **Headers:**
   - Content-Type: application/json
4. **Body (raw JSON):**
```json
{
  "placa": "TST1234",
  "linha": "A",
  "coluna": "1"
}
```

### Resposta Esperada:
```json
{
  "placa": "TST1234",
  "modelo": "Honda CBR600",
  "ano": 2023,
  "cor": "Azul Metálico",
  "idVaga": 1,
  "status": "NORMAL",
  "linha": "A",
  "coluna": "1",
  "links": [
    {
      "rel": "self",
      "href": "/api/motos/TST1234",
      "method": "GET"
    },
    {
      "rel": "update", 
      "href": "/api/motos/TST1234",
      "method": "PUT"
    },
    {
      "rel": "delete",
      "href": "/api/motos/TST1234", 
      "method": "DELETE"
    },
    {
      "rel": "retirar-vaga",
      "href": "/api/motos/TST1234/retirarVaga",
      "method": "POST"
    },
    {
      "rel": "all-motos",
      "href": "/api/motos/all",
      "method": "GET"
    }
  ]
}
```

## Como a Conversão Funciona

O novo endpoint converte automaticamente linha e coluna para um ID de vaga usando a fórmula:
- A1 = ID 1
- A2 = ID 2  
- B1 = ID 11
- B2 = ID 12
- E assim por diante...

## Resumo

Agora você tem duas opções para mover uma moto:

1. **Endpoint Original:** `/api/motos/moverVaga` - usa `idVaga`
2. **Novo Endpoint:** `/api/motos/moverVagaPorPosicao` - usa `linha` e `coluna`

Use o novo endpoint com os dados que você já estava enviando e funcionará perfeitamente!