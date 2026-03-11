# CashFlow Architecture Challenge

## Visão Geral
Solução arquitetural em .NET 8 baseada em CQRS, Outbox Pattern, mensageria com RabbitMQ e processamento assíncrono para consolidação diária de saldo.

## Componentes
- transaction-service
- balance-service
- rabbitmq
- postgres

## Requisitos
- Docker
- Docker Compose
- .NET SDK 8

## Subindo o ambiente
```bash
docker compose up --build -d
