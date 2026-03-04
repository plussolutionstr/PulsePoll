# PulsePoll — Proje Brief

> Bu döküman, projenin tüm mimari kararlarını, teknoloji seçimlerini ve yapısal detaylarını içerir.
> Wireframe dosyaları bu dökümanla birlikte referans alınmalıdır.

---

## 1. Proje Özeti

PulsePoll, kullanıcıların demografik ve psikografik profillerine göre hedeflenen anketlere katıldığı, tamamlanan anketler karşılığında ücret kazandığı bir panel platformudur. Her ankete bir dış link atanır. Kullanıcı ankete tıkladığında o linke yönlendirilir.

### Hedef Metrikler
- 100.000 kayıtlı kullanıcı kapasitesi
- Eşzamanlı 7.000–10.000 aktif kullanıcı
- Reklam dönemlerinde kayıt spike'larına dayanıklı mimari

---

## 2. Teknoloji Stack

| Katman | Teknoloji |
|---|---|
| Backend API | .NET Core 10 Web API |
| Admin Panel | Blazor Server + DevExpress Blazor |
| Mobil | .NET MAUI + DevExpress MAUI |
| Veritabanı | PostgreSQL 16 |
| Cache | Redis 7 |
| Mesaj Kuyruğu | RabbitMQ 3 (MassTransit ile) |
| Dosya Depolama | MinIO (S3 uyumlu) |
| Reverse Proxy | Nginx |
| Logging | Serilog + Seq |
| ORM | Entity Framework Core + Npgsql |
| Validation | FluentValidation |
| Test | xUnit + Moq |
| Container | Docker + Docker Compose |

---

## 3. Mimari Kararlar

### 3.1 Genel Prensipler

- **API sadece mobile içindir.** Blazor Admin panel doğrudan servis katmanını kullanır, API'ye istek atmaz.
- **Mobile tamamen bağımsızdır.** MAUI projesi hiçbir C# projesine proje referansı vermez, sadece HTTP ile konuşur.
- **Shared logic tek yerdedir.** `PulsePoll.Application` katmanındaki servisler hem API hem Worker hem Admin tarafından kullanılır. Aynı logic üç kez yazılmaz.
- **Anlık cevap gerektirmeyen her şey kuyruğa gider.** Kullanıcı beklemez, sistem spike'a dayanıklı kalır.
- **Tüm state dışarıda tutulur.** API container'ları stateless'tır, Redis/DB state'i taşır. Bu sayede API yatay olarak ölçeklenebilir.

### 3.2 Anket Akışı

1. Anketler panelden **elle** eşleştirilir — otomatik persona matching yoktur.
2. Eşleştirme yapıldığında hedef kullanıcılara push notification gider (kuyruk üzerinden).
3. Kullanıcı ankete tıklar → ankete tanımlanmış dış linke yönlendirilir.
4. Kullanıcı anketi tamamladığında `POST /api/webhook/survey-complete` endpoint'ini çağırır.
5. Webhook mesajı `survey.completed` kuyruğuna düşer.
6. Worker kazancı cüzdana yazar, kullanıcıya bildirim atar.

### 3.3 Spike Koruması

Reklam dönemlerinde binlerce kişi aynı anda kayıt olmaya çalışabilir. Bunu absorbe etmek için:

- **Nginx rate limiting:** Kayıt endpoint'i IP başına dakikada 5 istek ile sınırlıdır.
- **Kayıt kuyruğa alınır:** Form submit edildiğinde kullanıcıya "Kaydınız alındı" dönülür, gerçek kayıt işlemi `user.registered` kuyruğundan worker tarafından yapılır.
- **Anket listesi Redis'te cache'lenir:** 5 dakika TTL. Her kullanıcı isteğinde DB'ye gidilmez.
- **JWT token Redis'te cache'lenir:** Her istekte DB doğrulaması yapılmaz.

### 3.4 Admin Panel — Blazor Server

Blazor Server seçildi çünkü:
- Kullanıcı sayısı az (sadece operatörler kullanır)
- Sunucu tarafında çalışır, hassas işlemler direkt server'da kalır
- DevExpress Blazor bileşenleri ile hızlı geliştirme
- API'ye gerek kalmadan servis katmanını direkt çağırır

### 3.5 Mobile — MAUI Tema Sistemi

Kullanıcı profil tercihlerine göre 4 tema arasından seçim yapabilir:
- **Mor & Lavanta** (varsayılan)
- **Slate Blue & Mint**
- **Deep Teal & Amber**
- **Rose & Indigo**

Her tema `ResourceDictionary` olarak tanımlanır. Tema tercihi `UserService`'te saklanır. Uygulama açılışında kullanıcının tercihi yüklenir ve ilgili `ResourceDictionary` aktif edilir.

---

## 4. Solution Yapısı

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
│   │   ├── WebhookController.cs  # POST /webhook/survey-complete (anket tamamlama callback)
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
│   │   ├── ActiveSurveyPage.xaml # Soru akışı (dış linke yönlendirme)
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

## 5. Proje Bağımlılıkları

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

## 6. RabbitMQ Kuyruklarl

| Kuyruk | Publisher | Consumer | Açıklama |
|---|---|---|---|
| `user.registered` | AuthController | UserRegisteredConsumer | Kayıt formu submit → worker tamamlar |
| `survey.completed` | WebhookController | SurveyCompletedConsumer | Anket tamamlama callback → kazanç hesapla |
| `notification.send` | Her yer | NotificationSendConsumer | FCM push gönderimi |
| `withdrawal.requested` | WalletController | WithdrawalRequestedConsumer | Para çekimi talebi |
| `wallet.credit` | SurveyCompletedConsumer | WalletCreditConsumer | Cüzdana para yaz |

---

## 7. Redis Kullanımı

| Key Pattern | TTL | Açıklama |
|---|---|---|
| `session:{userId}` | 7 gün | JWT token cache |
| `surveys:list:{userId}` | 5 dk | Kullanıcıya göre anket listesi |
| `ratelimit:register:{ip}` | 1 dk | Kayıt rate limit sayacı |
| `ratelimit:login:{ip}` | 1 dk | Login rate limit sayacı |
| `user:theme:{userId}` | Kalıcı | Kullanıcı tema tercihi |

---

## 8. API Endpoint'leri

### Auth
```
POST   /api/auth/register          # Kuyruğa atar, 202 Accepted döner
POST   /api/auth/login             # JWT + refresh token döner
POST   /api/auth/refresh           # Token yenileme
POST   /api/auth/logout            # Token invalidate
```

### Survey
```
GET    /api/surveys                # Kullanıcıya atanmış anketler (Redis cache)
GET    /api/surveys/{id}           # Anket detay + uygunluk kontrolü
POST   /api/surveys/{id}/start     # Ankete ait dış linki döner
```

### Profile
```
GET    /api/profile                # Kullanıcı profili + persona
PUT    /api/profile                # Profil güncelleme
PUT    /api/profile/photo          # MinIO'ya yükle
PUT    /api/profile/theme          # Tema tercihi kaydet
```

### Wallet
```
GET    /api/wallet                 # Bakiye + son işlemler
GET    /api/wallet/banks           # Kayıtlı banka hesapları
POST   /api/wallet/banks           # Banka hesabı ekle
DELETE /api/wallet/banks/{id}      # Banka hesabı sil
POST   /api/wallet/withdraw        # Para çekimi talebi → kuyruğa
```

### Story & Notification
```
GET    /api/stories                # Aktif story'ler
GET    /api/notifications          # Kullanıcı bildirimleri
PUT    /api/notifications/read-all # Tümünü okundu işaretle
```

### Webhook
```
POST   /api/webhook/survey-complete  # Anket tamamlama callback → kuyruğa atar
```

---

## 9. Veritabanı Şeması (Ana Tablolar)

```sql
-- Kullanıcılar
Users
  Id, Email, PasswordHash, PhoneNumber
  FirstName, LastName, ProfilePhotoUrl
  Rank (Bronze/Silver/Gold/Platinum), Points
  ThemePreference, FcmToken
  CreatedAt, IsActive

-- Persona (demografik bilgiler)
UserPersonas
  UserId, Age, Gender, Education
  MaritalStatus, City, District
  Occupation, IncomeLevel
  UpdatedAt

-- İlgi alanları
UserInterests
  UserId, InterestTag

-- Anketler
Surveys
  Id, Title, Description, BrandName, BrandLogoUrl
  RewardAmount, EstimatedMinutes, QuestionCount
  SurveyUrl                 -- ankete yönlendirilecek dış link
  Status, StartsAt, EndsAt, CreatedAt

-- Anket-Kullanıcı ataması (panelden elle yapılır)
SurveyAssignments
  SurveyId, UserId
  AssignedAt, Status (Pending/Started/Completed/Eliminated)
  CompletedAt, EarnedAmount

-- Cüzdan
Wallets
  UserId, Balance, TotalEarned, UpdatedAt

-- Cüzdan işlemleri
WalletTransactions
  Id, WalletId, Amount, Type (Credit/Withdrawal)
  Description, ReferenceId, CreatedAt

-- Banka hesapları
BankAccounts
  Id, UserId, BankName, IbanLast4, IbanEncrypted
  IsDefault, CreatedAt

-- Bildirimler
Notifications
  Id, UserId, Title, Body, Type
  IsRead, CreatedAt

-- Story
Stories
  Id, Title, ImageUrl, LinkUrl
  BrandName, StartsAt, EndsAt, IsActive, Order
```

---

## 10. Docker Compose Servisleri

```yaml
services:
  nginx:        nginx:alpine          ports: 80, 443
  api:          pulsepoll/api         port: 8080
  admin:        pulsepoll/admin       port: 8081
  worker:       pulsepoll/worker      (internal)
  postgres:     postgres:16-alpine    port: 5432
  redis:        redis:7-alpine        port: 6379
  rabbitmq:     rabbitmq:3-management ports: 5672, 15672
  minio:        minio/minio           ports: 9000, 9001
  seq:          datalust/seq          port: 5341
```

---

## 11. MAUI Ekranları ve Wireframe Eşleştirmesi

Wireframe dosyasında aşağıdaki ekranlar bulunmaktadır:

| Wireframe | MAUI View | Açıklama |
|---|---|---|
| 01 — Ana Sayfa | `HomePage.xaml` | Stories + featured haber + anket listesi |
| 01b — Ana Sayfa Görsel | `HomePage.xaml` | Görselli varyant (aynı view, farklı template) |
| 02 — Anket Bilgi | `SurveyDetailPage.xaml` | Marka hero + meta + uygunluk + CTA |
| 03 — Aktif Soru | `ActiveSurveyPage.xaml` | Progress + soru + seçenekler (dış linke yönlendirme öncesi) |
| 04 — Profil | `ProfilePage.xaml` | Avatar + rank + demografi + ilgi alanları |
| 05 — Geçmiş | `HistoryPage.xaml` | Özet istatistik + filtre + kayıt listesi |
| 06 — Cüzdan | `WalletPage.xaml` | Bakiye kartı + banka hesapları + işlemler |
| 07 — Bildirimler | `NotificationsPage.xaml` | Filtre + gruplu bildirim listesi |

### Ana Sayfa Yapısı
```
HomePage
├── TopBar (Logo + Bildirim ikonu)
├── StoriesRow (CarouselView — yatay scroll)
├── FeaturedNewsCard (büyük gradient kart — slider)
├── SectionHeader ("Sana Özel" + "Tümü" linki)
└── SurveyList (CollectionView)
    └── SurveyCard (marka logo + başlık + kazanç + chip'ler)
```

### Bottom Navigation
```
Shell tabs:
  - Anasayfa   (home icon)
  - Anketler   (grid icon)
  - Geçmiş     (activity icon)
  - Cüzdan     (card icon)
  - Profil     (user icon)
```

---

## 12. Tasarım Sistemi (MAUI Tema)

### Varsayılan Renk Paleti (Mor & Lavanta)
```xml
<!-- Arka planlar -->
<Color x:Key="PageBackground">#F7F5FF</Color>
<Color x:Key="CardBackground">#FFFFFF</Color>
<Color x:Key="Surface2">#F0EDF9</Color>
<Color x:Key="BorderColor">#E8E3F5</Color>

<!-- Ana renk -->
<Color x:Key="Primary">#7C5CFC</Color>
<Color x:Key="PrimaryLight">#EDE8FF</Color>
<Color x:Key="PrimaryMid">#C4B5FD</Color>
<Color x:Key="PrimaryDark">#5B3FD9</Color>

<!-- Metin -->
<Color x:Key="TextPrimary">#1A1535</Color>
<Color x:Key="TextSecondary">#6B6589</Color>
<Color x:Key="TextTertiary">#B0AABE</Color>

<!-- Durum renkleri -->
<Color x:Key="Success">#22C988</Color>
<Color x:Key="SuccessLight">#E6FBF4</Color>
<Color x:Key="Error">#FF5C7A</Color>
<Color x:Key="ErrorLight">#FFF0F3</Color>
<Color x:Key="Amber">#F59E0B</Color>
<Color x:Key="AmberLight">#FFFBEB</Color>
<Color x:Key="Info">#3B82F6</Color>
<Color x:Key="InfoLight">#EFF6FF</Color>
```

### Font
- **Plus Jakarta Sans** — tüm ekranlarda kullanılır
- Heading: 800 weight
- Subheading: 700 weight
- Body: 500–600 weight
- Caption: 400–500 weight

### Border Radius
- Kart: 20px
- Küçük bileşen: 12px
- Chip/badge: 8–10px
- Pill: 20px+

---

## 13. Önemli Notlar

### Anket Link Yapısı
- Her ankete panelden bir dış URL atanır.
- Kullanıcı ankete başladığında bu URL'e yönlendirilir.
- Anket sağlayıcısı tamamlama sonrası `POST /api/webhook/survey-complete` adresine callback atar.
- Webhook entegrasyonu anket sağlayıcısına göre şekillenecektir — detaylar sonradan eklenecektir.

### Kayıt Akışı
1. Kullanıcı formu doldurur ve gönderir.
2. API `user.registered` kuyruğuna mesaj atar → `202 Accepted` döner.
3. Kullanıcıya "Kaydınız alındı, e-postanızı doğrulayın" mesajı gösterilir.
4. Worker mesajı alır, DB'ye kullanıcıyı yazar, doğrulama maili atar.
5. Kullanıcı maildeki linke tıklar → hesap aktif olur.

### Para Çekimi Akışı
1. Kullanıcı çekim talebi oluşturur.
2. API `withdrawal.requested` kuyruğuna atar → `202 Accepted` döner.
3. Admin panelde talep görünür, operatör onaylar/reddeder.
4. Onay sonrası banka transferi işlenir, kullanıcıya bildirim gider.

### Story Yönetimi
- Instagram story entegrasyonu yoktur. Story'ler admin panelden yönetilir.
- Her story: görsel (MinIO), başlık, link URL, başlangıç/bitiş tarihi, sıralama.
- Markalar kendi story'lerini atamak isterse admin panel üzerinden operatör girer.

### Ölçeklenme
- API container'ları yatay olarak çoğaltılabilir (Nginx round-robin).
- Worker container'ları bağımsız ölçeklenebilir.
- Tüm API instance'ları stateless — session Redis'te.
- İleride yük artarsa PostgreSQL önüne PgBouncer eklenebilir.

---

## 14. Geliştirme Sırası (Öneri)

1. `PulsePoll.Domain` → Entity ve enum'lar
2. `PulsePoll.Application` → Interface ve DTO'lar
3. `PulsePoll.Infrastructure` → DB context, migration, temel repository'ler
4. `PulsePoll.Api` → Tüm endpoint'ler
5. `PulsePoll.Worker` → Tüm consumer'lar
6. `PulsePoll.Admin` → Blazor panel
7. `PulsePoll.Mobile` → MAUI uygulama
8. `PulsePoll.Tests` → Kritik servis testleri
