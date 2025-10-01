# 🚀 Guia de Demonstração - MottoSprint API via Postman

## 📋 Visão Geral

Este guia explica detalhadamente como usar a coleção Postman **"MottoSprint API - Demonstração Completa"** para demonstrar todas as funcionalidades da API, desde a integração com Java até o sistema de notificações .NET.

## 🎯 Objetivo da Demonstração

Mostrar ao professor um **fluxo completo e funcional** que evidencia:

1. **✅ API .NET funcionando** com validações e HATEOAS
2. **✅ Integração com API Java** (comunicação bidirecional)
3. **✅ Sistema de notificações** em tempo real
4. **✅ Arquitetura Clean** bem estruturada
5. **✅ Tratamento de erros** robusto

---

## 🛠️ Pré-requisitos

### 1. Aplicação Rodando
```bash
# Certifique-se que a API .NET está rodando em:
http://localhost:5000

# Verifique se o Swagger está acessível:
http://localhost:5000/swagger/index.html
```

### 2. Importar Coleção no Postman
1. Abra o Postman
2. Clique em **"Import"**
3. Selecione o arquivo: `docs/MottoSprint_API_Demo_Complete.postman_collection.json`
4. A coleção será importada com todas as variáveis configuradas

---

## 📚 Estrutura da Demonstração

### 🚀 **1. SETUP - Verificações Iniciais**

#### 1.1 Health Check - API .NET
- **O que faz**: Verifica se a API .NET está rodando
- **URL**: `GET {{baseUrl}}/swagger/index.html`
- **Resultado esperado**: Página HTML do Swagger
- **Demonstra**: API .NET operacional

#### 1.2 Test Java API Connection
- **O que faz**: Testa conectividade com a API Java
- **URL**: `GET {{javaApiUrl}}/vagas`
- **Resultado esperado**: `405 Method Not Allowed` (confirma que está respondendo)
- **Demonstra**: API Java acessível e respondendo

---

### 🏍️ **2. CRUD DE MOTOS**

#### 2.1 Listar Todas as Motos (Inicial - Vazio)
- **O que faz**: Lista motos cadastradas (inicialmente vazio)
- **URL**: `GET {{baseUrl}}/api/motos`
- **Resultado esperado**: Array vazio `[]`
- **Demonstra**: Endpoint funcionando, banco limpo

#### 2.2 Criar Nova Moto (Demonstração) ⭐
- **O que faz**: Cria uma moto com validação completa
- **URL**: `POST {{baseUrl}}/api/motos`
- **Body**:
```json
{
  "placa": "ABC1234",
  "modelo": "Honda CB600F Hornet",
  "ano": 2023,
  "cor": "Azul Metálico"
}
```
- **Resultado esperado**: 
  - Status `201 Created`
  - Objeto moto com HATEOAS links
  - Links: `self`, `update`, `delete`, `mover-vaga`
- **Demonstra**: 
  - ✅ Validação funcionando
  - ✅ HATEOAS implementado
  - ✅ Status HTTP corretos

#### 2.3 Buscar Moto por Placa
- **O que faz**: Busca a moto criada por placa
- **URL**: `GET {{baseUrl}}/api/motos/{{motoPlaca}}`
- **Resultado esperado**: Dados completos da moto com links HATEOAS
- **Demonstra**: Busca por ID funcionando

#### 2.4 Listar Todas as Motos (Após Criação)
- **O que faz**: Lista motos novamente para mostrar que foi criada
- **Resultado esperado**: Array com a moto criada
- **Demonstra**: Persistência de dados

---

### 🔄 **3. INTEGRAÇÃO COM JAVA API** ⭐⭐⭐

#### 3.1 Mover Moto para Vaga (Comunicação .NET → Java) 🎯
- **O que faz**: **DEMONSTRAÇÃO PRINCIPAL** - Comunicação entre APIs
- **URL**: `POST {{baseUrl}}/api/motos/moverVaga`
- **Body**:
```json
{
  "placa": "ABC1234",
  "linha": "A",
  "coluna": 1
}
```
- **Fluxo interno**:
  1. 📨 API .NET recebe request
  2. 🔄 Comunica com API Java para alocar vaga
  3. 💾 Atualiza status da moto no banco .NET
  4. 📤 Retorna resposta com HATEOAS atualizado

- **Resultado esperado**:
  - Status `200 OK`
  - `idVaga` preenchido
  - Links HATEOAS atualizados (agora tem `retirar-vaga`)

- **Demonstra**: 
  - ✅ **Integração completa .NET ↔ Java**
  - ✅ **Comunicação bidirecional funcionando**
  - ✅ **HATEOAS dinâmico baseado no estado**

#### 3.2 Verificar Status da Moto (Após Mover)
- **O que faz**: Confirma que a moto foi movida
- **Resultado esperado**: Moto com `idVaga` preenchido e links diferentes
- **Demonstra**: Estado atualizado após integração

#### 3.3 Retirar Moto da Vaga
- **O que faz**: Remove moto da vaga (comunicação reversa)
- **URL**: `POST {{baseUrl}}/api/motos/retirarVaga/{{motoPlaca}}`
- **Demonstra**: Comunicação reversa com Java API

---

### 📢 **4. SISTEMA DE NOTIFICAÇÕES**

#### 4.1 Listar Notificações (Inicial)
- **O que faz**: Lista notificações existentes
- **Demonstra**: Sistema de notificações operacional

#### 4.2 Criar Notificação de Entrada ⭐
- **O que faz**: Cria notificação de entrada de moto
- **URL**: `POST {{baseUrl}}/api/notifications`
- **Body**:
```json
{
  "motoPlaca": "ABC1234",
  "tipoMovimentacao": "ENTRADA",
  "timestampEvento": "{{$isoTimestamp}}"
}
```
- **Resultado esperado**: 
  - Status `201 Created`
  - ID da notificação retornado
- **Demonstra**: 
  - ✅ Sistema de notificações funcionando
  - ✅ SignalR integrado (tempo real)
  - ✅ Timestamp automático

#### 4.3 Criar Notificação de Saída
- **O que faz**: Cria notificação de saída
- **Demonstra**: Diferentes tipos de movimentação

#### 4.4 Listar Notificações (Final)
- **O que faz**: Mostra histórico completo de notificações
- **Demonstra**: Persistência e rastreabilidade

---

### 🧪 **5. TESTES DE VALIDAÇÃO**

#### 5.1 Teste Validação - Moto sem Cor (Erro Esperado) ⭐
- **O que faz**: Tenta criar moto sem campo obrigatório
- **Body** (inválido):
```json
{
  "placa": "XYZ9999",
  "modelo": "Yamaha MT-07",
  "ano": 2023
  // "cor" está faltando!
}
```
- **Resultado esperado**: 
  - Status `400 Bad Request`
  - Mensagem de erro específica: `"A cor é obrigatória"`
- **Demonstra**: 
  - ✅ **Validação robusta funcionando**
  - ✅ **Mensagens de erro claras**
  - ✅ **Data Annotations implementadas**

#### 5.2 Teste Busca - Moto Inexistente (404)
- **O que faz**: Busca moto que não existe
- **URL**: `GET {{baseUrl}}/api/motos/INEXISTENTE999`
- **Resultado esperado**: Status `404 Not Found`
- **Demonstra**: Tratamento correto de recursos não encontrados

---

### 🧹 **6. LIMPEZA (OPCIONAL)**

#### 6.1 Deletar Moto de Teste
- **O que faz**: Remove a moto criada durante a demonstração
- **Demonstra**: Operação DELETE funcionando

---

## 🎯 Pontos-Chave para Destacar ao Professor

### 1. **🔗 Integração Completa** (Seção 3.1)
```
"Professor, aqui está a evidência da integração funcionando:
- A API .NET recebe o request
- Comunica com a API Java em tempo real
- Atualiza o estado da moto
- Retorna HATEOAS dinâmico baseado no novo estado"
```

### 2. **✅ Validação Robusta** (Seção 5.1)
```
"Veja como a validação funciona:
- Campos obrigatórios são validados
- Mensagens de erro específicas
- Status HTTP corretos"
```

### 3. **🌐 HATEOAS Dinâmico** (Seções 2.2 e 3.1)
```
"Observe como os links mudam baseado no estado:
- Antes: tem 'mover-vaga'
- Depois: tem 'retirar-vaga'
- Isso é HATEOAS verdadeiro!"
```

### 4. **📱 Sistema de Notificações** (Seção 4)
```
"Sistema completo de notificações:
- Criação em tempo real
- SignalR integrado
- Histórico persistente"
```

---

## 🚀 Roteiro de Apresentação Sugerido

### **Fase 1: Setup (2 min)**
1. Execute 1.1 e 1.2 para mostrar que tudo está funcionando
2. "Professor, ambas as APIs estão operacionais"

### **Fase 2: CRUD Básico (3 min)**
1. Execute 2.1 (vazio)
2. Execute 2.2 (criar moto) - **Destaque HATEOAS**
3. Execute 2.3 (buscar) - **Destaque dados completos**
4. Execute 2.4 (listar) - **Destaque persistência**

### **Fase 3: Integração (5 min) ⭐**
1. Execute 3.1 (mover vaga) - **PONTO PRINCIPAL**
2. **Explique o fluxo interno detalhadamente**
3. Execute 3.2 (verificar) - **Mostre mudança de estado**
4. "Aqui está a prova da integração funcionando!"

### **Fase 4: Notificações (2 min)**
1. Execute 4.2 (criar notificação)
2. Execute 4.4 (listar) - **Mostre histórico**

### **Fase 5: Validação (2 min)**
1. Execute 5.1 (erro esperado) - **Destaque validação**
2. "Veja como o sistema trata erros corretamente"

---

## 📊 Variáveis da Coleção

| Variável | Valor | Descrição |
|----------|-------|-----------|
| `baseUrl` | `http://localhost:5000` | URL da API .NET |
| `javaApiUrl` | `http://52.226.54.155:8080/api` | URL da API Java |
| `motoPlaca` | `ABC1234` | Placa para testes |
| `notificationId` | (auto) | ID da notificação criada |

---

## ✅ Checklist de Demonstração

- [ ] API .NET rodando em localhost:5000
- [ ] Coleção Postman importada
- [ ] Variáveis configuradas corretamente
- [ ] Executar requests na ordem sugerida
- [ ] Destacar pontos-chave durante execução
- [ ] Explicar fluxo de integração detalhadamente

---

## 🎯 Resultado Esperado

Ao final da demonstração, o professor terá visto:

1. ✅ **API .NET completamente funcional**
2. ✅ **Integração real com API Java**
3. ✅ **Sistema de notificações operacional**
4. ✅ **Validações robustas**
5. ✅ **HATEOAS implementado corretamente**
6. ✅ **Arquitetura Clean funcionando**

**Isso comprova que o projeto está 100% funcional e atende todos os requisitos!** 🚀