# PulsePoll.Admin A'dan Z'ye İnceleme Bulguları

Tarih: 4 Mart 2026  
Kapsam: `src/PulsePoll.Admin` altındaki tüm sayfalar, layout, auth/permission akışı, JS yardımcıları, konfigürasyon dosyaları.

## Kritik/Yüksek

1. **Repo içinde düz metin gizli bilgiler bulunuyor**
- Etki: Konfigürasyon sızıntısı ile DB/Redis/MinIO/RabbitMQ erişimi ele geçirilebilir.
- Referans:
  - `src/PulsePoll.Admin/appsettings.json:10`
  - `src/PulsePoll.Admin/appsettings.json:11`
  - `src/PulsePoll.Admin/appsettings.json:23`
  - `src/PulsePoll.Admin/appsettings.json:24`
  - `src/PulsePoll.Admin/appsettings.json:31`

2. **Login sonrası açık yönlendirme (open redirect) riski var**
- Etki: Kullanıcı başarılı login sonrası dış bir adrese yönlendirilebilir.
- Neden: `returnUrl` doğrulanmadan geri döndürülüp client tarafında doğrudan navigate ediliyor.
- Referans:
  - `src/PulsePoll.Admin/Program.cs:167`
  - `src/PulsePoll.Admin/Components/Pages/Auth/Login.razor:99`
  - `src/PulsePoll.Admin/Components/Pages/Auth/Login.razor:104`

3. **Proxy arkasında güvenli cookie/scheme davranışı kırılabilir**
- Etki: Reverse proxy terminasyonunda `Secure` flag/şema tespiti yanlış çalışabilir.
- Neden: `CookieSecurePolicy.SameAsRequest` var, fakat forwarded headers middleware yok.
- Referans:
  - `src/PulsePoll.Admin/Program.cs:69`
  - `src/PulsePoll.Admin/Program.cs:126`

## Orta

4. **Bildirim Geçmişi ekranda hard-limit 500 kayıt çekiliyor (gerçek pagination yok)**
- Etki: 500 üzeri kayıtlar listelenemez; geçmiş eksik görünür.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:190`
  - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:191`

5. **NotificationHistory katman sınırını deliyor (UI doğrudan repository enjekte ediyor)**
- Etki: İş kuralı/DTO standardı bypass ediliyor, sürdürülebilirlik ve test edilebilirlik düşüyor.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:5`
  - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:6`

6. **Story/Haber sıralama güncellemesi N adet item için N adet update çağrısı yapıyor**
- Etki: Büyük listelerde performans ve işlem süresi kötüleşir.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Content/Stories.razor:485`
  - `src/PulsePoll.Admin/Components/Pages/Content/Stories.razor:498`
  - `src/PulsePoll.Admin/Components/Pages/Content/News.razor:485`
  - `src/PulsePoll.Admin/Components/Pages/Content/News.razor:498`

7. **İletişim otomasyon saat ayarı için yetki modeli tutarsız**
- Etki: Sayfaya erişim `Settings.View` ile açılıyor, ama düzenleme `Communications.Edit` ile kontrol ediliyor; rol kurgusunda karışıklık yaratır.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Settings/CommunicationAutomationSettings.razor:2`
  - `src/PulsePoll.Admin/Components/Pages/Settings/CommunicationAutomationSettings.razor:66`

8. **Permission attribute'larında string literal kullanımı mevcut**
- Etki: Refactor/rename sırasında derleyici koruması azalır.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Content/News.razor:2`
  - `src/PulsePoll.Admin/Components/Pages/Content/Stories.razor:2`
  - `src/PulsePoll.Admin/Components/Pages/Notifications/NotificationHistory.razor:2`

9. **Permission cache her navigation'da invalidate edilip yeniden yükleniyor**
- Etki: Redis çağrıları gereksiz artar; menü geçişlerinde ekstra yük oluşturur.
- Referans:
  - `src/PulsePoll.Admin/Services/PermissionAuthorizationService.cs:94`
  - `src/PulsePoll.Admin/Services/PermissionAuthorizationService.cs:95`
  - `src/PulsePoll.Admin/Components/Layout/NavMenu.razor:120`

## Düşük

10. **Claim parse işlemlerinde `int.Parse` kullanımı kırılgan**
- Etki: Beklenmeyen/bozuk claim durumunda UI exception verebilir.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Payments/PaymentSettings.razor:167`
  - `src/PulsePoll.Admin/Components/Pages/Payments/PaymentBatches.razor:256`
  - `src/PulsePoll.Admin/Components/Pages/Payments/WithdrawalRequests.razor:289`

11. **Beklenmeyen hata mesajları son kullanıcıya ham exception metniyle gösteriliyor**
- Etki: İç teknik detayların kullanıcıya sızması.
- Referans:
  - `src/PulsePoll.Admin/Services/ErrorHandler.cs:31`
  - `src/PulsePoll.Admin/Services/ErrorHandler.cs:38`

12. **Dashboard hızlı erişim linkleri yetkiye göre filtrelenmiyor**
- Etki: Kullanıcı tıklayınca sık “Erişim Reddedildi” deneyimi.
- Referans:
  - `src/PulsePoll.Admin/Components/Pages/Dashboard/Dashboard.razor:106`
  - `src/PulsePoll.Admin/Components/Pages/Dashboard/Dashboard.razor:112`

13. **Harici CDN bağımlılığı var (bootstrap-icons), SRI/fallback yok**
- Etki: CDN erişim/supply-chain problemi halinde ikonlar etkilenebilir.
- Referans:
  - `src/PulsePoll.Admin/Components/App.razor:8`

## Pozitif Gözlemler

- Build temiz: `dotnet build src/PulsePoll.Admin/PulsePoll.Admin.csproj` (0 hata, 0 uyarı)
- Popup standardı tutarlı (`ShowFooter="true"`) görünümünde önemli iyileştirme yapılmış.
- Role/permission UI ve menü sıralaması önceki duruma göre daha düzenli ve yönetilebilir.

## Önerilen Önceliklendirme

1. Gizli bilgileri repo dışına taşı (`UserSecrets`/env vars/secret manager) ve rotasyon yap.  
2. Login `returnUrl` için local-path whitelist doğrulaması ekle.  
3. Forwarded headers + secure cookie davranışını production topolojisine göre sabitle.  
4. NotificationHistory için gerçek server-side pagination uygula (page index/size).  
5. String literal permission kullanımını sabitlere taşı.
