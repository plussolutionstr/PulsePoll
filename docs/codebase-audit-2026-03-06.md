# PulsePoll Kapsamli Kod Denetimi

**Tarih:** 6 Mart 2026  
**Kapsam:** `src/`, `tests/`, proje konfigurasyon dosyalari  
**Notlar:**
- `survey-result` endpointinin public olmasi is gereksinimi olarak kabul edildi.
- `docs/demo-data/seed-data.md` kapsam disi tutuldu (projeyle baglantisiz).
- `appsettings`/sunucu `.env` yonetimi kullanici talebine gore kapsam disi tutuldu.

---

### 🔴 Güvenlik — Anonim Medya Erişimi (Profil Fotoğrafları Dahil)

**Dosya:** `src/PulsePoll.Api/Controllers/MediaController.cs`, `src/PulsePoll.Admin/Program.cs`  
**Satır:** `MediaController: 13, 16-19`; `Admin Program: 203-217`  
**Sorun:** API tarafinda `profile-photos` bucket'i anonim okunabiliyor. Admin proxy endpoint'i de anonim ve bucket bazli ek kisit yok.  
**Etki:** Object key tahmini ile ozel medya varliklarinin sizdirilmasi riski.  
**Öneri:** `profile-photos` icin yetkilendirme zorunlu yapin; signed URL veya scoped proxy modeli kullanin; admin proxy'de bucket allowlist + permission kontrolu ekleyin.

### 🟠 Güvenlik — Forwarded Header Trust-Model ve Login CSRF Riski

**Dosya:** `src/PulsePoll.Admin/Program.cs`  
**Satır:** `111-116, 193`  
**Sorun:** `KnownIPNetworks/KnownProxies` temizlenmis (genis trust), login endpoint'te `DisableAntiforgery()` kullaniliyor.  
**Etki:** Reverse proxy arkasinda hatali guven modeli ile IP spoofing/rate-limit bypass ve login CSRF riski artar.  
**Öneri:** Sadece bilinen proxy IP'lerini trust edin; login'de antiforgery veya origin/samesite tabanli ek koruma uygulayin.

### 🟠 Derleme — Mobile (MacCatalyst) Build Kırığı

**Dosya:** `src/PulsePoll.Mobile/Helpers/ImageResizer.cs`  
**Satır:** `5`  
**Sorun:** `public static partial` methodun MacCatalyst hedefinde implementasyonu yok (`CS8795`).  
**Etki:** `net10.0-maccatalyst` derlemesi fail oluyor, yayin hattini bloke ediyor.  
**Öneri:** MacCatalyst icin `ImageResizer` partial implementasyonu ekleyin veya target-conditional ortak fallback yazin.

### 🟠 Test — Test Projesi Derlenmiyor

**Dosya:** `tests/PulsePoll.Tests/UserServiceTests.cs`  
**Satır:** `25-32`  
**Sorun:** `SubjectService` ctor imzasi guncellenmis, testte eksik bagimliliklar var (`ICacheService`, `IRefreshTokenRepository`).  
**Etki:** `dotnet test` calismiyor; regresyon yakalama zayifliyor.  
**Öneri:** Test fixture'ini guncel constructor'a uyarlayin, eksik mock'lari ekleyin.

### 🟠 Veri Tutarlılığı — Cüzdan Akışlarında Atomiklik Eksik

**Dosya:** `src/PulsePoll.Application/Services/WalletService.cs`  
**Satır:** `137-191, 257-317`  
**Sorun:** Bakiye guncelleme, hareket yazma ve bildirim adimlari tek transaction siniri icinde degil.  
**Etki:** Ara adimlarda hata durumunda finansal tutarsizlik olusabilir.  
**Öneri:** Wallet + transaction + request durum degisikliklerini tek DB transaction kapsaminda yonetin.

### 🟠 Performans — N+1 Sorgu ve Fazla SaveChanges

**Dosya:** `src/PulsePoll.Application/Services/MessageAutomationService.cs`, `src/PulsePoll.Infrastructure/Persistence/Repositories/MessageAutomationRepository.cs`  
**Satır:** `Service: 139-155, 192-199, 247-254`; `Repository: 71-82`  
**Sorun:** Her denek/kanal icin `DispatchLogExists` sorgusu ve her log insertinde ayri `SaveChangesAsync`.  
**Etki:** Kampanya hacmi buyudugunde ciddi DB round-trip ve throughput kaybi.  
**Öneri:** Mevcut loglari toplu cekip bellekte set ile kontrol edin; log insertlerini batchleyip tek `SaveChangesAsync` yapin.

### 🟠 Performans — Gereksiz Büyük Veri Çekimi

**Dosya:** `src/PulsePoll.Application/Services/WalletService.cs`, `src/PulsePoll.Application/Services/SubjectService.cs`  
**Satır:** `WalletService: 465-467`; `SubjectService: 154-156`  
**Sorun:** Ledger icin tum hareketler bellege alinip hesap yapiliyor; toplu SMS icin tum denekler cekilip sonra filtreleniyor.  
**Etki:** Buyuk veri setlerinde bellek ve gecikme maliyeti artar.  
**Öneri:** Ledger icin SQL tarafinda running-balance stratejisi kullanin; SMS hedeflerini `WHERE IN` ile dogrudan sorgulayin.

### 🟠 Mimari — Presentation Katmanında Doğrudan DbContext Kullanımı

**Dosya:** `src/PulsePoll.Admin/Components/Pages/Subjects/Subjects.razor`, `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor`, `src/PulsePoll.Admin/Components/Pages/Settings/Banks.razor`, `src/PulsePoll.Admin/Components/Pages/Projects/ProjectSubjects.razor`, `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor`  
**Satır:** `Subjects: 7, 566-591`; `WithdrawalRequests: 5, 218-234, 480-513`; `Banks: 5, 209-213, 318-327, 347-377`; `ProjectSubjects: 6, 588-610`; `NotificationHistory: 5, 203-215`  
**Sorun:** UI komponentleri uygulama servis katmanini bypass ederek sorgu/guncelleme yapiyor.  
**Etki:** Is kurali daginikligi, tekrar, test edilebilirlikte dusus ve bakim maliyeti artisi.  
**Öneri:** Query/command akislarini Application katmanina tasiyin; Razor tarafini orchestration seviyesinde birakin.

### 🟡 Hata Yönetimi — Sessiz Catch ve Hata Yutma

**Dosya:** `src/PulsePoll.Mobile/ViewModels/NotificationsViewModel.cs`, `src/PulsePoll.Mobile/ViewModels/RegisterViewModel.cs`, `src/PulsePoll.Application/Services/SubjectService.cs`, `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor`  
**Satır:** `202`; `307`; `99, 177`; `324-327, 401-404`  
**Sorun:** Bazi hata durumlari loglanmadan yutuluyor.  
**Etki:** Uretimde problem tespiti zorlasir; sessiz veri/islem kaybi gorulebilir.  
**Öneri:** En az warning/error seviyesinde loglayin; toplu islemlerde hata sayisi ve nedenlerini raporlayin.

### 🟡 Güvenlik/Operasyon — Swagger Ortam Ayrımı Olmadan Açık

**Dosya:** `src/PulsePoll.Api/Program.cs`  
**Satır:** `37-38`  
**Sorun:** Swagger ve SwaggerUI kosulsuz aciliyor.  
**Etki:** Endpoint kesfi ve saldiri yuzeyi production ortaminda da genisler.  
**Öneri:** `Development` ile sinirlayin veya yetkili erisim arkasina alin.

### 🟡 Kod Kalitesi — TODO ve İş Mesajı Tutarsızlığı

**Dosya:** `src/PulsePoll.Worker/Consumers/UserRegisteredConsumer.cs`, `tests/PulsePoll.Tests/UserServiceTests.cs`  
**Satır:** `Worker: 37, 68, 106-109`; `Test: 38`  
**Sorun:** LSM hesaplamasi TODO durumda; test TODO durumda; kullanici `Approved` atanirken gonderilen e-posta metni "incelemeye alindi" diyor.  
**Etki:** Is kurali/metin tutarsizligi ve test kapsami boslugu.  
**Öneri:** LSM hesaplamasini tamamlayin; e-posta metnini gercek durumla esitleyin; TODO testleri gercek assertion'a cevirin.

### 🟡 Bağımlılıklar — Eski Sürümler ve Kullanılmayan Paket Referansı

**Dosya:** `Directory.Packages.props`, `src/PulsePoll.Api/PulsePoll.Api.csproj`  
**Satır:** `props genel`; `Api.csproj: 20`  
**Sorun:** Bircok paket outdated (`dotnet list package --outdated`); `FluentValidation.AspNetCore` referansi var ancak kodda kullanim izi yok.  
**Etki:** Surum borcu birikir, uyumluluk ve guvenlik bakim riski artar.  
**Öneri:** Paketleri kademeli guncelleyin; kullanilmayan package referanslarini kaldirin.

### 🔵 Kod Kalitesi — İsimlendirme ve Dokümantasyon Boşluğu

**Dosya:** `tests/PulsePoll.Tests/UserServiceTests.cs`, repo root  
**Satır:** `12`; `README yok`  
**Sorun:** Dosya adi `UserServiceTests`, sinif adi `SubjectServiceTests`; kok dizinde README bulunmuyor.  
**Etki:** Onboarding ve kod kesfi zorlasir.  
**Öneri:** Dosya/sinif adlarini hizalayin; kurulum-calisma-test adimlarini iceren kok README ekleyin.

### 🟣 Türkçe — ASCII Kullanımı (Kullanıcıya Görünen Dosya Adı)

**Dosya:** `src/PulsePoll.Admin/Components/Pages/Customers/Customers.razor`  
**Satır:** `284`  
**Sorun:** Export dosya adinda bozuk Turkce karakter kullanimi.
**Etki:** Kullaniciya gorunen metin kalitesi dusuyor.  
**Öneri:**

📁 Dosya: `src/PulsePoll.Admin/Components/Pages/Customers/Customers.razor`  
📍 Satır: `284`  
❌ Mevcut: `"musteriler"`  
✅ Önerilen: `"müşteriler"`

---

## Özet Tablo

| # | Seviye | Kategori | Dosya | Kısa Açıklama |
|---|---|---|---|---|
| 1 | 🔴 | Güvenlik | `MediaController.cs`, `Admin/Program.cs` | Anonim medya erisimi |
| 2 | 🟠 | Güvenlik | `Admin/Program.cs` | Forwarded header trust + antiforgery kapali login |
| 3 | 🟠 | Derleme | `Mobile/Helpers/ImageResizer.cs` | MacCatalyst build kirik |
| 4 | 🟠 | Test | `tests/UserServiceTests.cs` | Test projesi derlenmiyor |
| 5 | 🟠 | Veri Tutarlılığı | `WalletService.cs` | Atomik olmayan finansal akis |
| 6 | 🟠 | Performans | `MessageAutomationService.cs` | N+1 sorgu ve fazla SaveChanges |
| 7 | 🟠 | Performans | `WalletService.cs`, `SubjectService.cs` | Gereksiz buyuk veri cekimi |
| 8 | 🟠 | Mimari | Admin Razor sayfalari | UI katmanindan dogrudan DB erisimi |
| 9 | 🟡 | Hata Yönetimi | Coklu dosya | Sessiz catch / hata yutma |
| 10 | 🟡 | Güvenlik/Operasyon | `Api/Program.cs` | Swagger kosulsuz acik |
| 11 | 🟡 | Kod Kalitesi | `UserRegisteredConsumer.cs`, `tests/...` | TODO ve is mesaji tutarsizligi |
| 12 | 🟡 | Bağımlılık | `Directory.Packages.props` | Outdated paketler, atil referans |
| 13 | 🔵 | Kod Kalitesi | `tests/...`, root | Isimlendirme + README eksik |
| 14 | 🟣 | Türkçe | `Customers.razor` | `musteriler` -> `müşteriler` |

---

## Öncelikli Aksiyon Planı (Kısa)

1. **P0 (1-2 gun):** Medya endpoint yetkilendirme/signed-url duzeltmeleri.  
2. **P0 (0.5-1 gun):** Admin proxy/header trust ve login antiforgery sertlestirmesi.  
3. **P1 (0.5-1 gun):** Mobile maccatalyst build fix + test fixture fix.  
4. **P1 (1-2 gun):** Wallet ve odul akislarinda transaction siniri.  
5. **P1 (1-2 gun):** N+1/bulk performans refactor.  
6. **P2 (1-2 gun):** Razor -> Application katmani ayristirmasi.  
7. **P2 (0.5 gun):** Gorunen metinlerde Turkce karakter standardizasyonu.

