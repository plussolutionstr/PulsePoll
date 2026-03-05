# PulsePoll — Kapsamlı Kod Denetim Raporu

**Tarih:** 2026-03-05
**Denetlenen projeler:** Api, Admin, Worker, Application, Domain, Infrastructure, Mobile

---

## 🔴 KRİTİK BULGULAR

### KRİTİK-1: appsettings.json'da Hardcoded Secret'lar (Git'e Commit Edilmiş)

**Dosyalar:** `Api/appsettings.json`, `Admin/appsettings.json`, `Worker/appsettings.json`
**Sorun:** PostgreSQL parolası (`Dev123456!`), MinIO SecretKey, JWT SecretKey placeholder, RabbitMQ guest/guest, SMTP parolası — hepsi plaintext ve Git reposunda.
**Etki:** Repo'ya erişen herkes tüm altyapı credential'larını görebilir.
**Öneri:**
- `dotnet user-secrets` ile Development secret'larını taşı
- Production için Azure Key Vault / AWS Secrets Manager kullan
- `appsettings.Development.json`'ı `.gitignore`'a ekle
- Git geçmişinden `BFG Repo-Cleaner` ile mevcut secret'ları temizle

---

### KRİTİK-2: IBAN Plaintext Saklanması (Şifreleme Yok)

**Dosyalar:** `Domain/Entities/BankAccount.cs`, `Application/Services/WalletService.cs:413,436`, `Worker/Consumers/UserRegisteredConsumer.cs:90`
**Sorun:** `IbanEncrypted` alanı adına rağmen IBAN düzmetin olarak kaydediliyor. Projede hiçbir Encrypt/Decrypt/AES/DataProtection mekanizması yok.
**Etki:** DB sızıntısında tüm IBAN'lar açık.
**Öneri:** ASP.NET Core Data Protection veya AES-256 ile şifreleme katmanı ekle.

---

### KRİTİK-3: Admin API Endpoint'lerinde `[Authorize]` Eksik

**Dosyalar:**
- `Api/Controllers/Admin/WithdrawalController.cs:11` — `// TODO: [Authorize(Roles = "Admin")]`
- `Api/Controllers/CustomerController.cs:9` — tüm CRUD açık
- `Api/Controllers/NewsController.cs:22-23` — Create/Update/Delete açık
- `Api/Controllers/StoryController.cs:32-33` — Create/Update/Delete açık

**Sorun:** TODO yorumu var ama attribute eklenmemiş. Endpoint'ler herkese açık.
**Etki:** Yetkisiz kullanıcılar müşteri CRUD, çekim onay/red, haber/story yönetimi yapabilir.
**Öneri:** Hemen `[Authorize]` + uygun policy ekle.

---

### KRİTİK-4: CORS "Dev" Policy Production'da Açık

**Dosya:** `Api/Program.cs:52`
**Sorun:** `app.UseCors("Dev")` — `AllowAnyOrigin + AllowAnyMethod + AllowAnyHeader` her environment'ta aktif.
**Etki:** Production'da herhangi bir origin API'ye istek atabilir.
**Öneri:** `if (app.Environment.IsDevelopment())` kontrolü ekle, production için kısıtlı policy tanımla.

---

## 🟠 YÜKSEK BULGULAR

### YÜKSEK-1: Webhook Endpoint'inde Kimlik Doğrulaması Yok

**Dosya:** `Api/Controllers/WebhookController.cs`
**Sorun:** `POST /api/webhook/survey-complete` — ne `[Authorize]` ne HMAC/signature doğrulaması var. Rate limiting (30/dk) tek koruma.
**Öneri:** HMAC signature veya `X-Webhook-Secret` header kontrolü ekle.

---

### YÜKSEK-2: Admin Blazor Sayfalarında Doğrudan DbContext Kullanımı (Katman İhlali)

**Dosyalar:** `Subjects.razor`, `ProjectSubjects.razor`, `NotificationHistory.razor`, `WithdrawalRequests.razor`, `Banks.razor`
**Sorun:** Admin sayfaları `IDbContextFactory<AppDbContext>` ile doğrudan EF Core sorgusu yapıyor → Presentation → Infrastructure doğrudan bağımlılığı.
**Öneri:** `IQueryable<T>` döndüren repository/servis metotları oluştur, Razor'dan DbContext'i kaldır.

---

### YÜKSEK-3: WalletService vs PaymentBatchService — Duplicate Withdrawal Logic

**Dosyalar:** `WalletService.cs:232-322`, `PaymentBatchService.cs:34-80`
**Sorun:** Her ikisi de `ApproveWithdrawalAsync` / `RejectWithdrawalAsync` implementasyonu taşıyor. Farklı davranış riski var.
**Öneri:** Tek bir yere (`WalletService`) konsolide et, `PaymentBatchService` delege etsin.

---

## 🟡 ORTA BULGULAR

| # | Kategori | Dosya | Sorun |
|---|----------|-------|-------|
| 1 | Kod Kalitesi | 6 servis dosyası | `PresignedUrlExpirySeconds` sabiti 6 farklı yerde tekrar → `StorageDefaults.cs`'ye taşı |
| 2 | Kod Kalitesi | `NewsService.cs`, `StoryService.cs` | `NormalizeToTurkeyLocal` metodu birebir tekrar → shared helper'a taşı |
| 3 | Kod Kalitesi | 5 servis dosyası | `"media-library"` bucket adı farklı değişken adlarıyla tekrar → `BucketNames.cs`'ye taşı |
| 4 | Kod Kalitesi | `WalletService.cs` | God class: 568 satır, 14 public metot, 13 dependency → BankAccountService, WithdrawalService olarak ayır |
| 5 | Kod Kalitesi | `ProjectSubjects.razor` | 1127 satır, 30+ field → alt componentlere böl |
| 6 | Performans | `SpecialDayCalendarService.cs:161` | 12 ay seri HTTP çağrısı → `Task.WhenAll` ile paralelleştir (DbContext yok, güvenli) |
| 7 | Performans | `Mobile/HomeViewModel.cs:101,106,107` | `.Result` kullanımı → `await` ile değiştir |
| 8 | Performans | `Mobile/RegisterViewModel.cs:277` | `new HttpClient()` → `IHttpClientFactory` kullan |
| 9 | Performans | `SpecialDayCalendarService.cs:16` | Static `HttpClient` → `IHttpClientFactory` ile DI'dan al |
| 10 | Hata Yönetimi | `Worker/CommunicationAutomationJobScheduler.cs:55` | `catch(Exception)` çok geniş, log yok → `catch(TimeZoneNotFoundException)` + warning log |
| 11 | Config | Api `appsettings.json` | `RabbitMQ.VirtualHost` eksik (Admin/Worker'da var) |
| 12 | Config | Admin `appsettings.json` | JWT bölümü var ama Admin cookie auth kullanıyor → kafa karıştırıcı |
| 13 | Config | Worker | `appsettings.Development.json` dosyası yok |
| 14 | NuGet | `Api.csproj` | `FluentValidation.AspNetCore` deprecated → kaldır |
| 15 | Test | Proje geneli | Sadece 2 test dosyası (WalletService: 16 test, SubjectService: 1 test) — çok düşük coverage |
| 16 | Duplicate | `Subjects.razor`, `ProjectSubjects.razor` | BankAccount yükleme kodu birebir aynı |
| 17 | TODO | `UserRegisteredConsumer.cs:37` | `// TODO: LSM hesaplaması — formül gelince eklenecek` |

---

## 🔵 DÜŞÜK BULGULAR

| # | Kategori | Dosya | Sorun |
|---|----------|-------|-------|
| 1 | Kod Kalitesi | `DbSeeder.cs` | 1617 satır god class → seed verilerini JSON'a veya ayrı seeder'lara böl |
| 2 | Kod Kalitesi | 5 Razor sayfası | `EnumOpt<T>` / `EnumOption<T>` record tekrarı → shared helper'a taşı |
| 3 | Config | `.gitignore` | `.idea/` duplicate (satır 4 ve 8), `publish/` klasörü eksik |
| 4 | Test | `UserServiceTests.cs` | Dosya adı eski ("User"), içi "SubjectServiceTests" → adını düzelt |
| 5 | NuGet | `Directory.Packages.props` | `Microsoft.Data.Sqlite.Core` + `SQLitePCLRaw.bundle_green` tanımlı ama kullanılmıyor |

---

## 🟣 TÜRKÇE KARAKTER BULGULARI (52 Adet)

### En Kritik — Hukuki Metin

📁 `src/PulsePoll.Mobile/Views/RegisterPage.xaml:369`
❌ `"KVKK metnini okudum ve onayliyorum."`
✅ `"KVKK metnini okudum ve onaylıyorum."`

### Mobil — Çıkış Dialog'u

📁 `src/PulsePoll.Mobile/ViewModels/ProfileViewModel.cs:331`
❌ `"Cikis Yap", "Hesabinizdan cikis yapmak istediginize emin misiniz?", "Cikis Yap", "Iptal"`
✅ `"Çıkış Yap", "Hesabınızdan çıkış yapmak istediğinize emin misiniz?", "Çıkış Yap", "İptal"`

### Admin — PaymentBatches.razor

📁 `src/PulsePoll.Admin/Components/Pages/Payments/PaymentBatches.razor`

| Satır | Mevcut | Önerilen |
|-------|--------|----------|
| 25 | `Text="Yeni Paket Olustur"` | `Text="Yeni Paket Oluştur"` |
| 38 | `Text="Yukleniyor..."` | `Text="Yükleniyor..."` |
| 56 | `Caption="Olusturulma"` | `Caption="Oluşturulma"` |
| 57 | `Caption="Gonderilme"` | `Caption="Gönderilme"` |
| 59 | `Caption="Islemler"` | `Caption="İşlemler"` |
| 87 | `HeaderText="Yeni Odeme Paketi"` | `HeaderText="Yeni Ödeme Paketi"` |
| 142 | `Text="Paket Olustur"` | `Text="Paket Oluştur"` |
| 147 | `Text="Iptal"` | `Text="İptal"` |
| 237 | `"Paket olusturuldu: ..."` | `"Paket oluşturuldu: ..."` |

### Admin — PaymentBatchDetail.razor (22 bulgu)

📁 `src/PulsePoll.Admin/Components/Pages/Payments/PaymentBatchDetail.razor`

| Satır | Mevcut | Önerilen |
|-------|--------|----------|
| 19 | `"Yukleniyor..."` | `"Yükleniyor..."` |
| 66 | `Text="Gonderildi Olarak Isaretlendi"` | `Text="Gönderildi Olarak İşaretlendi"` |
| 74 | `Text="Tumunu Sec"` | `Text="Tümünü Seç"` |
| 80 | `Text="Secimi Temizle"` | `Text="Seçimi Temizle"` |
| 91 | `Text="Secilenleri Odendi Yap"` | `Text="Seçilenleri Ödendi Yap"` |
| 97 | `Text="Secilenleri Hata Yap"` | `Text="Seçilenleri Hata Yap"` |
| 110 | `Text="Yukleniyor..."` | `Text="Yükleniyor..."` |
| 144 | `Caption="Islem Tarihi"` | `Caption="İşlem Tarihi"` |
| 147 | `Caption="Guncelle"` | `Caption="Güncelle"` |
| 155 | `Text="Odendi"` | `Text="Ödendi"` |
| 194 | `Text="Iptal"` | `Text="İptal"` |
| 257 | `"Paket 'Gonderildi' olarak isaretlendi."` | `"Paket 'Gönderildi' olarak işaretlendi."` |
| 277 | `"Paket tamamlandi."` | `"Paket tamamlandı."` |
| 298 | `"Guncellenecek odeme bulunamadi."` | `"Güncellenecek ödeme bulunamadı."` |
| 326 | `"Secili odemeler guncellenemedi."` | `"Seçili ödemeler güncellenemedi."` |
| 330 | `"odendi"` / `"hatali"` | `"ödendi"` / `"hatalı"` |
| 331 | `"... odeme ... olarak guncellendi."` | `"... ödeme ... olarak güncellendi."` |
| 349 | `"1 odeme secildi: ..."` | `"1 ödeme seçildi: ..."` |
| 364 | `"Hata olarak isaretlenecek odeme yok."` | `"Hata olarak işaretlenecek ödeme yok."` |
| 379 | `"Lutfen bekleyen odemeleri secin."` | `"Lütfen bekleyen ödemeleri seçin."` |
| 391 | `"Lutfen bekleyen odemeleri secin."` | `"Lütfen bekleyen ödemeleri seçin."` |
| 396 | `"... odeme secildi."` | `"... ödeme seçildi."` |

### Admin — WithdrawalRequests.razor (15 bulgu)

📁 `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor`

| Satır | Mevcut | Önerilen |
|-------|--------|----------|
| 35 | `Text="Tumunu Sec"` | `Text="Tümünü Seç"` |
| 42 | `Text="Secimi Temizle"` | `Text="Seçimi Temizle"` |
| 51 | `Text="Secilenleri Onayla"` | `Text="Seçilenleri Onayla"` |
| 61 | `Text="Secilenleri Reddet"` | `Text="Seçilenleri Reddet"` |
| 126 | `Caption="Islemler"` | `Caption="İşlemler"` |
| 168 | `Caption="Secili Talep"` | `Caption="Seçili Talep"` |
| 181 | `Text="Iptal"` | `Text="İptal"` |
| 272 | `"Talep onaylandi."` | `"Talep onaylandı."` |
| 308 | `"Lutfen en az bir bekleyen talep secin."` | `"Lütfen en az bir bekleyen talep seçin."` |
| 331 | `"... talep onaylandi."` | `"... talep onaylandı."` |
| 333 | `"Secili talepler onaylanamadi."` | `"Seçili talepler onaylanamadı."` |
| 355 | `"Lutfen en az bir bekleyen talep secin."` | `"Lütfen en az bir bekleyen talep seçin."` |
| 361 | `"... talep secildi."` | `"... talep seçildi."` |
| 384 | `"Reddedilecek talep bulunamadi."` | `"Reddedilecek talep bulunamadı."` |
| 410 | `"Secili talepler reddedilemedi."` | `"Seçili talepler reddedilemedi."` |

---

## PROJE YAPISI VE KONFİGÜRASYON

### NuGet Paket Sorunları

| Sorun | Dosya | Detay |
|-------|-------|-------|
| FluentValidation.AspNetCore deprecated | `Api.csproj` | Maintainer tarafından deprecated. `FluentValidation.DependencyInjectionExtensions`'a geç. |
| Microsoft.AspNetCore.Identity gereksiz ağır | `Infrastructure.csproj` | Sadece `PasswordHasher<string>` için kullanılıyor. `Microsoft.Extensions.Identity.Core` yeterli. |
| Kullanılmayan paketler | `Directory.Packages.props` | `Microsoft.Data.Sqlite.Core` + `SQLitePCLRaw.bundle_green` tanımlı ama referans yok. |
| Worker'da MassTransit.RabbitMQ duplikasyonu | `Worker.csproj` | Infrastructure zaten taşıyor, Worker project reference ile alıyor — gereksiz PackageReference. |

### .gitignore Eksikleri

- `appsettings.Development.json` gitignore'da değil (secret'lar commit ediliyor)
- Firebase credential dosyaları için kural yok (`firebase-adminsdk*.json`)
- `.idea/` duplicate tanım (satır 4 ve 8)
- `publish/` ve `artifacts/` klasörleri eksik

### Program.cs Notları

- **Admin/Program.cs:** Login endpoint inline tanımlı (140+ satır) → ayrı dosyaya çıkarılabilir
- **Worker:** `appsettings.Development.json` yok, DB migration çalıştırmıyor (Api'ye bağımlı)
- **Api:** Swagger production'da kapalı (iyi), migration + seed startup'ta çalışıyor (kabul edilebilir)

### Test Durumu

- **WalletServiceTests.cs:** 16 test (iyi yazılmış)
- **UserServiceTests.cs:** 1 gerçek test, 1 TODO — dosya adı eski ("User" → "Subject")
- **Eksik:** AuthService, ProjectService, StoryService, MediaAssetService, PaymentBatchService testleri yok
- **Integration test yok:** API endpoint testleri, consumer testleri, DB integration testleri hiç yok

---

## ÖZET TABLOSU

| # | Seviye | Kategori | Kısa Açıklama |
|---|--------|----------|---------------|
| 1 | 🔴 | Güvenlik | appsettings.json'da hardcoded secret'lar (Git'te) |
| 2 | 🔴 | Güvenlik | IBAN plaintext saklanması |
| 3 | 🔴 | Güvenlik | 4 controller'da `[Authorize]` eksik |
| 4 | 🔴 | Güvenlik | CORS Dev policy production'da açık |
| 5 | 🟠 | Güvenlik | Webhook endpoint'inde signature doğrulama yok |
| 6 | 🟠 | Mimari | Admin Blazor → doğrudan DbContext (katman ihlali) |
| 7 | 🟠 | Mimari | Duplicate withdrawal approve/reject logic |
| 8 | 🟡 | Kod Kalitesi | Sabit/helper tekrarları (6 bulgu) |
| 9 | 🟡 | Kod Kalitesi | God class: WalletService (568 satır) |
| 10 | 🟡 | Performans | Static HttpClient, .Result kullanımı, seri HTTP çağrıları |
| 11 | 🟡 | Config | Tutarsız appsettings, deprecated paket, eksik gitignore |
| 12 | 🟡 | Test | Çok düşük test coverage |
| 13 | 🟣 | Türkçe | 52 bozuk karakter (ödeme modülü yoğun) |

---

## AKSİYON PLANI (Öncelik Sırası)

### Sprint 1 — Acil (Güvenlik)

1. **Secret'ları taşı** — `dotnet user-secrets` + environment variable + `.gitignore` güncelle
2. **`[Authorize]` ekle** — WithdrawalController, CustomerController, NewsController, StoryController
3. **CORS düzelt** — production için kısıtlı policy
4. **Webhook'a HMAC ekle**

### Sprint 2 — Yüksek

5. **IBAN şifreleme** — Data Protection veya AES-256
6. **Türkçe karakter düzeltmeleri** — 52 bulgu (özellikle KVKK metni öncelikli)
7. **Katman ihlali** — Admin sayfalarından DbContext kaldır

### Sprint 3 — İyileştirme

8. **Withdrawal logic konsolidasyonu**
9. **Duplicate sabit/helper temizliği**
10. **WalletService refactor**
11. **Test coverage artır** (AuthService, ProjectService, PaymentBatchService)
