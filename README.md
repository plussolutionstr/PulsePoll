# PulsePoll

Panel araştırma platformu — denekler anket doldurur, puan/para kazanır.

## Mimari

Clean Architecture: **Domain → Application → Infrastructure → Api / Worker / Admin / Mobile**

| Proje | Açıklama |
|---|---|
| `PulsePoll.Api` | REST API (JWT auth, mobile istemci) |
| `PulsePoll.Admin` | Blazor Server yönetim paneli (Cookie auth) |
| `PulsePoll.Worker` | MassTransit consumer'ları (RabbitMQ) |
| `PulsePoll.Mobile` | .NET MAUI mobil uygulama (iOS + Android) |

## Gereksinimler

- .NET 10 SDK
- PostgreSQL
- Redis
- MinIO (S3-uyumlu nesne depolama)
- RabbitMQ

## Build

```bash
dotnet build src/PulsePoll.Api/
dotnet build src/PulsePoll.Admin/
dotnet build src/PulsePoll.Worker/
dotnet build src/PulsePoll.Mobile/
```

## Test

```bash
dotnet test tests/PulsePoll.Tests/
```

## Veritabanı Migration

```bash
dotnet ef database update --project src/PulsePoll.Infrastructure --startup-project src/PulsePoll.Api
```
