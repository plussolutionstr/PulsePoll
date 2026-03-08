# Scoped Code Audit

Kapsam `4055b30 feat: implement full dark mode support with AppThemeBinding` commitinden `HEAD`'e kadar daraltıldı. Başlangıç commit tarihi: `2026-03-08 02:58:34 +03:00`.

Bu aralıkta değişen 71 dosya incelendi. Türkçe karakter taramasında, kullanıcıya görünen metinlerde bozuk Türkçe karakter bulunmadı.

## Bulgular

### 🟠 [Hata Yönetimi] — Cooldown hesapları yanlış zaman tabanı kullanıyor

**Dosya:** `src/PulsePoll.Application/Services/WalletService.cs`  
**Satır:** 226-233, 400-414, 484-490  
**Sorun:** `CreatedAt` alanı `TurkeyTime.Now` ile tutuluyor, ama yeni cooldown kontrolleri `DateTime.UtcNow` ile kıyaslanıyor.  
**Etki:** IBAN silme/çekim blokesi gerçekte yaklaşık 3 saat fazla sürebilir. UI sadece tarih gösterdiği için kullanıcı "bugün açılması gerekiyordu" diye hatalı blok yaşayabilir.  
**Öneri:** Cooldown karşılaştırmalarında `TurkeyTime.Now` kullanın ya da tüm tarihleri UTC veya `DateTimeOffset` standardına taşıyın.

### 🟠 [UI/Güvenilirlik] — Connection error overlay retry akışı kilitlenebilir

**Dosya:** `src/PulsePoll.Mobile/Controls/ConnectionErrorOverlay.xaml.cs`  
**Satır:** 5-12, 19-33  
**Sorun:** `RetryCommandProperty` tipi `Command`, fakat sayfalarda bağlanan komutlar MVVM Toolkit'in ürettiği `RetryConnectionCommand` nesneleri. Kaynak koda göre bu tip uyumsuzluğu binding'in `RetryCommand`'ı boş bırakmasına yol açabilir.  
**Etki:** Bağlantı hatası ekranı kullanıcı için çıkmaz sokak olabilir; "Yeniden Dene" hiçbir şey yapmayabilir. Aynı desen `History`, `NewsDetail`, `StoryViewer`, `SurveyDetail`, `Surveys`, `Wallet`, `Home` ekranlarında tekrar ediyor.  
**Öneri:** Bindable property tipini `ICommand` yapın. Async komutları da gerçekten beklemek için sabit `Task.Delay(500)` yerine `IAsyncRelayCommand.ExecuteAsync` benzeri bir yol kullanın.

### 🟡 [Veri Bütünlüğü] — `BankCode` alanı validasyonsuz ve benzersizlik garantisiz

**Dosya:** `src/PulsePoll.Application/Services/BankService.cs`  
**Satır:** 21-49  
**Sorun:** Admin panel her türlü `BankCode` değerini kabul ediyor; servis sadece `Trim()` yapıyor. Uzunluk, numerik format ve uniqueness kontrolü yok. Mobile tarafı ise ilk eşleşeni banka kabul ediyor.  
**Etki:** Hatalı veya duplicate banka kodları valid IBAN'ları reddedebilir ya da yanlış bankayı otomatik seçebilir.  
**Öneri:** `BankCode` için 5 haneli numerik doğrulama ekleyin, benzersiz index oluşturun, servis seviyesinde duplicate kontrolü yapın.

### 🟡 [UX/Hata Yönetimi] — IBAN alanının görsel kontratı ile validasyon kontratı çakışıyor

**Dosya:** `src/PulsePoll.Mobile/Views/WalletAddBankAccountPage.xaml`  
**Satır:** 31-37  
**Sorun:** Ekran solda sabit `TR` etiketi gösteriyor, placeholder da `TR...` ile başlıyor, fakat save validasyonu kullanıcının girdiği değerin de `TR` ile başlamasını zorunlu tutuyor.  
**Etki:** Kullanıcı sabit prefix'i UI'nin sağladığını varsayıp sadece kalan 24 karakteri girerse, görsel olarak doğru görünen giriş sahte "Geçerli bir TR IBAN girin." hatası verir.  
**Öneri:** Ya sabit `TR` label'ını kaldırın ya da normalizasyonda eksik prefix'i tamamlayın. Placeholder, hint ve validasyon aynı kontrata çekilmeli.

### 🟡 [Test] — Wallet soft delete değişikliği testlere yansıtılmamış

**Dosya:** `tests/PulsePoll.Tests/WalletServiceTests.cs`  
**Satır:** 327-335  
**Sorun:** Servis artık fiziksel silme yerine soft delete + `UpdateBankAccountAsync` yapıyor; test hâlâ `DeleteBankAccountAsync(account)` çağrısını bekliyor.  
**Etki:** `dotnet test tests/PulsePoll.Tests/PulsePoll.Tests.csproj --no-restore` komutu 1 başarısız test veriyor. Yeni cooldown davranışı da test kapsamına alınmamış.  
**Öneri:** Testi soft delete akışına göre güncelleyin; ayrıca "cooldown aktifken silinemez" ve "cooldown bitince silinir" senaryolarını ekleyin.

## Türkçe Karakter Denetimi

Bu commit aralığında kullanıcıya görünen metinlerde bozuk Türkçe karakter bulunmadı.

## Doğrulama

- `dotnet test tests/PulsePoll.Tests/PulsePoll.Tests.csproj --no-restore`: 1 test fail, 19 pass
- `dotnet build PulsePoll.sln --no-restore`: mobile tarafta restore/runtime asset eksikleri (`NETSDK1047`) ve yerel Java SDK uyarısı verdi; bunlar commit regresyonundan çok build ortamı veya restore durumu kaynaklı görünüyor

## Özet Tablo

| # | Seviye | Kategori | Dosya | Kısa Açıklama |
|---|---|---|---|---|
| 1 | 🟠 | Hata Yönetimi | `src/PulsePoll.Application/Services/WalletService.cs` | Cooldown kontrolleri `TurkeyTime` yerine `UtcNow` ile yapılıyor |
| 2 | 🟠 | UI/Güvenilirlik | `src/PulsePoll.Mobile/Controls/ConnectionErrorOverlay.xaml.cs` | Retry command tipi binding tarafıyla uyumsuz |
| 3 | 🟡 | Veri Bütünlüğü | `src/PulsePoll.Application/Services/BankService.cs` | `BankCode` için format ve unique koruması yok |
| 4 | 🟡 | UX/Hata Yönetimi | `src/PulsePoll.Mobile/Views/WalletAddBankAccountPage.xaml` | IBAN alanı TR prefix'ini iki farklı şekilde gösteriyor |
| 5 | 🟡 | Test | `tests/PulsePoll.Tests/WalletServiceTests.cs` | Soft delete değişikliği testleri kırmış |

## Aksiyon Planı

1. `WalletService` zaman karşılaştırmalarını düzeltin.
2. `ConnectionErrorOverlay` komut tipini `ICommand` yapıp retry akışını gerçek async beklemeye çevirin.
3. `BankCode` için validation ve unique index ekleyin.
4. IBAN giriş kontratını sadeleştirip testleri güncelleyin.
