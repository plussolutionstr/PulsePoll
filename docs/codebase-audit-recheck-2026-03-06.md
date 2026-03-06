# PulsePoll Denetim Yeniden Kontrol Raporu

**Tarih:** 6 Mart 2026  
**Kapsam:** Önceki `docs/codebase-audit-2026-03-06.md` içindeki 14 bulgunun yeniden doğrulaması  
**Not:** `appsettings` ve `docs/demo-data` kapsam dışı tutuldu.

## Sonuç Özeti

- **Tam çözülen:** 13
- **Kısmen çözülen:** 0
- **Açık kalan:** 1

## Bulgu Bazlı Durum

1. **Anonim medya erişimi** -> **ÇÖZÜLDÜ**  
   Kanıt: `MediaController` class seviyesinde `[Authorize]`; public erişim yalnızca `public/{bucket}` ve allowlist ile sınırlı.  
   Dosya: `src/PulsePoll.Api/Controllers/MediaController.cs:9,15-21,38-43`  
   Ek: Admin media proxy artık yetkili (`RequireAuthorization`) ve bucket allowlist var.  
   Dosya: `src/PulsePoll.Admin/Program.cs:206-219`

2. **Forwarded header trust + login CSRF** -> **ÇÖZÜLDÜ**  
   - Login tarafında `DisableAntiforgery()` kaldırılmış.  
   - Rate-limit IP çözümü manuel `X-Forwarded-For` parse etmeden `RemoteIpAddress` kullanacak şekilde sadeleştirildi.  
     Dosya: `src/PulsePoll.Admin/Program.cs:230-233`

3. **Mobile MacCatalyst build kırığı** -> **ÇÖZÜLDÜ (ÜRÜN KARARIYLA)**  
   - Mobil uygulama kapsamı iOS + Android olarak netleştirildi (`TargetFrameworks: android;ios`).  
     Dosya: `src/PulsePoll.Mobile/PulsePoll.Mobile.csproj:4`  
   - `Platforms/MacCatalyst` ve `Platforms/Windows` kaldırıldı.

4. **Test projesi derlenmiyor** -> **ÇÖZÜLDÜ**  
   - `SubjectServiceTests` constructor bağımlılıkları güncel imzayla hizalı.  
     Dosya: `tests/PulsePoll.Tests/SubjectServiceTests.cs:21-37`  
   - `dotnet test` sonucu: **20/20 başarılı**.

5. **Cüzdan akışlarında atomiklik eksik** -> **ÇÖZÜLDÜ**  
   - Cüzdan kritik akışlarında transaction sınırı kullanılıyor (`Begin/Commit/Rollback`, withdrawal içinde DB transaction).  
     Dosyalar:  
     - `src/PulsePoll.Application/Services/WalletService.cs:137-202,250-265,279-350`  
     - `src/PulsePoll.Infrastructure/Persistence/Repositories/WalletRepository.cs:74-131`

6. **MessageAutomation N+1 / fazla SaveChanges** -> **ÇÖZÜLDÜ**  
   - Mevcut dispatch loglar toplu çekiliyor (`GetExistingDispatchLogsAsync`) ve toplu insert (`AddDispatchLogsAsync`) yapılıyor.  
     Dosyalar:  
     - `src/PulsePoll.Application/Services/MessageAutomationService.cs:136-163`  
     - `src/PulsePoll.Infrastructure/Persistence/Repositories/MessageAutomationRepository.cs:78-103`

7. **Gereksiz büyük veri çekimi** -> **ÇÖZÜLDÜ**  
   - Ledger artık SQL window function + sayfalama ile geliyor.  
     Dosya: `src/PulsePoll.Infrastructure/Persistence/Repositories/WalletRepository.cs:153-208`  
   - Toplu SMS hedefleri artık `GetByIdsAsync` ile çekiliyor.  
     Dosyalar:  
     - `src/PulsePoll.Application/Services/SubjectService.cs:151-155`  
     - `src/PulsePoll.Infrastructure/Persistence/Repositories/SubjectRepository.cs:66-69`

8. **Presentation katmanında doğrudan DbContext** -> **ÇÖZÜLDÜ**  
   - 4 Razor sayfasındaki doğrudan `DbContext/IDbContextFactory` erişimi kaldırıldı.  
   - Bu sorgular `IAdminGridDataService` üzerinden servis katmanına taşındı.  
     Dosyalar:  
     - `src/PulsePoll.Application/Interfaces/IAdminGridDataService.cs`  
     - `src/PulsePoll.Infrastructure/Services/AdminGridDataService.cs`  
     - `src/PulsePoll.Admin/Components/Pages/Subjects/Subjects.razor`  
     - `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor`  
     - `src/PulsePoll.Admin/Components/Pages/Projects/ProjectSubjects.razor`  
     - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor`

9. **Sessiz catch / hata yutma** -> **ÇÖZÜLDÜ**  
   - `SubjectService` içindeki sessiz catch blokları warning log ile değiştirildi.  
     Dosya: `src/PulsePoll.Application/Services/SubjectService.cs:99-104,176-181`  
   - `WithdrawalRequests` toplu onay/red döngülerinde sessiz catch kaldırıldı; kayıt bazlı warning log + kullanıcıya hata sayısı eklendi.  
     Dosya: `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor:306-317,385-397`  
   - `RegisterViewModel` IP alma hatasında debug log eklendi.  
     Dosya: `src/PulsePoll.Mobile/ViewModels/RegisterViewModel.cs:309-312`

10. **Swagger ortam ayrımı olmadan açık** -> **ÇÖZÜLDÜ**  
    - Swagger artık sadece `Development` altında açılıyor.  
      Dosya: `src/PulsePoll.Api/Program.cs:37-41`

11. **TODO ve iş mesajı tutarsızlığı** -> **ÇÖZÜLDÜ**  
    - Onay e-postası metni hesap durumuyla uyumlu hale getirildi.  
      Dosya: `src/PulsePoll.Worker/Consumers/UserRegisteredConsumer.cs:106-109`  
    - LSM tarafında `TODO` kaldırıldı; bilinçli varsayılan atama notu bırakıldı (altyapı korunuyor).  
      Dosya: `src/PulsePoll.Worker/Consumers/UserRegisteredConsumer.cs:37-38`  
    - LSM alanları UI’dan gizlendi (Admin Subject/Project ekranları).  
      Dosyalar:  
      - `src/PulsePoll.Admin/Components/Pages/Subjects/Subjects.razor`  
      - `src/PulsePoll.Admin/Components/Pages/Projects/ProjectSubjects.razor`

12. **Bağımlılıklar (outdated + kullanılmayan paket)** -> **AÇIK**  
    - Merkezi paket yönetimi aktif (`ManagePackageVersionsCentrally=true`) ve API'deki atıl `FluentValidation.DependencyInjectionExtensions` referansı kaldırıldı.  
      Dosyalar:  
      - `Directory.Packages.props`  
      - `src/PulsePoll.Api/PulsePoll.Api.csproj`  
    - Güvenli patch/minor güncellemeler uygulandı (`DevExpress.Blazor`, `Microsoft.Extensions.Logging.Debug`, `CommunityToolkit.Mvvm`, `Microsoft.Maui.Controls`).  
    - Ancak `dotnet list package --outdated` çıktısına göre major kırılım riski taşıyan paketler hâlâ güncel değil (`MassTransit`, `Serilog`, `Swashbuckle`, `FluentValidation`, `Minio`, `CommunityToolkit.Maui`, test paketleri).  
    - EF Core `10.0.3` denemesi `EFCore.NamingConventions 10.0.1` ile Relational assembly çakışması ürettiği için EF sürümü bu turda `10.0.1`de bırakıldı.

13. **İsimlendirme + README eksikliği** -> **ÇÖZÜLDÜ**  
    - Test dosya/sınıf isimleri hizalı (`SubjectServiceTests.cs` / `SubjectServiceTests`).  
      Dosya: `tests/PulsePoll.Tests/SubjectServiceTests.cs:13`  
    - Root `README.md` mevcut.  
      Dosya: `README.md:1`

14. **Türkçe ASCII kullanım (`musteriler`)** -> **ÇÖZÜLDÜ**  
    - Export adı `müşteriler` olarak düzeltilmiş.  
      Dosya: `src/PulsePoll.Admin/Components/Pages/Customers/Customers.razor:284`

## Kısa Aksiyon Önceliği

1. `#12` paket güncellemeleri ve olası atıl package cleanup
