# 🏦 NovaBankTech - Backend API

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](https://docs.docker.com/compose/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red.svg)](https://www.microsoft.com/sql-server/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-orange.svg)](https://www.rabbitmq.com/)

Sistema bancário digital completo desenvolvido com arquitetura de microserviços em .NET 9.0, oferecendo operações financeiras essenciais, sistema PIX, pagamentos via boleto e assistente virtual com IA.

## 🚀 Microserviços

| Serviço | Porta | Descrição | Tecnologias |
|---------|-------|-----------|-------------|
| **Auth.Api** | 5001 | Autenticação JWT e autorização | JWT Bearer, BCrypt |
| **Bank.Api** | 5002 | Usuários e operações bancárias | EF Core, Procedures SQL |
| **Email.Api** | 5003 | Notificações por email | RabbitMQ, SMTP |
| **Pix.Api** | 5004 | Sistema PIX brasileiro | Chaves PIX, Transferências |
| **ApiGateway** | 5005 | Gateway de entrada e roteamento | Yarp, Load Balancing |
| **Payments.Api** | 5007 | Boletos e pagamentos | Geração/Pagamento de boletos |
| **Ai.Api** | 5008 | Assistente virtual Nova | Gemini AI, Groq, Cache |
| **ReportApi** | 5006 | Relatórios em PDF/CSV | QuestPDF, Exportação |

### Infraestrutura
- **SQL Server** (1433) - Banco de dados relacional
- **RabbitMQ** (5672/15672) - Mensageria assíncrona
- **Docker Compose** - Orquestração de containers

## ⚡ Stack Tecnológica

**Backend Framework:**
- .NET 9.0 com ASP.NET Core Web API
- Entity Framework Core para ORM
- AutoMapper para mapeamento de objetos
- FluentValidation para validações

**Segurança:**
- JWT Bearer Authentication
- BCrypt para hash de senhas
- Role-based Authorization
- CORS configurado

**Banco de Dados:**
- SQL Server 2022
- Entity Framework Migrations
- Stored Procedures para operações críticas
- Indexes otimizados

**Mensageria:**
- RabbitMQ para eventos assíncronos
- Producer/Consumer patterns
- Dead letter queues

**Documentação:**
- Swagger/OpenAPI 3.0
- XML Documentation
- Postman Collections

**DevOps:**
- Docker Compose
- Health checks
- Logging estruturado (Serilog)
- Environment-based configuration

## 🔧 Funcionalidades Principais

### 🔐 Autenticação (Auth.Api)
```csharp
// Login flexível: conta, CPF ou email
POST /api/auth/login
POST /api/auth/refresh
```
- Login por número da conta, CPF ou email
- Múltiplas contas por usuário
- Tokens JWT com refresh automático
- Middleware de validação de token
- Controle de sessão e logout

### 👤 Gestão de Usuários (Bank.Api)
```csharp
// CRUD completo de usuários
GET    /api/GetAccountByNumber
POST   /api/create-user
PUT    /api/update-user
DELETE /api/delete-user/{accountNumber}
```
- Criação automática de conta corrente e poupança
- Atualização de dados pessoais e senhas
- Soft delete com controle de status
- Validação de CPF e email únicos
- Histórico completo de alterações

### 💰 Operações Bancárias (Bank.Api)
```csharp
// Operações financeiras seguras
POST /api/Transfer/Transfer        // Transferências
POST /api/deposit                  // Depósitos
POST /api/whitdraw                 // Saques
GET  /api/listmovimentation        // Histórico
GET  /api/history                  // Histórico paginado
```
- **Transferências** entre contas com validação de saldo
- **Depósitos e Saques** com procedures SQL transacionais
- **Histórico paginado** de movimentações
- **Extratos** por período com filtros
- **Validação de senha** para operações críticas

### 💳 Sistema PIX (Pix.Api)
```csharp
// PIX brasileiro completo
POST /api/registrar               // Criar chave PIX
POST /api/mandar                  // Transferir via PIX
POST /api/get                     // Consultar chave
GET  /api/has                     // Verificar se tem PIX
```
- Registro de chaves PIX (CPF, email, telefone, aleatória)
- Transferências instantâneas 24/7
- Consulta de dados por chave PIX
- Integração com operações bancárias
- Validação de chaves duplicadas

### 🧾 Pagamentos (Payments.Api)
```csharp
// Sistema completo de boletos
POST /api/GenerateBankSlip        // Gerar boleto
POST /api/PayBankSlip             // Pagar integral
POST /api/PayPartialBankSlip      // Pagar parcial
GET  /api/GetPendingBankSlips     // Boletos pendentes
GET  /api/GetPaidBankSlips        // Boletos pagos
GET  /api/ValidateBankSlip/{number} // Validar boleto
```
- Geração de boletos com formato brasileiro (18 dígitos)
- Pagamento integral ou parcial
- Validação de boletos por número
- Histórico completo de pagamentos
- Integração com operações bancárias

### 🤖 Assistente IA (Ai.Api)
```csharp
// Chatbot inteligente "Nova"
POST /api/Ask                     // Fazer pergunta
```
- Assistente virtual "Nova" especializada em banking
- Fallback automático: Gemini → Groq
- Cache inteligente para respostas frequentes
- Respostas contextualizadas sobre NovaBankTech
- Rate limiting e controle de uso

### 📧 Notificações (Email.Api)
```csharp
// Sistema de emails automatizado
POST /api/send-welcome            // Email de boas-vindas
```
- Emails automáticos de boas-vindas
- Templates HTML responsivos
- Consumer RabbitMQ para eventos
- Configuração SMTP flexível
- Tracking de entrega

### 📊 Relatórios (ReportApi)
```csharp
// Relatórios personalizados
GET /api/csv                      // Exportar CSV
GET /api/pdf                      // Exportar PDF
```
- Exportação em PDF e CSV
- Relatórios de movimentações
- Geração sob demanda
- Filtros por período e tipo
- Autenticação JWT obrigatória

## 🔄 Fluxos de Negócio

### Cadastro de Usuário
1. **POST** `/api/create-user` → Bank.Api
2. Criar usuário + 2 contas automáticas (corrente/poupança)
3. Hash da senha com BCrypt
4. Publicar evento `UserCreated` → RabbitMQ
5. Email.Api consome evento → Envio automático de boas-vindas

### Autenticação
1. **POST** `/api/auth/login` com conta/CPF/email
2. Validação de senha com BCrypt
3. Geração de JWT + Refresh Token
4. Retorno das contas disponíveis (se múltiplas)
5. Storage do token no banco para controle de sessão

### Transferência Bancária
1. **POST** `/api/Transfer/Transfer` com validação JWT
2. Verificação de senha do usuário
3. Validação de saldo disponível
4. Execução de procedure SQL transacional
5. Registro de movimentação para ambas as contas

### Transferência PIX
1. **POST** `/api/get` para consultar chave PIX
2. Verificação se conta destino existe internamente
3. **Débito** na conta origem via procedure
4. **Crédito** na conta destino (se interna)
5. Registro da transação PIX

### Pagamento de Boleto
1. **GET** `/api/ValidateBankSlip/{number}` para validação
2. Verificação de senha e saldo do usuário
3. **POST** `/api/PayBankSlip` com procedure específica
4. Atualização do status do boleto
5. Registro da movimentação bancária

## 🛠️ Execução do Projeto

### Docker Compose (Recomendado)
```bash
# Clonar o repositório
git clone https://github.com/DigitalVaultOf/NovaBankTech.git
cd NovaBankTech

# Subir toda a infraestrutura
docker-compose up -d

# Verificar status dos containers
docker-compose ps

# Verificar logs em tempo real
docker-compose logs -f [serviço]

# Parar todos os serviços
docker-compose down
```

### Desenvolvimento Local
```bash
# 1. Infraestrutura base
docker-compose up sqlserver rabbitmq -d

# 2. Executar serviços individualmente
dotnet run --project Auth.Api --urls "http://localhost:5001"
dotnet run --project Bank.Api --urls "http://localhost:5002"
dotnet run --project Email.Api --urls "http://localhost:5003"
dotnet run --project Pix.Api --urls "http://localhost:5004"
dotnet run --project ApiGateway --urls "http://localhost:5005"
dotnet run --project ReportApi --urls "http://localhost:5006"
dotnet run --project Payments.Api --urls "http://localhost:5007"
dotnet run --project Ai.Api --urls "http://localhost:5008"
```

### Restauração e Build
```bash
# Restaurar dependências de todos os projetos
dotnet restore

# Build da solution completa
dotnet build

# Executar migrations
dotnet ef database update --project Bank.Api
dotnet ef database update --project Pix.Api
dotnet ef database update --project Payments.Api
```

## 📡 Principais Endpoints

### Autenticação
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "senha123"
}
```

### Operações Bancárias
```http
# Transferência
POST /api/Transfer/Transfer
Authorization: Bearer {token}
Content-Type: application/json

{
  "accountNumberTo": "123456",
  "amount": 100.00,
  "description": "Pagamento",
  "password": "senha123"
}

# Consultar saldo
GET /api/GetAccountByNumber
Authorization: Bearer {token}
```

### Sistema PIX
```http
# Criar chave PIX
POST /api/registrar
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "João Silva",
  "pixKey": "11999999999",
  "bank": "NovaBank"
}

# Transferir via PIX
POST /api/mandar
Authorization: Bearer {token}
Content-Type: application/json

{
  "going": "11999999999",
  "coming": "user-pix-key",
  "amount": 50.00
}
```

### Boletos
```http
# Gerar boleto
POST /api/GenerateBankSlip
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": 200.00,
  "description": "Cobrança de serviços",
  "dueDate": "2024-08-15"
}

# Pagar boleto
POST /api/PayBankSlip
Authorization: Bearer {token}
Content-Type: application/json

{
  "bankSlipNumber": "26072024151045123",
  "userPassword": "senha123"
}
```

### Assistente IA
```http
# Conversar com Nova
POST /api/Ask
Authorization: Bearer {token}
Content-Type: application/json

{
  "question": "Como faço uma transferência?"
}
```

## 🔒 Configuração de Segurança

### Variáveis de Ambiente (.env)
```env
# Banco de Dados
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=UserBank;User Id=sa;Password=Senha123!;TrustServerCertificate=True

# JWT Settings
Jwt__Key=sua-chave-super-secreta-aqui-123456789-minimo-32-caracteres
Jwt__Issuer=AuthApi
Jwt__Audience=BankApp

# APIs de IA
Gemini__ApiKey=your-gemini-api-key-here
Groq__ApiKey=your-groq-api-key-here

# Configurações de Email
SmtpSettings__Server=smtp.gmail.com
SmtpSettings__Port=587
SmtpSettings__Username=projetodigitalvault@gmail.com
SmtpSettings__Password=your-app-password
SmtpSettings__EnableSSL=true

# RabbitMQ
RABBITMQ_DEFAULT_USER=guest
RABBITMQ_DEFAULT_PASS=guest

# Portas customizáveis
PAYMENTS_PORT=5007
AI_PORT=5008
```

### Configurações de Produção
```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "Swagger": {
    "EnableInProduction": false
  }
}
```

## 🧪 Testes e Validação

### Swagger/OpenAPI
```
http://localhost:5001/swagger  # Auth API
http://localhost:5002/swagger  # Bank API  
http://localhost:5003/swagger  # Email API
http://localhost:5004/swagger  # Pix API
http://localhost:5005/swagger  # API Gateway
http://localhost:5006/swagger  # Report API
http://localhost:5007/swagger  # Payments API
http://localhost:5008/swagger  # AI API
```

### Health Checks
```http
GET http://localhost:5001/health  # Status de cada serviço
GET http://localhost:5008/health  # AI API health check
```

### Postman Collection
```bash
# Importar collection para testes
curl -o NovaBankTech.postman_collection.json \
  https://api.postman.com/collections/your-collection-id
```

### Executar Testes
```bash
# Testes unitários
dotnet test

# Testes de integração
dotnet test --filter Category=Integration

# Coverage de código
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Monitoramento

### Logs
```bash
# Logs em tempo real
docker-compose logs -f auth
docker-compose logs -f user
docker-compose logs -f payments

# Logs estruturados com Serilog
tail -f logs/app-{date}.json
```

### Métricas
- Tempo de resposta das APIs
- Taxa de sucesso/erro por endpoint  
- Throughput de transações
- Uso de memória e CPU dos containers
- Status de conectividade com SQL Server e RabbitMQ

### RabbitMQ Management
```
http://localhost:15672
Username: guest
Password: guest
```

## 🔧 Troubleshooting

### Problemas Comuns

**Erro de Connection String**
```bash
# Verificar se SQL Server está rodando
docker-compose ps sqlserver

# Verificar logs do SQL Server  
docker-compose logs sqlserver
```

**Erro de JWT Token**
```bash
# Verificar se a chave tem pelo menos 32 caracteres
echo "sua-chave-super-secreta-aqui-123456789" | wc -c

# Verificar configuração no appsettings.json
cat Bank.Api/appsettings.json | grep -A 5 "Jwt"
```

## 📈 Performance

### Otimizações Implementadas
- Connection pooling no Entity Framework
- Indexes otimizados nas tabelas principais
- Cache de consultas frequentes (AI API)
- Lazy loading configurado
- Stored procedures para operações críticas
- Rate limiting para APIs públicas

### Benchmarks
- Autenticação: < 200ms
- Transferências: < 500ms  
- Consulta de saldo: < 100ms
- Geração de boleto: < 300ms
- PIX: < 400ms

## 🚀 Deploy em Produção

### Docker Registry
```bash
# Build e push das imagens
docker build -t your-registry/novabankttech-auth:latest ./Auth.Api
docker push your-registry/novabankttech-auth:latest

# Repetir para todos os serviços...
```

## 📄 Licença

Este projeto foi desenvolvido para fins educacionais como projeto final do curso de .NET.

## 👥 Equipe de Desenvolvimento

**Grupo 9 - DigitalVault**
- **Raul Netto** - Full Stack Developer
- **Danilo Cossiolo Dias** - Full Stack Developer
- **Eduardo Cimitan** - Full Stack Developer
- **MOISÉS GABRIEL DE CARIS** - Full Stack Developer
- **Marcos "H0wZy" Junior** - Full Stack Developer


## 📞 Contato e Suporte

- **Email**: projetodigitalvault@gmail.com
- **GitHub**: [DigitalVaultOf](https://github.com/DigitalVaultOf)
- **LinkedIn**: [Publicação (parte 1) do projeto](https://www.linkedin.com/posts/marcosh0wzy_novabanktech-digitalvault-dotnet-activity-7354871854899126273-SpSN?utm_source=share&utm_medium=member_desktop&rcm=ACoAAEaCRhsBbzfa6nk3JyKJnzsHepOj5V6qRmY)
- **LinkedIn**: [Publicação (parte 2) do projeto](https://www.linkedin.com/posts/marcosh0wzy_novabanktech-digitalvault-dotnet-activity-7354872551241662464-hDDA/?utm_source=share&utm_medium=member_desktop&rcm=ACoAAEaCRhsBbzfa6nk3JyKJnzsHepOj5V6qRmY)

Para dúvidas técnicas, abra uma issue no GitHub ou entre em contato via email.

---

**Desenvolvido com ❤️ pelo Grupo 9 - DigitalVault**  
*"Construindo o futuro dos serviços bancários digitais"*
