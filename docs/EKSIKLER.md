# Eksikler & Yapılacaklar

> Backend bitmeden front'a geçilmeyecek.

---

## Mobile (PulsePoll.Mobile)

### Sayfalar
- [ ] Login sayfası yok (`LoginPage.xaml` + `LoginViewModel`)
- [ ] Register sayfası yok (`RegisterPage.xaml` + `RegisterViewModel`)
- [ ] Uygulama direkt AppShell'e açılıyor, auth guard yok

### AppShell
- [ ] 5 tab olması gerekiyor — **Anketler sekmesi eksik** (brief: Anasayfa, Anketler, Geçmiş, Cüzdan, Profil)

### Sayfaların içerikleri
- [ ] `SurveyDetailPage.xaml` — iskelet, boş
- [ ] `HistoryPage.xaml` — iskelet, boş
- [ ] `ProfilePage.xaml` — iskelet, boş
- [ ] `WalletPage.xaml` — iskelet, boş
- [ ] `NotificationsPage.xaml` — iskelet, boş

### Controls (brief'te var, projede yok)
- [ ] `StoryRing.xaml` — gradient ring'li story bileşeni
- [ ] `SurveyCard.xaml` — anket kartı
- [ ] `EarnBadge.xaml` — kazanç göstergesi

### Servisler
- [ ] `PushService.cs` yok (FCM token kayıt)
- [ ] Tema değiştirme mekanizması yok (`App.xaml.cs`'de switch logic eksik)

### Modeller
- [ ] `ProfileModel` Subject mimarisine uygun değil (eski `Rank`, `Points`, `ThemePreference` alanları var)

### Fontlar
- [ ] Plus Jakarta Sans dosyaları `Resources/Fonts/` klasöründe yok (MauiProgram'da kayıtlı ama ttf yok)

---

## Backend (API + Worker + Application + Infrastructure)

### Auth
- [ ] JWT üretimi yok (`AuthService.LoginAsync` → `NotImplementedException`)
- [ ] Refresh token saklama stratejisi belirlenmedi (DB mi, Redis mi?)
- [ ] E-posta doğrulama token'ı yok
- [ ] `SubjectRegisteredConsumer`: DB'ye Subject yazma ve cüzdan oluşturma logic'i yok

### Application Servisleri (hepsi NotImplementedException)
- [ ] `SubjectService` — tüm metotlar
- [ ] `AuthService` — tüm metotlar
- [ ] `SurveyService` — tüm metotlar
- [ ] `WalletService` — tüm metotlar
- [ ] `NotificationService` — tüm metotlar
- [ ] `StoryService` — tüm metotlar

### API Controller'ları
- [ ] `AuthController` — register/login/refresh/logout gerçek implementasyon yok
- [ ] `SurveyController` — gerçek implementasyon yok
- [ ] `ProfileController` — gerçek implementasyon yok
- [ ] `WalletController` — gerçek implementasyon yok
- [ ] `WebhookController` — gerçek implementasyon yok
- [ ] `StoryController` — gerçek implementasyon yok
- [ ] `NotificationController` — gerçek implementasyon yok

### Infrastructure
- [ ] EF Core migration yok (hiç migration oluşturulmadı)
- [ ] `SurveyRepository` implementasyonu iskelet
- [ ] `RabbitMqPublisher` implementasyonu iskelet

### Worker Consumer'ları (hepsi TODO)
- [ ] `SubjectRegisteredConsumer` — DB yazma, cüzdan oluşturma, hoşgeldin maili
- [ ] `SurveyCompletedConsumer` — kazanç hesaplama, `wallet.credit` kuyruğuna atma
- [ ] `NotificationSendConsumer` — FCM push gönderimi
- [ ] `WithdrawalRequestedConsumer` — admin onay akışı
- [ ] `WalletCreditConsumer` — cüzdana yazma

---

## Admin (PulsePoll.Admin)
- [ ] Auth yok (AdminUser login sistemi belirlenmedi)
- [ ] Tüm Blazor sayfaları iskelet (Dashboard, Users, Surveys, Stories, Wallet)
- [ ] DevExpress bileşenleri entegre edilmedi

---

## Genel
- [ ] EF Core migration'ları oluşturulacak
- [ ] `appsettings.json` değerleri doldurulacak (connection string'ler, JWT secret, MinIO, FCM vs.)
- [ ] `docker-compose.yml` test edilmedi
