# PulsePoll.Admin — Kapsamlı Proje Analizi

> Oluşturulma tarihi: 2026-03-04
> Proje tipi: ASP.NET Core Blazor Server (Interactive Server)
> Target framework: net10.0
> URL: http://localhost:5010 / https://localhost:5011

---

## 1. Proje Dizin Yapısı

```
PulsePoll.Admin/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── PulsePoll.Admin.csproj
├── Properties/
│   └── launchSettings.json
├── Components/
│   ├── _Imports.razor
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   ├── Drawer.razor + .css
│   │   ├── EmptyLayout.razor
│   │   ├── MainLayout.razor + .css
│   │   └── NavMenu.razor + .css
│   ├── Pages/
│   │   ├── Auth/
│   │   │   └── Login.razor + .css
│   │   ├── Communications/
│   │   │   ├── MessageAutomations.razor
│   │   │   └── SpecialDaysCalendar.razor
│   │   ├── Content/
│   │   │   ├── News.razor
│   │   │   └── Stories.razor
│   │   ├── Customers/
│   │   │   └── Customers.razor
│   │   ├── Dashboard/
│   │   │   └── Dashboard.razor + .css
│   │   ├── Error.razor
│   │   ├── MediaLibrary/
│   │   │   └── MediaLibrary.razor
│   │   ├── Notifications/
│   │   │   └── NotificationHistory.razor
│   │   ├── Payments/
│   │   │   ├── PaymentBatchDetail.razor
│   │   │   ├── PaymentBatches.razor
│   │   │   ├── PaymentSettings.razor
│   │   │   └── WithdrawalRequests.razor
│   │   ├── Projects/
│   │   │   ├── Projects.razor
│   │   │   ├── ProjectStatement.razor
│   │   │   └── ProjectSubjects.razor
│   │   ├── Settings/
│   │   │   ├── AdminUsers.razor
│   │   │   ├── AppContentSettings.razor
│   │   │   ├── CommunicationAutomationSettings.razor
│   │   │   ├── ReferralRewardSettings.razor
│   │   │   ├── Roles.razor + .css
│   │   │   └── Settings.razor
│   │   └── Subjects/
│   │       ├── RewardUnitSettings.razor
│   │       ├── SubjectLedger.razor
│   │       ├── SubjectProjects.razor
│   │       ├── SubjectReferrals.razor
│   │       ├── SubjectScoringSettings.razor
│   │       └── Subjects.razor
│   └── Shared/
│       ├── DrawerStateComponentBase.cs
│       ├── MediaPickerPopup.razor
│       └── PermissionChecker.razor
├── Services/
│   ├── AppToastService.cs
│   ├── IAppToastService.cs
│   ├── ErrorHandler.cs
│   ├── IPermissionAuthorizationService.cs
│   └── PermissionAuthorizationService.cs
└── wwwroot/
    ├── favicon.ico
    ├── css/
    │   ├── site.css
    │   ├── icons.css
    │   ├── bootstrap/
    │   └── open-iconic/
    ├── images/
    │   ├── logo.svg, menu.svg, close.svg, back.svg, cards.svg, demos.svg, doc.svg
    │   └── pages/
    └── js/
        ├── auth.js
        └── download.js
```

---

## 2. Bağımlılıklar

### NuGet Paketleri

| Paket | Kullanım Amacı |
|---|---|
| DevExpress.Blazor | UI bileşenleri (Grid, Popup, ComboBox, Toast vb.) |
| Serilog.AspNetCore | Structured logging |

### Proje Referansları

- `PulsePoll.Application` — servisler, DTO'lar, FluentValidation
- `PulsePoll.Infrastructure` — DB context, repository, MinIO, Redis, RabbitMQ

### Infrastructure Üzerinden Gelen

- **MassTransit** (RabbitMQ publish-only, consumer yok)
- **Microsoft.AspNetCore.Authentication.Cookies**
- **Microsoft.AspNetCore.RateLimiting**
- **Serilog + Seq** (structured logging)
- **FluentValidation** (Application katmanından)

---

## 3. Program.cs — Servis Kaydı ve Middleware

### Servis Kaydı

```
AddRazorComponents + AddInteractiveServerComponents
AddDevExpressBlazor(SizeMode.Medium)
AddMvc()
AddApplication()                → FluentValidation validators
AddInfrastructure(config)       → DB, Redis, MinIO, JWT
AddMassTransit → RabbitMQ (publish-only, consumer yok)
AddScoped<IAdminAuthService, AdminAuthService>
AddScoped<IAppToastService, AppToastService>
AddAuthentication → Cookie ("PulsePoll.Admin.Auth")
AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>
AddRateLimiter → "admin-login" policy
AddCascadingAuthenticationState()
AddHttpContextAccessor()
```

### Cookie Auth Ayarları

| Parametre | Değer |
|---|---|
| Cookie Name | PulsePoll.Admin.Auth |
| LoginPath | /login |
| LogoutPath | /logout |
| AccessDeniedPath | /access-denied |
| ExpireTimeSpan | 8 saat |
| SlidingExpiration | true |
| HttpOnly | true |
| SameSite | Lax |

### Rate Limiting

- **Policy adı**: `admin-login`
- **Tip**: Fixed Window
- **Limit**: 5 istek / 5 dakika
- **Partition**: IP bazlı (X-Forwarded-For destekli)

### Middleware Pipeline

```
UseExceptionHandler("/Error")    — sadece production
UseHsts()                        — sadece production
UseHttpsRedirection()
UseAuthentication()
UseAuthorization()
UseRateLimiter()
UseAntiforgery()
MapStaticAssets()
POST /api/auth/login             → cookie sign-in (rate-limited, antiforgery disabled)
GET  /logout                     → cookie sign-out → redirect /login
MapRazorComponents<App>()        → InteractiveServer render mode
```

### Startup İşlemleri

1. `PermissionSeeder.SeedAsync()` → SuperAdmin rol + ilk admin kullanıcı (admin@pulsepoll.com / Admin123!)
2. `IAdminPermissionCacheService.RefreshAsync()` → Redis'e permission ön yükleme

---

## 4. Authentication & Authorization Sistemi

### Giriş Akışı

1. **Login.razor** (`/login`, `[AllowAnonymous]`, `EmptyLayout` layout kullanır)
2. Kullanıcı form submit → `adminAuth.login()` JS fonksiyonu çağrılır
3. JS, `/api/auth/login` endpoint'ine POST yapar
4. Backend: `IAdminAuthService.ValidateCredentialsAsync()` → şifre doğrulama
5. `IAdminAuthService.GetPermissionCodesAsync()` → tüm permission kodlarını çekme
6. Claims oluşturma: `NameIdentifier`, `Name`, `Email` + her permission için `"perm"` claim
7. `HttpContext.SignInAsync()` ile cookie yazma
8. RememberMe = false → 8 saat, RememberMe = true → 30 gün

### Permission Kontrol Katmanları

```
PermissionChecker.razor (Routes.razor içinde RouteView'ı sarar)
  ├── [AllowAnonymous] → doğrudan izin ver
  ├── Authenticate değilse → /login?returnUrl=... yönlendir
  ├── [HasPermission(codes...)] → IPermissionAuthorizationService.HasAnyPermissionAsync()
  └── Yetkisiz ise → inline "Erişim Reddedildi" UI
```

### Permission Kaynağı (İki Katmanlı)

| Öncelik | Kaynak | Açıklama |
|---|---|---|
| Primary | Redis cache | `IAdminPermissionCacheService.GetPermissionCodesAsync(adminUserId)` |
| Fallback | Cookie claims | `"perm"` claim'lerinden okuma |

### NavMenu Permission Filtresi

- `NavigationManager.LocationChanged` event'ine abone
- Her sayfa değişiminde `GetCurrentPermissionsAsync()` çağrılır
- `_menuRenderKey` güncellenerek menü zorla yeniden render edilir
- Her menü öğesi `Has(PermissionCodes.X.View)` ile koşullu render

### Çıkış

- `GET /logout` → `HttpContext.SignOutAsync()` → redirect `/login`

---

## 5. Sayfa ve Route Haritası

### Auth

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/login` | Auth/Login.razor | [AllowAnonymous] | Giriş formu; JS fetch ile cookie auth |

### Ana Sayfalar

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/` | Dashboard/Dashboard.razor | Dashboard.View | KPI kartları: Denekler, Projeler, Müşteriler, Çekim Talepleri, Ödeme Paketleri |
| `/customers` | Customers/Customers.razor | Customers.View | Müşteri listesi/CRUD; logo upload; il/ilçe/vergi dairesi cascade dropdown |

### Projeler

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/projects` | Projects/Projects.razor | Projects.View | Proje listesi/CRUD; status badge; cover image media picker; proje kopyalama |
| `/projects/{id}/subjects` | Projects/ProjectSubjects.razor | Projects.ManageSubjects | Denek atama; filtre paneli; proje aç/kapat; assignment yönetimi |
| `/projects/{id}/statement` | Projects/ProjectStatement.razor | Projects.StatementView | Proje ekstresi: fatura hesabı, KDV, maliyet, karlılık |

### Denekler

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/subjects` | Subjects/Subjects.razor | Subjects.View | Denek listesi; filtre; SMS/Push toplu gönderim; Excel export |
| `/subjects/{id}/ledger` | Subjects/SubjectLedger.razor | Subjects.LedgerView | Hesap ekstresi; bakiye; kredi/çekim geçmişi |
| `/subjects/{id}/projects` | Subjects/SubjectProjects.razor | Subjects.ProjectsView | Denek proje geçmişi |
| `/subjects/{id}/referrals` | Subjects/SubjectReferrals.razor | Subjects.ReferralsView | Referral geçmişi; komisyon özeti |
| `/subjects/scoring-settings` | Subjects/SubjectScoringSettings.razor | Subjects.ScoringSettings | Skor puanı konfigürasyonu; "Tüm Skorları Yeniden Hesapla" butonu |
| `/subjects/reward-unit-settings` | Subjects/RewardUnitSettings.razor | Subjects.RewardUnitSettings | Ödül birimi (kod, etiket, TL çarpanı) |

### Ödemeler

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/payments/withdrawals` | Payments/WithdrawalRequests.razor | Payments.WithdrawalsView | Çekim talepleri; Bekleyen/Onaylanan sekmeleri; onay/red aksiyonları |
| `/payments/batches` | Payments/PaymentBatches.razor | Payments.BatchesView | Ödeme paketleri listesi; yeni paket oluşturma |
| `/payments/batches/{id}` | Payments/PaymentBatchDetail.razor | Payments.BatchesView | Paket detayı; banka dosyası oluşturma/indirme; paketi tamamlama |
| `/payments/settings` | Payments/PaymentSettings.razor | Payments.SettingsView | Banka transfer şablon ayarları (header/detail/trailer) |

### İletişim & Bildirimler

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/communications/calendar` | Communications/SpecialDaysCalendar.razor | Communications.View | Özel gün takvimi; Hangfire zamanlaması |
| `/communications/automations` | Communications/MessageAutomations.razor | Communications.View | Mesaj şablonları + kampanya yönetimi (iki sekmeli) |
| `/notifications` | Notifications/NotificationHistory.razor | Notifications.View | SMS + Push bildirim geçmişi (sekmeli) |

### İçerik

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/stories` | Content/Stories.razor | Stories.View | Story listesi; sürükle-bırak sıralama; CRUD |
| `/news` | Content/News.razor | News.View | Haber listesi; sürükle-bırak sıralama; CRUD |
| `/media-library` | MediaLibrary/MediaLibrary.razor | MediaLibrary.View | Görsel kütüphanesi; upload; silme |

### Ayarlar

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/settings` | Settings/Settings.razor | Settings.View | Ayarlar hub sayfası; kart grid ile alt sayfalara link |
| `/settings/admin-users` | Settings/AdminUsers.razor | AdminUsers.View | Admin kullanıcı yönetimi; rol ataması; şifre sıfırlama |
| `/settings/roles` | Settings/Roles.razor | Roles.View | Rol & yetki yönetimi; permission checkbox matrisi |
| `/settings/app-content` | Settings/AppContentSettings.razor | Settings.AppContentEdit | Mobil uygulama KVKK/içerik metinleri |
| `/settings/communication-automation` | Settings/CommunicationAutomationSettings.razor | Settings.View | İletişim otomasyon ayarları (çalışma saati vb.) |
| `/settings/referral-reward` | Settings/ReferralRewardSettings.razor | Settings.ReferralRewardEdit | Referans ödül kural konfigürasyonu |

### Hata

| Route | Dosya | Permission | Açıklama |
|---|---|---|---|
| `/Error` | Error.razor | — | Hata sayfası (Request ID gösterimi) |

---

## 6. Layout ve Shared Bileşenler

### App.razor

- Bootstrap CSS + Bootstrap Icons CDN yüklemesi
- DevExpress tema: `BootstrapExternal`
- `PulsePoll.Admin.styles.css` (scoped CSS bundle)
- `auth.js` + `download.js` script yüklemesi
- `<Routes>` bileşenini `InteractiveServer` modunda render eder

### Routes.razor

- `<Router>` → `<PermissionChecker>` → `<RouteView DefaultLayout="MainLayout">`
- Bilinmeyen route'larda "Sayfa bulunamadı" mesajı

### MainLayout.razor

- `DrawerStateLayoutComponentBase`'den türetilmiş
- Sol taraf: `Drawer` + `NavMenu` (gradient sidebar, 240px genişlik)
- Üst bar: hamburger menü, "Back to Home" linki, kullanıcı dropdown menüsü
- Kullanıcı menüsü: "Profilim" (profil popup açar) + "Çıkış Yap"
- Profil popup: Ad, Soyad, Email, şifre değiştirme formu (`IAccessControlService`)
- `DxToastProvider` (sağ-alt, 5 saniye, Slide animasyonu)

### EmptyLayout.razor

- Sadece `@Body` — login sayfası için navigation olmadan temiz sayfa

### Drawer.razor

- `DrawerStateComponentBase`'den türetilmiş
- Mobil: `DrawerMode.Overlap` (üstte açılır)
- Masaüstü: `DrawerMode.Shrink` (içeriği daraltır)
- URL'deki `?toggledSidebar=true` query parametresiyle durum yönetimi

### DrawerStateComponentBase.cs

- `ComponentBase` ve `LayoutComponentBase` için iki abstract base class
- `[SupplyParameterFromQuery]` ile `toggledSidebar` URL parametresini okur
- `DrawerStateUrlBuilder` statik helper ile URL manipülasyonu

### MediaPickerPopup.razor

- `IMediaAssetService.GetAllAsync()` ile medya kütüphanesini listeler
- Grid görünümü; seçilen item üzerine yeşil badge
- Inline upload paneli: DxFileInput → byte okuma → önizleme → `MediaAssetService.UploadAsync()`
- `OnSelected` EventCallback ile parent'a seçilen `MediaAssetDto` döner
- `Visible` + `VisibleChanged` pattern (RZ10010 hatası önleme)

### PermissionChecker.razor

- Router ile page component arasında middleware gibi çalışır
- Reflection ile page type üzerinde `[AllowAnonymous]` ve `[HasPermission]` attribute kontrolü
- Authenticate değilse returnUrl ile login'e yönlendir
- Yetkisiz ise inline "Erişim Reddedildi" UI göster

---

## 7. Admin-Özel Servisler

| Servis | Görev |
|---|---|
| `IAppToastService` / `AppToastService` | DevExpress `IToastNotificationService` wrapper; Success/Error/Warning/Info metodları |
| `IPermissionAuthorizationService` / `PermissionAuthorizationService` | Scoped; Redis'ten permission yükle, fallback cookie claims; navigation'da cache invalidate |
| `ErrorHandler` (static) | `HandleFormSave` → Business/Validation/NotFoundException UI mesajına çevirir; `HandleOperation` → her zaman toast gösterir |

---

## 8. Statik Dosyalar (wwwroot)

### JavaScript

| Dosya | İşlev |
|---|---|
| `js/auth.js` | `window.adminAuth.login(data)` — `/api/auth/login`'e fetch POST; Login.razor `JS.InvokeAsync<LoginResult>("adminAuth.login", ...)` ile çağırır |
| `js/download.js` | `window.downloadBase64File(base64, fileName, mimeType)` — PaymentBatchDetail'da banka dosyasını tarayıcıya indirmek için |

### CSS

| Dosya/Dizin | İçerik |
|---|---|
| `css/site.css` | Global stiller, CSS custom properties (marka renkleri: `--pp-dark: #1A1535`, `--pp-accent: #7C5CFC`), Bootstrap token override |
| `css/icons.css` | Open-iconic font import |
| `css/bootstrap/` | Bootstrap 5 CSS |
| `css/open-iconic/` | Open Iconic icon font (eot, otf, svg, ttf, woff) |
| Scoped CSS | Her component için `.razor.css` dosyası (Login, Dashboard, MainLayout, Drawer, NavMenu) |

### Görseller

- `images/`: logo.svg, menu.svg, close.svg, back.svg, cards.svg, demos.svg, doc.svg
- `images/pages/`: counter.svg, home.svg, weather.svg (template kalıntıları)

---

## 9. Konfigürasyon Dosyaları

### appsettings.json

```
ConnectionStrings:
  Postgres    → Host=winpc;Port=5432;Database=pulsepoll_db
  Redis       → winpc:6379

Jwt:
  SecretKey, Issuer, Audience, expiry ayarları

MinIO:
  Endpoint    → winpc:9000
  UseSSL      → false

RabbitMQ:
  Host        → winpc
  Port        → 5672

Smtp:
  Host/Port/User/Password/From

Seq:
  ServerUrl   → http://localhost:5341

Serilog:
  MinimumLevel → Information
  Override     → Microsoft/System: Warning
```

### appsettings.Development.json

- `DetailedErrors: true`
- Log seviyesi override

### launchSettings.json

- HTTP → port 5010
- HTTPS → port 5011

---

## 10. Mimari Örüntüler ve Kurallar

### Temel Desenler

1. **Permission sistemi iki katmanlı**: Redis cache (primary) + cookie claims (fallback). Her navigation'da invalidate edilir.

2. **NavMenu permission reload**: `NavigationManager.LocationChanged` event'i → `GetCurrentPermissionsAsync()` → `_menuRenderKey` güncellemesi → menü yeniden render.

3. **Drawer state URL'de tutulur**: `?toggledSidebar=true` query parametresi. `DrawerStateComponentBase` bunu `[SupplyParameterFromQuery]` ile okur.

4. **DB thread safety**: Tüm `OnInitializedAsync` methodlarında `foreach + await` ile sıralı çağrı. `Task.WhenAll` ile paralel DB işlemi **yasak**.

5. **Context menu pattern**: Grid satırlarında `DxContextMenu` + `@ref` → `ShowMenuAsync(MouseEventArgs, item)` → `_contextMenu.ShowAsync(e)`.

6. **Popup buton sırası**: Aksiyon butonu **solda**, İptal her zaman **sağda** (Kaydet|İptal, Sil|İptal, Onayla|İptal).

7. **MediaPickerPopup**: `Visible` + `VisibleChanged` pattern; `OnParametersSetAsync`'te `Visible && !_previousVisible` kontrolü. `@bind-Visible` kullanılmaz (RZ10010 duplicate hatası).

8. **Rate limiting**: `/api/auth/login` → IP bazlı 5 istek / 5 dakika (fixed window). Private/loopback IP'den gelen isteklerde `X-Forwarded-For` header'a güven.

9. **Excel export**: `DxGrid.ExportToXlsxAsync("filename")` — her liste sayfasında mevcut.

10. **Profil düzenleme**: MainLayout içine gömülü popup; `IAccessControlService` ile CRUD; kendi rol/aktiflik bilgilerini koruyarak sadece ad/soyad/email/şifre günceller.

### DevExpress Blazor Kuralları

| Kural | Açıklama |
|---|---|
| `DxTextBox` | `@bind-Text` kullan, `@bind-Value` değil |
| `DxComboBox` filtre | `SearchMode="ListSearchMode.AutoSearch"` + `SearchFilterCondition` — `FilteringMode` OBSOLETE |
| `DxFileInput` tek dosya | `MaxFileCount="1"` — `UploadFileInfo.Size` tipi `decimal`, cast gerekir |
| `DxComboBox` cascade | `@bind-Value:after="OnXChangedAsync"` pattern |
| `DxGridDataColumn MinWidth` | Sadece SAYI alır — `MinWidth="150"` doğru, `"150px"` hata verir |
| `DxPopup` Context | `BodyContentTemplate Context="popupCtx"` — `context` isim çakışmasını önler |
| `DxComboBox ItemDisplayTemplate` | Context tipi: `ComboBoxItemDisplayTemplateContext<T>` |
| `DxPopup Visible` | `@bind-Visible` KULLANMA; `Visible="@x" VisibleChanged="handler"` + `OnParametersSetAsync` ile load tetikle |

### Hata Yönetimi

- `ErrorHandler.HandleFormSave()`: `BusinessException` → kullanıcı dostu mesaj; `ValidationException` → ilk hata mesajı; `NotFoundException` → "Kayıt bulunamadı"
- `ErrorHandler.HandleOperation()`: Her zaman toast bildirimi gösterir
- API dışı, Blazor Server tarafında direkt try-catch + toast pattern

### Naming Convention

- Sayfa dosyaları: PascalCase (ör. `WithdrawalRequests.razor`)
- Route'lar: kebab-case değil, çoğunlukla tekil/çoğul İngilizce (ör. `/payments/withdrawals`, `/subjects/{id}/ledger`)
- Permission kodları: `ModülAdı.Aksiyon` formatı (ör. `Payments.WithdrawalsView`, `Projects.ManageSubjects`)

---

## 11. Toplam Sayfa İstatistikleri

| Kategori | Sayfa Sayısı |
|---|---|
| Auth | 1 |
| Dashboard | 1 |
| Müşteriler | 1 |
| Projeler | 3 |
| Denekler | 6 |
| Ödemeler | 4 |
| İletişim | 2 |
| Bildirimler | 1 |
| İçerik | 2 |
| Medya | 1 |
| Ayarlar | 6 |
| Hata | 1 |
| **Toplam** | **29** |

| Bileşen Türü | Adet |
|---|---|
| Sayfa bileşenleri | 29 |
| Layout bileşenleri | 4 (MainLayout, EmptyLayout, Drawer, NavMenu) |
| Shared bileşenler | 3 (PermissionChecker, MediaPickerPopup, DrawerStateComponentBase) |
| Admin-özel servisler | 3 (AppToastService, PermissionAuthorizationService, ErrorHandler) |
| JS dosyaları | 2 (auth.js, download.js) |
| CSS dosyaları | 3 global + 5 scoped |

---

## 12. Harmanlanmış Bulgular (Aksiyon Odaklı)

Bu bölüm, mevcut analiz ile ek teknik inceleme bulgularının birleştirilmiş halidir. Öncelik sırası doğrudan uygulama riski ve üretim etkisine göre verilmiştir.

### Kritik / Yüksek

1. **Login dönüş URL'si doğrulanmıyor (open redirect riski)**
   - Durum: `returnUrl` login endpoint'inde doğrulanmadan geri dönülüyor, UI tarafı da doğrudan navigate ediyor.
   - Etki: Kötü niyetli link ile kullanıcı dış domaine yönlendirilebilir.
   - Referans:
     - `src/PulsePoll.Admin/Program.cs:167`
     - `src/PulsePoll.Admin/Components/Pages/Auth/Login.razor:99`
     - `src/PulsePoll.Admin/Components/Pages/Auth/Login.razor:104`
   - Aksiyon: Sadece local relative path (`/x`) kabul edilecek; dış URL ve `//` gibi şema-bağımsız yollar reddedilecek.

2. **Repo içinde düz metin altyapı secret bilgileri mevcut**
   - Durum: `appsettings.json` içinde DB/MinIO/RabbitMQ benzeri değerler plaintext.
   - Etki: Repo erişimi olan herkes doğrudan altyapıya bağlanabilir.
   - Referans:
     - `src/PulsePoll.Admin/appsettings.json:10`
     - `src/PulsePoll.Admin/appsettings.json:23`
     - `src/PulsePoll.Admin/appsettings.json:31`
   - Aksiyon: Secret değerleri User Secrets/ENV/secret vault'a taşınmalı; dosyada placeholder kalmalı.

3. **Proxy arkasında güvenli cookie/scheme davranışı net değil**
   - Durum: Cookie `SameAsRequest`; forwarded headers middleware yok.
   - Etki: Reverse-proxy terminasyonunda güvenlik bayrakları/redirect davranışı yanlış çalışabilir.
   - Referans:
     - `src/PulsePoll.Admin/Program.cs:69`
     - `src/PulsePoll.Admin/Program.cs:126`
   - Aksiyon: `UseForwardedHeaders` eklenecek; güvenli ortamda HTTPS algısı proxy header'larıyla normalize edilecek.

### Orta

4. **Bildirim Geçmişi ekranında gerçek server-side pagination yok**
   - Durum: Sabit `500` kayıt çekilip UI'da listeleniyor.
   - Etki: 500 üzeri veri görünmez; yüksek hacimde performans düşer.
   - Referans:
     - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:190`
     - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:191`
   - Aksiyon: Repository seviyesinde sayfalı yükleme UI ile bağlanmalı.

5. **Permission cache her navigation'da invalidate ediliyor**
   - Durum: `LocationChanged` ile yetki cache sürekli sıfırlanıyor.
   - Etki: Gereksiz Redis/db erişimi, circuit başına ek yük.
   - Referans:
     - `src/PulsePoll.Admin/Services/PermissionAuthorizationService.cs:94`
   - Aksiyon: Navigation invalidation kaldırılacak; sadece rol/yetki değişiminde explicit invalidate kullanılacak.

6. **Permission attribute'larında string literal kullanımı var**
   - Durum: Bazı sayfalarda `HasPermission("X.Y")` şeklinde hard-coded değerler kullanılıyor.
   - Etki: Refactor/typo hatalarında compile-time güvence kaybı.
   - Referans:
     - `src/PulsePoll.Admin/Components/Pages/Content/Stories.razor:2`
     - `src/PulsePoll.Admin/Components/Pages/Content/News.razor:2`
     - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:2`
   - Aksiyon: Tamamı `PermissionCodes.*` sabitlerine taşınacak.

7. **`int.Parse` ile claim parse edilen noktalar mevcut**
   - Durum: Claim parse hatasında sayfa kırılabilir.
   - Etki: Beklenmeyen auth state durumlarında runtime exception.
   - Aksiyon: `int.TryParse` + guard pattern'e taşınmalı.

### Düşük

8. **Kullanıcıya ham exception mesajı gösterilen noktalar var**
   - Durum: Bazı catch bloklarında `ex.Message` doğrudan toast'a basılıyor.
   - Etki: İç teknik detaylar son kullanıcıya sızabilir.
   - Aksiyon: Kullanıcı dostu genel mesaj + log ayrımı.

9. **Harici CDN icon bağımlılığı için fallback/SRI yok**
   - Durum: Bootstrap Icons CDN'e bağlı.
   - Etki: CDN kesintisinde icon kaybı; güvenlik sertleştirmesi eksik.
   - Aksiyon: Yerel paket veya SRI değerlendirilmeli.

### Doğrulama Notu

- Analiz dosyasındaki genel mimari/route envanteri büyük oranda doğru ve güncel.
- Ancak bu bölümdeki maddeler, envanter bilgisini risk/aksiyon sırasına dönüştürmek için eklenmiştir.
