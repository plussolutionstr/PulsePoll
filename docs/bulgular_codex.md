# PulsePoll Kapsamlı Kod Denetimi Bulguları (Codex, Revize)

Tarih: 2026-03-05  
Kapsam: `docs/bulgular_claude.md` ile karşılaştırmalı tekrar doğrulama + kod üstünden satır bazlı teyit.

## Durum Etiketleri

- `✅ Doğrulandı`: Kodda doğrudan kanıtlandı.
- `⚠️ Olası`: Risk var; iş kuralına bağlı veya niyet net değil.
- `❌ Doğrulanamadı`: Koddan net kanıt çıkmadı / yanlış pozitif.

## 1) Claude'da Olup Önceki Codex Raporunda Eksik Kalanlar

### ✅ 🔴 [Güvenlik] — Story admin mutation endpoint’leri yetkisiz
**Dosya:** `src/PulsePoll.Api/Controllers/StoryController.cs`  
**Satır:** 32-67  
**Sorun:** `POST/PUT/DELETE` için `[Authorize]` yok.  
**Etki:** İçerik manipülasyonu ve yetkisiz yönetim işlemleri.  
**Öneri:** Controller veya mutation action seviyesinde admin policy zorunlu kılın.

### ✅ 🟣 [Türkçe] — Withdrawal talepleri ekranında yaygın bozuk karakter
**Dosya:** `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor`  
**Satır:** 26, 35, 42, 51, 61, 126, 168, 181, 272, 308, 331, 333, 355, 361, 384, 410, 435  
**Sorun:** `Eklenmemis`, `Tumunu Sec`, `Secimi`, `Iptal`, `Lutfen`, `Onayli` vb.  
**Etki:** Yönetim paneli kalite ve güven algısı düşüyor.  
**Öneri:** Tüm kullanıcıya görünen string’leri Türkçe karakterli normalize edin.

```text
📁 Dosya: src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor
📍 Satır: 35
❌ Mevcut: "Tumunu Sec"
✅ Önerilen: "Tümünü Seç"

📍 Satır: 181
❌ Mevcut: "Iptal"
✅ Önerilen: "İptal"
```

### ✅ 🟠 [Mimari] — Doğrudan DbContext kullanımı beklenenden daha yaygın
**Dosyalar:**  
`Subjects.razor`, `ProjectSubjects.razor`, `NotificationHistory.razor`, `WithdrawalRequests.razor`, `Banks.razor`  
**Sorun:** Presentation katmanı doğrudan EF sorgusu yapıyor (`IDbContextFactory<AppDbContext>`).  
**Etki:** Katman ihlali, test maliyeti, davranışın sayfa içinde dağılması.  
**Öneri:** Query/command katmanı (Application service/handler) üzerinden ilerleyin.

### ✅ 🟡 [Kod Kalitesi] — Tekrarlı sabit/helper
**Dosyalar:** `NewsService.cs`, `StoryService.cs`, `ProjectService.cs`, `WalletService.cs`, `MediaAssetService.cs`, `CustomerService.cs`  
**Sorun:**  
- `NormalizeToTurkeyLocal` birebir tekrar (`NewsService`/`StoryService`)  
- `PresignedUrlExpirySeconds = 7 * 24 * 3600` 6 yerde tekrar  
- `"media-library"` 5 yerde tekrar  
**Etki:** Değişiklik maliyeti ve konfig drift riski.  
**Öneri:** Ortak constant/helper sınıfına alın.

### ✅ 🟡 [Performans] — Mobil kayıtta kısa ömürlü `HttpClient` üretimi
**Dosya:** `src/PulsePoll.Mobile/ViewModels/RegisterViewModel.cs`  
**Satır:** 277  
**Sorun:** `new HttpClient()` her kayıt akışında oluşturuluyor.  
**Etki:** Socket yönetimi ve retry/policy yönetimi zayıf.  
**Öneri:** `IHttpClientFactory` veya mevcut API client üstünden çözün.

### ✅ 🟡 [Hata Yönetimi] — Worker scheduler’da geniş catch + sessiz fallback
**Dosya:** `src/PulsePoll.Worker/Services/CommunicationAutomationJobScheduler.cs`  
**Satır:** 55-58  
**Sorun:** `catch (Exception)` ile `TimeZoneInfo.Local` fallback, log yok.  
**Etki:** Yanlış timezone konfigurasyonu görünmez kalır.  
**Öneri:** Spesifik exception + warning log.

### ✅ 🟡 [Kod Kalitesi] — Ek uzun/god dosya
**Dosyalar:** `WalletService.cs` (568 satır), `ProjectSubjects.razor` (1127 satır)  
**Sorun:** Çok sorumluluk tek dosyada toplanmış.  
**Etki:** Bakım, regression ve review maliyeti artar.  
**Öneri:** Fonksiyonel alt bileşen/servislere bölün.

## 2) Mevcut Bulguların Gerçeklik Sorgusu (Güncellenmiş)

### ✅ Doğrulanan Kritik/Yüksek Bulgular

1. `appsettings.json` dosyalarında plaintext credential/secret var (`Api`, `Admin`, `Worker`).
2. `IbanEncrypted` alanına plaintext IBAN yazılıyor (`WalletService`, `SubjectRegisteredConsumer`).
3. Admin mutation endpoint’lerinde yetkilendirme eksikleri var (`Customer`, `News`, `Story`, `Admin/Withdrawal`).
4. `UseCors("Dev")` koşulsuz; `Dev` policy `AllowAnyOrigin/Method/Header`.
5. `WebhookController` içinde imza/HMAC doğrulama yok.
6. `PaymentBatchService` ve `WalletService` withdrawal approve/reject akışını paralel taşıyor (davranış ayrışma riski).
7. `MessageAutomationService` içinde hedef başına `DispatchLogExists` + `AddDispatchLog` (N+1/write amplification).
8. `WalletService.GetLedgerAsync` içinde `int.MaxValue` ile tüm transaction çekiliyor.
9. Test projesi derlenmiyor (2026-03-05 tekrar çalıştırma):
   - `SubjectService` constructor imza uyumsuzluğu
   - `WalletService` constructor param uyumsuzluğu
   - İki adet `string -> int` argüman hatası

### ⚠️ Olası / İş Kuralına Bağlı

1. `ProjectController.GetById(int id)` için IDOR riski:
   - `GetAssigned` endpoint’i ayrı olduğu için niyet “sadece atanmış projeler” olabilir.
   - Ancak koddan kesin iş kuralı görülemediği için `⚠️` olarak tutuldu.

### ❌ Doğrulanamayan veya Zayıf Bulgular

1. “API tarafında `RabbitMQ.VirtualHost` eksikliği kritik”:
   - Kodda `config["RabbitMQ:VirtualHost"] ?? "/"` fallback var; doğrudan hata değil.
2. “Worker’da `appsettings.Development.json` olmaması sorun”:
   - Zorunlu değil; ekip tercihi.
3. “Admin appsettings içindeki Jwt bloğu kesin gereksiz”:
   - `AddInfrastructure` JWT config bind ettiği için tamamen yanlış demek mümkün değil.

## 3) Türkçe Karakter Doğrulama Özeti

Doğrulanan ana odak dosyalar:

- `src/PulsePoll.Mobile/Views/RegisterPage.xaml` (KVKK metni ve onay cümlesi)
- `src/PulsePoll.Mobile/ViewModels/ProfileViewModel.cs`
- `src/PulsePoll.Mobile/Views/ProfilePage.xaml`
- `src/PulsePoll.Admin/Components/Pages/Payments/PaymentBatches.razor`
- `src/PulsePoll.Admin/Components/Pages/Payments/PaymentBatchDetail.razor`
- `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor` (ek 15+ bulgu)

## 4) NuGet ve Yapı Doğrulaması

### ✅ Doğrulandı

1. `FluentValidation.AspNetCore` paketi deprecated/legacy:
   - Komut: `dotnet list src/PulsePoll.Api/PulsePoll.Api.csproj package --deprecated`
2. `Microsoft.Data.Sqlite.Core` ve `SQLitePCLRaw.bundle_green` versiyonları merkezi dosyada tanımlı, referans bulunmadı.
3. `.gitignore` içinde `.idea/` tekrarlı ve `bin\\Debug` klasörü için anormal desen mevcut.

## 5) Özet Tablo

| # | Durum | Seviye | Kategori | Dosya | Kısa Açıklama |
|---|---|---|---|---|---|
| 1 | ✅ | 🔴 | Güvenlik | `StoryController.cs` | Admin mutation endpoint auth yok |
| 2 | ✅ | 🟣 | Türkçe | `WithdrawalRequests.razor` | Çoklu bozuk Türkçe karakter |
| 3 | ✅ | 🟠 | Mimari | 5 Razor sayfası | Doğrudan `DbContext` kullanımı |
| 4 | ✅ | 🟡 | Kod Kalitesi | `NewsService`/`StoryService` vb. | Helper/sabit tekrarları |
| 5 | ✅ | 🟡 | Performans | `RegisterViewModel.cs` | `new HttpClient()` kullanımı |
| 6 | ✅ | 🟡 | Hata Yönetimi | `CommunicationAutomationJobScheduler.cs` | Geniş catch + sessiz fallback |
| 7 | ✅ | 🟡 | Kod Kalitesi | `WalletService.cs`, `ProjectSubjects.razor` | Aşırı uzun/god dosya |
| 8 | ✅ | 🔴 | Güvenlik | `appsettings.json` dosyaları | Hardcoded credential/secret |
| 9 | ✅ | 🔴 | Güvenlik | `WalletService.cs`, `SubjectRegisteredConsumer.cs` | `IbanEncrypted` plaintext |
| 10 | ✅ | 🟠 | Güvenlik | `WebhookController.cs` | İmza doğrulaması yok |
| 11 | ✅ | 🟠 | Performans | `MessageAutomationService.cs` | N+1 + write amplification |
| 12 | ✅ | 🟠 | Performans | `WalletService.cs` | `int.MaxValue` ile ledger |
| 13 | ⚠️ | 🔴 | Güvenlik | `ProjectController.cs` | Olası IDOR (iş kuralına bağlı) |
| 14 | ❌ | 🟡 | Config | API rabbit config | `VirtualHost` fallback mevcut |

## 6) Öncelikli Aksiyon Planı (Revize)

1. Güvenlik hotfix: `[Authorize]` eksikleri + webhook signature + secret taşıma.
2. IBAN şifreleme ve veri koruma stratejisi (key management dahil).
3. Türkçe metin normalizasyonu (özellikle KVKK + ödeme ekranları + withdrawal ekranı).
4. Test suite derleme kırıklarını kapatıp CI’da zorunlu geçiş kuralı koyma.
5. Admin Razor sayfalarında `DbContext` kullanımını Application katmanına taşıma.
6. Tekrarlı sabit/helper konsolidasyonu.
