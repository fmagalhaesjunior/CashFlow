
# CashFlow Architecture Challenge

Solução arquitetural em **.NET 9** baseada em **CQRS, Outbox Pattern, mensageria com RabbitMQ e processamento assíncrono**, para controle de lançamentos financeiros e geração de saldo consolidado diário.

A arquitetura é composta por dois serviços principais:

- **TransactionService** → responsável pelo registro de lançamentos (débito/crédito)
- **BalanceService** → responsável pela consolidação diária do saldo

A comunicação entre serviços ocorre de forma **assíncrona via RabbitMQ**, garantindo desacoplamento e resiliência.

---

# Run in 5 Minutes

Execute os comandos abaixo:

```bash
docker compose up --build -d

dotnet tool install --global dotnet-ef

dotnet ef database update --project src/Services/TransactionService/CashFlow.TransactionService.Infra --startup-project src/Services/TransactionService/CashFlow.TransactionService.API

dotnet ef database update --project src/Services/BalanceService/CashFlow.BalanceService.Infrastructure --startup-project src/Services/BalanceService/CashFlow.BalanceService.API
```

---

# Arquitetura
<img width="871" height="843" alt="Desenho-Tecnico" src="https://github.com/user-attachments/assets/2fa1d8eb-07db-47b3-a1d8-3ac184b03df5" />


Serviços e portas:

| Serviço | Porta |
|-------|------|
TransactionService | 8080 |
BalanceService | 8081 |
RabbitMQ | 5672 |
RabbitMQ Management | 15672 |
PostgreSQL | 5432 |

---

# Tecnologias Utilizadas

- .NET 9
- ASP.NET Minimal APIs
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Docker
- Docker Compose
- CQRS
- Outbox Pattern
- OpenTelemetry Metrics

---

# Pré‑requisitos

Antes de iniciar, certifique-se de ter instalado:

- Docker
- Docker Compose
- .NET SDK 9

Verifique:

```bash
docker --version
docker compose version
dotnet --version
```

---

# 1. Clonar o repositório

```bash
git clone https://github.com/fmagalhaesjunior/CashFlow.git

cd cashflow-architecture-challenge
```

---

# 2. Subir infraestrutura

Este comando irá subir:

- PostgreSQL
- RabbitMQ
- TransactionService
- BalanceService

```bash
docker compose up --build -d
```

Verificar containers:

```bash
docker compose ps
```

---

# 3. Instalar ferramenta de migrations

Caso não tenha:

```bash
dotnet tool install --global dotnet-ef
```

Verificar:

```bash
dotnet ef --version
```

---

# 4. Executar migrations do TransactionService

```bash
dotnet ef database update --project src/Services/TransactionService/CashFlow.TransactionService.Infra --startup-project src/Services/TransactionService/CashFlow.TransactionService.API
```

Tabelas criadas:

- transactions
- outbox_messages

Banco:

```
cashflow_transaction
```

---

# 5. Executar migrations do BalanceService

```bash
dotnet ef database update --project src/Services/BalanceService/CashFlow.BalanceService.Infrastructure --startup-project src/Services/BalanceService/CashFlow.BalanceService.API
```

Tabelas criadas:

- daily_balance
- processed_events

Banco:

```
cashflow_balance
```

---

# 6. Verificar Health Check

TransactionService:

```
http://localhost:8080/health
```

BalanceService:

```
http://localhost:8081/health
```

Resposta esperada:

```json
{
  "status": "healthy"
}
```

---

# 7. Acessar RabbitMQ

Interface de administração:

```
http://localhost:15672
```

Login:

```
user: guest
password: guest
```

---

# 8. Teste ponta a ponta

## Criar crédito

```bash
curl --request POST "http://localhost:8080/transactions" --header "Content-Type: application/json" --header "X-Correlation-Id: credit-test-001" --data '{
  "amount": 150,
  "type": 1,
  "description": "Venda do dia"
}'
```

---

## Criar débito

```bash
curl --request POST "http://localhost:8080/transactions" --header "Content-Type: application/json" --header "X-Correlation-Id: debit-test-001" --data '{
  "amount": 40,
  "type": 2,
  "description": "Pagamento fornecedor"
}'
```

---

# 9. Consultar saldo diário

```bash
curl "http://localhost:8081/daily-balance/2026-03-11"
```

Resposta esperada:

```json
{
  "date": "2026-03-11",
  "totalCredit": 150,
  "totalDebit": 40,
  "balance": 110
}
```

---

# Documentação API (Scalar)

TransactionService

```
http://localhost:8080/scalar
```

BalanceService

```
http://localhost:8081/scalar
```

---

# Logs

Ver logs dos serviços:

```bash
docker compose logs -f transaction-service
```

```bash
docker compose logs -f balance-service
```

---

# Parar ambiente

```bash
docker compose down
```

Remover volumes:

```bash
docker compose down -v
```

---

# Evoluções futuras

- Deploy em Kubernetes
- Cache Redis para queries
- Dashboard Grafana
- Retry com backoff exponencial
- Multi‑tenant
- Versionamento de eventos

---

# Autor

Francisco Magalhães de Barros Junior
