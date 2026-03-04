# PulsePoll Proje Hafızası

## Proje Özeti
Panel platformu: Kullanıcılar demografik profillerine göre anketlere katılır, ücret kazanır.

## Stack
- Backend API: .NET Core 10 Web API (sadece mobile için)
- Admin Panel: Blazor Server + DevExpress Blazor (servis katmanını direkt çağırır, API'ye gitmez)
- Mobile: .NET MAUI (sadece Android + iOS) + CommunityToolkit.Mvvm
- DB: PostgreSQL 16 + EF Core + Npgsql
- Cache: Redis 7 (StackExchange.Redis)
- Queue: RabbitMQ 3 + MassTransit
- Storage: MinIO (S3 uyumlu)
- Logging: Serilog + Seq
- Validation: FluentValidation

## Disk Yapısı
```
PulsePoll/
├── src/              ← 7 kaynak proje
│   ├── PulsePoll.Domain
│   ├── PulsePoll.Application
│   ├── PulsePoll.Infrastructure
│   ├── PulsePoll.Api
│   ├── PulsePoll.Worker
│   ├── PulsePoll.Admin
│   └── PulsePoll.Mobile
├── tests/            ← PulsePoll.Tests
├── deploy/           ← docker-compose.yml, nginx.conf
├── docs/
└── PulsePoll.sln     ← Solution Folder'larla gruplu
```

## Solution Yapısı (8 proje)
```
PulsePoll.Domain        ← Entity, Enum, Event (bağımsız)
PulsePoll.Application   ← Interface, DTO, Service stub, Validator
PulsePoll.Infrastructure← EF Core, Redis, RabbitMQ, MinIO, FCM, SMTP
PulsePoll.Api           ← Web API (mobile için, JWT auth)
PulsePoll.Worker        ← MassTransit consumer'lar
PulsePoll.Admin         ← Blazor Server + DevExpress
PulsePoll.Mobile        ← MAUI (Android + iOS)
PulsePoll.Tests         ← xUnit + Moq
```

## Kritik Mimari Kararlar
- Admin direkt Application servislerini çağırır (API bypass)
- Mobile HTTP ile API'ye konuşur, proje referansı yok
- Anlık cevap gerektirmeyen işlemler kuyruğa gider
- Kayıt: 202 Accepted → user.registered kuyruğu → Worker DB'ye yazar
- Anket tamamlama: webhook → survey.completed → Worker kazanç hesaplar

## RabbitMQ Kuyrukları
- user.registered, survey.completed, notification.send, withdrawal.requested, wallet.credit

## Redis Key Pattern'leri
- session:{userId} (7gün), surveys:list:{userId} (5dk), ratelimit:register:{ip} (1dk)

## Mobile Tema Sistemi
- 4 tema: PurpleLavender (varsayılan), SlateBlue, DeepTeal, RoseIndigo
- ResourceDictionary olarak tanımlı, Themes/ klasöründe
- Varsayılan Primary: #7C5CFC (Mor)
- Font: Plus Jakarta Sans (800/700/600/500 weight)

## Domain Mimarisi (Subject/AdminUser)
- Mobile kullanıcı = Subject (EntityBase : int Id, soft-delete, audit)
- Admin kullanıcı = AdminUser (EntityBase)
- Subject: 30 alan + 10 navigation property (City, District, Profession×2, EducationLevel×2, Bank, SocioeconomicStatus, LSMSocioeconomicStatus, SpecialCode)
- Enums: Gender, MaritalStatus, GsmOperator, ApprovalStatus (hepsi Description attribute'lu)
- Custom attributes: AgeRangeAttribute(18-90), RequiredIfAttribute
- Lookup tablolar: City, District, Profession, EducationLevel, Bank, SocioeconomicStatus, LSMSocioeconomicStatus, SpecialCode
- Kuyruk: subject.registered (eski: user.registered)
- Application: ISubjectService, ISubjectRepository, SubjectService, SubjectRepository
- Eski User/UserPersona/UserInterest/UserRank tamamen silindi

## Önemli Notlar
- Mobile csproj: sadece net10.0-android;net10.0-ios (MacCatalyst ve Windows kaldırıldı)
- Admin csproj: DevExpress.Blazor 25.2.*
- Mobile'da Syncfusion kaldırıldı, CommunityToolkit.Maui kullanılıyor
- Application servislerinde NotImplementedException var (iskelet)
- AppShell: TabBar (Anasayfa, Geçmiş, Cüzdan, Profil)
