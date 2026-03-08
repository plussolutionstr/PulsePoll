# Branch Audit: `feature/scheduled-distribution`

Kapsam: Bu rapor yalnızca `main...feature/scheduled-distribution` diff'inde yer alan kodlar için hazırlanmıştır. Geçmiş kodlar bilinçli olarak kapsam dışı bırakılmıştır.

## Bulgular

### 🟠 [İş Mantığı] — Manuel dağıtım/reminder akışı aktif olmayan projeleri de çalıştırabiliyor

**Dosya:** `src/PulsePoll.Application/Services/DistributionService.cs`, `src/PulsePoll.Admin/Components/Pages/Projects/ProjectDistribution.razor`  
**Satır:** 26-30, 149-164, 99-113, 195-224  
**Sorun:** Manuel "Dağıtım Çalıştır" ve "Hatırlatma Gönder" akışları `project.Status == Active` ve `project.IsScheduledDistribution` doğrulaması yapmıyor. Route doğrudan açılırsa pause/completed ya da planlı dağıtımı kapalı projelerde de servis çağrılabiliyor.  
**Etki:** Yanlış projeye push gönderimi ve assignment statülerinin beklenmedik şekilde ilerlemesi mümkün.  
**Öneri:** Kontrolü UI'da değil servis katmanında zorunlu kılın; uygun olmayan projelerde `BusinessException` dönün ve butonları yalnızca eligible projelerde gösterin.

### 🟠 [Mimari/İş Mantığı] — Proje bazlı saat ayarı UI'da var, worker tarafında fiilen yok

**Dosya:** `src/PulsePoll.Admin/Components/Pages/Projects/Projects.razor`, `src/PulsePoll.Worker/Services/SurveyDistributionJobScheduler.cs`  
**Satır:** 272-287, 24-38, 42-45  
**Sorun:** Admin ekranı proje bazlı başlangıç/bitiş saati alıyor; fakat worker tüm projeler için sabit `09-19` dağıtım ve `10:00` reminder cron'u kuruyor. Ayrıca `Europe/Istanbul` bulunamazsa sessizce `TimeZoneInfo.Local`'a düşüyor.  
**Etki:** 08:00, 20:00 gibi geçerli görünen saatler asla çalışmaz; host timezone farklıysa işler sessizce yanlış saatte koşar.  
**Öneri:** Scheduler'ı daha sık tetikleyip proje saat filtresini serviste uygulatın veya per-project job üretin. Timezone fallback yerine fail-fast/log+health check kullanın.

### 🟠 [Doğrulama/Veri] — Eski projeler 00:00-00:00 pencereye düşebiliyor

**Dosya:** `src/PulsePoll.Infrastructure/Persistence/Migrations/20260308164048_AddScheduledDistribution.cs`, `src/PulsePoll.Admin/Components/Pages/Projects/Projects.razor`, `src/PulsePoll.Application/Services/ProjectService.cs`, `src/PulsePoll.Application/Services/DistributionService.cs`  
**Satır:** 15-27, 421-454, 531-589, 94-125, 39-46  
**Sorun:** Migration mevcut kayıtlar için saat alanlarını `00:00` ile dolduruyor. Edit popup bu değerleri olduğu gibi forma taşıyor; `IsScheduledDistribution` açıldığında `start < end` veya "sıfır saat geçersiz" doğrulaması yok.  
**Etki:** Eski bir projede checkbox açılıp kaydedildiğinde dağıtım penceresi `00:00-00:00` kalabilir; job pratikte hiç çalışmaz.  
**Öneri:** Migration/backfill ile `09:00-19:00` verin, validator ekleyin, edit formunda sıfır saatleri güvenli default'a çevirin.

### 🟡 [Performans] — Dağıtım ekranı sayaç için tüm assignment grafiğini belleğe çekiyor

**Dosya:** `src/PulsePoll.Application/Services/DistributionService.cs`, `src/PulsePoll.Infrastructure/Persistence/Repositories/ProjectRepository.cs`  
**Satır:** 167-188, 71-77  
**Sorun:** İlerleme kartları için sadece status sayıları gerekirken servis `GetAssignmentsByProjectAsync` ile tüm assignment kayıtlarını ve `Subject/City/SocioeconomicStatus` ilişkilerini çekiyor.  
**Etki:** Büyük projelerde admin dağıtım ekranı gereksiz IO ve bellek tüketir.  
**Öneri:** Status bazlı aggregate repository sorgusu ekleyin; detay listesi ile özet sayaç sorgularını ayırın.

### 🟡 [Hata Yönetimi] — Admin UI ham exception mesajını kullanıcıya gösteriyor

**Dosya:** `src/PulsePoll.Admin/Components/Pages/Projects/ProjectDistribution.razor`  
**Satır:** 175-188, 195-206, 214-224  
**Sorun:** `ex.Message` doğrudan toast'a basılıyor.  
**Etki:** İngilizce/teknik hata mesajları UI'a sızar; iç detaylar açığa çıkabilir ve hata deneyimi tutarsızlaşır.  
**Öneri:** Detayı loglayın, kullanıcıya standart Türkçe hata mesajı verin; mevcut `ErrorHandler` yaklaşımını burada da kullanın.

### 🔵 [UI Tutarlılığı] — Kaldırma metni yeni `Scheduled` durumunu yansıtmıyor

**Dosya:** `src/PulsePoll.Admin/Components/Pages/Projects/ProjectSubjects.razor`  
**Satır:** 567-572, 911-920  
**Sorun:** Kod artık `Scheduled` atamaları da kaldırıyor, ama popup/warning metni hâlâ yalnızca "başlanmamış" diyor.  
**Etki:** Operasyon ekranı yanlış yönlendirici olur.  
**Öneri:** Metinleri "başlanmamış veya zamanlanmış" şeklinde güncelleyin.

## Türkçe Karakter Denetimi

Bu branchta kullanıcıya görünen yeni/değişmiş metinlerde bozuk ASCII Türkçe karakter bulgusu tespit edilmedi.

## Doğrulama

- `dotnet test PulsePoll.sln` çalıştı.
- Test sonucu: `26/26` başarılı.
- Mobil proje restore/build sırasında Android JDK yolu için ayrı bir uyarı üretildi; bu branch bulgularından bağımsız görünüyor.

## Özet Tablo

| # | Seviye | Kategori | Dosya | Kısa Açıklama |
|---|--------|----------|-------|---------------|
| 1 | 🟠 | İş Mantığı | `DistributionService.cs` | Manuel akış aktif olmayan projeleri de tetikleyebiliyor |
| 2 | 🟠 | Mimari/İş Mantığı | `SurveyDistributionJobScheduler.cs` | Proje bazlı saat ayarı worker'da yok, cron sabit |
| 3 | 🟠 | Doğrulama/Veri | `AddScheduledDistribution.cs` | Eski projeler 00:00-00:00 ile kaydedilebilir |
| 4 | 🟡 | Performans | `DistributionService.cs` | Sayaç için tüm assignment grafiği yükleniyor |
| 5 | 🟡 | Hata Yönetimi | `ProjectDistribution.razor` | Ham exception mesajı UI'a basılıyor |
| 6 | 🔵 | UI Tutarlılığı | `ProjectSubjects.razor` | "Başlanmamış" metni `Scheduled` durumunu kapsamıyor |

## Aksiyon Planı

1. Önce servis katmanına `Active + IsScheduledDistribution + valid window` guard'larını ekleyin.
2. Ardından migration/backfill ve validator ile saat verisini güvenli hale getirin.
3. Sonra scheduler stratejisini proje ayarlarını gerçekten uygulayacak şekilde düzeltin.
4. Son aşamada dağıtım ekranının aggregate sorgularını ve UI hata mesajlarını toparlayın.
