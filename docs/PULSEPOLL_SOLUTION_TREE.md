# PulsePoll — Solution Ağacı

## Proje Bağımlılıkları

```
Domain          ← hiçbir şeye bağlı değil
Application     ← Domain
Infrastructure  ← Application
Api             ← Application + Infrastructure
Worker          ← Application + Infrastructure
Admin           ← Application + Infrastructure
Mobile          ← hiçbir projeye bağlı değil (sadece HTTP)
Tests           ← Application + Infrastructure
```

---

## Solution Yapısı

```
PulsePoll.sln
│
├── PulsePoll.Domain              # Entity, Enum, Domain Event, Exception
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Survey.cs
│   │   ├── Wallet.cs
│   │   ├── WalletTransaction.cs
│   │   ├── Notification.cs
│   │   └── Story.cs
│   ├── Enums/
│   │   ├── UserRank.cs           # Bronze, Silver, Gold, Platinum
│   │   ├── SurveyStatus.cs       # Active, Completed, Expired
│   │   └── WalletTransactionType.cs  # Credit, Withdrawal
│   └── Events/
│       ├── UserRegistered.cs
│       ├── SurveyCompleted.cs
│       └── WalletCredited.cs
│
├── PulsePoll.Application         # Interface, DTO, Service Implementation
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   ├── ISurveyService.cs
│   │   ├── IWalletService.cs
│   │   ├── INotificationService.cs
│   │   └── IStorageService.cs
│   ├── DTOs/
│   │   ├── UserDto.cs
│   │   ├── SurveyDto.cs
│   │   ├── WalletDto.cs
│   │   └── NotificationDto.cs
│   ├── Services/
│   │   ├── UserService.cs
│   │   ├── SurveyService.cs
│   │   ├── WalletService.cs
│   │   └── NotificationService.cs
│   └── Validators/
│       ├── RegisterRequestValidator.cs
│       └── WithdrawalRequestValidator.cs
│
├── PulsePoll.Infrastructure      # EF Core, Redis, RabbitMQ, MinIO
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/       # IEntityTypeConfiguration sınıfları
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       ├── UserRepository.cs
│   │       └── SurveyRepository.cs
│   ├── Messaging/
│   │   ├── RabbitMqPublisher.cs
│   │   └── MessageContracts.cs   # Kuyruk mesaj modelleri
│   ├── Caching/
│   │   └── RedisCacheService.cs
│   ├── Storage/
│   │   └── MinioStorageService.cs
│   └── Notifications/
│       ├── FcmPushService.cs
│       └── SmtpMailService.cs
│
├── PulsePoll.Api                 # .NET Core Web API — sadece mobile için
│   ├── Controllers/
│   │   ├── AuthController.cs     # POST /auth/register, /auth/login, /auth/refresh
│   │   ├── SurveyController.cs   # GET /surveys, /surveys/{id}
│   │   ├── ProfileController.cs  # GET/PUT /profile, PUT /profile/photo
│   │   ├── WalletController.cs   # GET /wallet, POST /wallet/withdraw
│   │   ├── WebhookController.cs  # POST /webhook/survey-complete
│   │   └── StoryController.cs    # GET /stories
│   ├── Middleware/
│   │   ├── ExceptionMiddleware.cs
│   │   └── JwtMiddleware.cs
│   ├── Extensions/
│   │   └── ServiceExtensions.cs  # DI kayıtları
│   ├── Program.cs
│   └── appsettings.json
│
├── PulsePoll.Worker              # .NET Worker Service — kuyruk consumer'ları
│   ├── Consumers/
│   │   ├── UserRegisteredConsumer.cs      # Kayıt tamamlama, hoşgeldin maili
│   │   ├── SurveyCompletedConsumer.cs     # Kazanç hesaplama → wallet.credit kuyruğuna
│   │   ├── NotificationSendConsumer.cs    # FCM push gönderimi
│   │   ├── WithdrawalRequestedConsumer.cs # Para çekimi işleme
│   │   └── WalletCreditConsumer.cs        # Cüzdana yazma
│   ├── Program.cs
│   └── appsettings.json
│
├── PulsePoll.Admin               # Blazor Server — admin panel
│   ├── Pages/
│   │   ├── Dashboard.razor
│   │   ├── Users.razor           # Kullanıcı listesi, detay, engelleme
│   │   ├── Surveys.razor         # Anket oluşturma, kullanıcılara atama
│   │   ├── Stories.razor         # Story yönetimi (görsel + süre + link)
│   │   └── Wallet.razor          # Para çekimi onayları
│   ├── Components/
│   │   ├── SurveyForm.razor
│   │   ├── UserGrid.razor
│   │   └── StoryUpload.razor
│   ├── Layout/
│   └── Program.cs
│
├── PulsePoll.Mobile              # .NET MAUI
│   ├── Views/
│   │   ├── HomePage.xaml         # Stories + News + Anket listesi
│   │   ├── SurveyDetailPage.xaml # Anket bilgi, uygunluk, başlat
│   │   ├── ActiveSurveyPage.xaml # Dış linke yönlendirme
│   │   ├── ProfilePage.xaml      # Profil + persona + ilgi alanları
│   │   ├── HistoryPage.xaml      # Tamamlanan/elenen anketler
│   │   ├── WalletPage.xaml       # Bakiye + banka hesapları + işlemler
│   │   └── NotificationsPage.xaml
│   ├── ViewModels/
│   │   ├── HomeViewModel.cs
│   │   ├── SurveyViewModel.cs
│   │   ├── ProfileViewModel.cs
│   │   ├── WalletViewModel.cs
│   │   └── NotificationsViewModel.cs
│   ├── Services/
│   │   ├── ApiService.cs         # HTTP client, base URL, token yönetimi
│   │   ├── AuthService.cs        # Login, register, token refresh
│   │   └── PushService.cs        # FCM token kayıt
│   ├── Controls/
│   │   ├── StoryRing.xaml        # Gradient ring'li story bileşeni
│   │   ├── SurveyCard.xaml       # Anket kartı
│   │   └── EarnBadge.xaml        # Kazanç göstergesi
│   └── Themes/
│       ├── ThemeBase.xaml        # Ortak değişkenler
│       ├── PurpleLavender.xaml
│       ├── SlateBlue.xaml
│       ├── DeepTeal.xaml
│       └── RoseIndigo.xaml
│
├── PulsePoll.Tests               # xUnit + Moq
│   ├── UserServiceTests.cs
│   └── WalletServiceTests.cs
│
├── docker-compose.yml
├── docker-compose.override.yml   # Geliştirme ortamı port'ları
└── nginx.conf
```

---

## Temel NuGet Paketleri

| Paket | Proje |
|---|---|
| EF Core + Npgsql | Infrastructure |
| MassTransit RabbitMQ | Api, Worker |
| StackExchange.Redis | Infrastructure |
| Minio | Infrastructure |
| Serilog + Seq | Tümü |
| FluentValidation | Application |
| DevExpress MAUI | Mobile |
| DevExpress Blazor | Admin |
| xUnit + Moq | Tests |
