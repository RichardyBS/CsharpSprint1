# 📋 Comandos Postman e Scripts de Banco de Dados

## 🎯 Visão Geral

Este documento contém todos os comandos Postman para testar o **MotoNotificationController** e os scripts SQL para criar as tabelas necessárias do sistema de notificação de motos.

---

## 🚀 Comandos Postman

### 📁 Arquivo de Coleção
**Localização:** `docs/Postman_Commands_MotoNotificationController.json`

### 🔧 Configuração Inicial

1. **Importe a coleção** no Postman usando o arquivo JSON
2. **Configure as variáveis de ambiente:**
   - `baseUrl`: `http://localhost:5003`
   - `authToken`: `Bearer SEU_JWT_TOKEN_AQUI`

### 📋 Endpoints Disponíveis

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| `GET` | `/api/motos/notificacoes/health` | Verificação de saúde do serviço | ❌ Não |
| `POST` | `/api/motos/notificacoes/entrada` | Processar entrada de moto | ✅ Sim |
| `POST` | `/api/motos/notificacoes/saida` | Processar saída de moto | ✅ Sim |
| `GET` | `/api/motos/notificacoes/cliente/{clienteId}` | Buscar notificações do cliente | ✅ Sim |
| `GET` | `/api/motos/notificacoes/cliente/{clienteId}/nao-lidas` | Buscar notificações não lidas | ✅ Sim |
| `PUT` | `/api/motos/notificacoes/marcar-lida` | Marcar notificação como lida | ✅ Sim |
| `GET` | `/api/motos/notificacoes/logs` | Buscar logs de movimentação | ✅ Sim |
| `GET` | `/api/motos/notificacoes/estatisticas` | Estatísticas do estacionamento | ✅ Sim |
| `GET` | `/api/motos/notificacoes/moto/{placa}` | Informações da moto | ✅ Sim |
| `GET` | `/api/motos/notificacoes/vaga/{idVaga}` | Informações da vaga | ✅ Sim |
| `GET` | `/api/motos/notificacoes/vagas-livres/{linha}` | Vagas livres por linha | ✅ Sim |

### 🔍 Exemplos de Uso

#### 1. Health Check
```http
GET http://localhost:5003/api/motos/notificacoes/health
```

#### 2. Processar Entrada de Moto
```http
POST http://localhost:5003/api/motos/notificacoes/entrada
Authorization: Bearer SEU_JWT_TOKEN
Content-Type: application/json

{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "clienteId": "123e4567-e89b-12d3-a456-426614174001",
  "placa": "ABC1234",
  "modelo": "Honda CB 600F",
  "ano": 2023,
  "cor": "Azul",
  "status": "Ativo",
  "idVaga": 15,
  "dataEntrada": "2025-01-27T10:30:00Z",
  "dataHora": "2025-01-27T10:30:00Z",
  "processado": false
}
```

#### 3. Buscar Notificações do Cliente
```http
GET http://localhost:5003/api/motos/notificacoes/cliente/123e4567-e89b-12d3-a456-426614174001
Authorization: Bearer SEU_JWT_TOKEN
```

---

## 🗄️ Scripts de Banco de Dados

### 📁 Arquivo SQL
**Localização:** `docs/Database_Scripts_Notification_System.sql`

### 🏗️ Estrutura do Banco

#### 📊 Tabelas Principais

1. **TB_NOTIFICACAO_MOTO**
   - Armazena todas as notificações enviadas aos clientes
   - Campos: ID, CLIENTE_ID, TITULO, MENSAGEM, TIPO, DATA_CRIACAO, LIDA, DADOS_MOTO, DADOS_VAGA

2. **TB_FILA_ENTRADA**
   - Fila de processamento para entrada de motos
   - Campos: ID, CLIENTE_ID, PLACA, MODELO, ANO, COR, STATUS, ID_VAGA, DATA_ENTRADA, PROCESSADO

3. **TB_FILA_SAIDA**
   - Fila de processamento para saída de motos
   - Campos: ID, CLIENTE_ID, PLACA, ID_VAGA, DATA_SAIDA, VALOR_COBRADO, PROCESSADO

4. **TB_LOG_MOVIMENTACAO**
   - Logs de todas as movimentações
   - Campos: ID, PLACA, TIPO_MOVIMENTACAO, ID_VAGA, LOCALIZACAO_VAGA, DATA_HORA, CLIENTE_ID

5. **TB_CONFIGURACAO_NOTIFICACAO**
   - Configurações de notificação por cliente
   - Campos: ID, CLIENTE_ID, NOTIFICAR_ENTRADA, NOTIFICAR_SAIDA, EMAIL_ATIVO, SMS_ATIVO, etc.

#### 🔗 Tabelas Complementares

6. **TB_CLIENTE**
   - Informações dos clientes
   - Campos: ID, NOME, EMAIL, CPF, TELEFONE, DATA_CADASTRO, ATIVO

7. **TB_MOTO**
   - Cadastro de motos
   - Campos: ID, CLIENTE_ID, PLACA, MODELO, MARCA, ANO, COR, CILINDRADA

8. **TB_VAGA**
   - Informações das vagas do estacionamento
   - Campos: ID, LINHA, COLUNA, OCUPADA, PLACA_MOTO, DATA_OCUPACAO

9. **TB_ESTATISTICAS_ESTACIONAMENTO**
   - Estatísticas diárias do estacionamento
   - Campos: ID, DATA_REFERENCIA, TOTAL_VAGAS, VAGAS_OCUPADAS, RECEITA_TOTAL, etc.

### 🚀 Como Executar os Scripts

1. **Conecte-se ao Oracle Database**
2. **Execute o script completo** `Database_Scripts_Notification_System.sql`
3. **Verifique a criação** das tabelas, índices e triggers

### 📈 Recursos Incluídos

- ✅ **Índices otimizados** para performance
- ✅ **Foreign Keys** para integridade referencial
- ✅ **Triggers** para auditoria e automação
- ✅ **Views** para consultas complexas
- ✅ **Dados iniciais** para teste
- ✅ **Constraints** para validação de dados

---

## 🔐 Autenticação JWT

### 🎫 Como Obter o Token

Para testar os endpoints que requerem autenticação, você precisa:

1. **Fazer login** no sistema através do endpoint de autenticação
2. **Copiar o token JWT** retornado
3. **Configurar no Postman** na variável `authToken` como `Bearer SEU_TOKEN`

### 📝 Exemplo de Header de Autenticação
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🧪 Testes Recomendados

### 🔄 Fluxo Completo de Teste

1. **Health Check** - Verificar se o serviço está funcionando
2. **Processar Entrada** - Simular entrada de uma moto
3. **Buscar Notificações** - Verificar se a notificação foi criada
4. **Processar Saída** - Simular saída da moto
5. **Verificar Logs** - Consultar logs de movimentação
6. **Estatísticas** - Verificar estatísticas atualizadas

### 📊 Cenários de Teste

- ✅ **Entrada normal** de moto
- ✅ **Saída normal** de moto
- ✅ **Notificações não lidas**
- ✅ **Marcar como lida**
- ✅ **Busca por placa**
- ✅ **Consulta de vagas livres**
- ✅ **Logs com filtros**
- ✅ **Estatísticas em tempo real**

---

## 🛠️ Troubleshooting

### ❌ Problemas Comuns

1. **401 Unauthorized**
   - Verificar se o token JWT está correto
   - Verificar se o token não expirou

2. **500 Internal Server Error**
   - Verificar se o Redis está funcionando
   - Verificar logs do serviço

3. **404 Not Found**
   - Verificar se a URL está correta
   - Verificar se o serviço está rodando na porta 5003

### 🔧 Soluções

- **Reiniciar o serviço** se necessário
- **Verificar logs** no console do `dotnet run`
- **Testar health endpoint** primeiro
- **Verificar configurações** de banco de dados

---

## 📞 Suporte

Para dúvidas ou problemas:

1. **Verificar logs** do serviço
2. **Consultar documentação** da API
3. **Testar endpoints** individualmente
4. **Verificar configurações** de ambiente

---

## 🎉 Conclusão

Com estes comandos Postman e scripts SQL, você tem tudo o que precisa para:

- ✅ **Testar completamente** o MotoNotificationController
- ✅ **Criar a estrutura** de banco de dados
- ✅ **Validar o funcionamento** do sistema de notificações
- ✅ **Monitorar** logs e estatísticas

**Boa sorte com os testes! 🚀**