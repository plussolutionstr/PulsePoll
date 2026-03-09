# Survey Helper Manuel Mod Uygulama Planı

Bu doküman, [survey-helper-plan.md](/Users/ilhancakmak/RiderProjects/PulsePoll/docs/survey-helper-plan.md) içindeki genel yaklaşımı PulsePoll kod tabanına somut şekilde uyarlamak için hazırlanmıştır.

Odak:

- İlk sürümde sadece manuel mod olacak.
- "Özel denek" tarafı teknik olarak var kabul edilecek.
- Bu durum API/mobile contract'ta `IsSurveyHelperEnabled` capability alanı ile temsil edilecek.
- Ayrı bir ikinci WebView sayfası açılmayacak.
- Mevcut `SurveyWebViewPage` tek giriş noktası olarak kalacak.

## 1. Temel Kararlar

### 1.1 Tek WebView, Çift Davranış

Mevcut anket akışı zaten tek bir sayfadan ilerliyor:

- `SurveyDetailViewModel` anket URL'sini çözüyor
- `surveywebview` route'una gidiyor
- `SurveyWebViewPage` içinde WebView yükleniyor
- `SurveyWebViewViewModel` sonuç URL'lerini yakalıyor

Bu yüzden iki ayrı ekran yerine tek ekran içinde modlu davranış tercih edilecek:

- Normal denek: standart WebView deneyimi
- Helper yetkili denek: aynı ekran + `?` yardım butonu

Bu kararın gerekçesi:

- Navigation akışı tek yerde kalır
- `TryHandleSurveyResultUrlAsync` mantığı ikiye bölünmez
- Loading, back, error ve result handling davranışı kopyalanmaz
- Otomatik moda geçiş gerekirse aynı ekran genişletilir

### 1.2 Capability Adı

Domain tarafında iş kuralı "özel denek" olabilir. Ancak mobile/API contract tarafında capability adı kullanılacak:

- `IsSurveyHelperEnabled`

Bu tercih daha doğru çünkü:

- İş anlamı yerine ürün yeteneğini temsil eder
- İleride bu özellik özel denek dışındaki gruplara da açılabilir
- Kod, segment adı yerine davranış adı üzerinden okunur

## 2. Varsayımlar

Bu plan aşağıdaki varsayımlarla hazırlanmıştır:

- `Subject` üzerinde helper yetkisini belirleyen bir flag vardır veya eklenecektir
- Backend bu flag'i profile cevabında dönebilir
- İlk sürümde helper sadece manuel modda çalışacaktır
- İlk sürümde survey bazlı destek matrisi zorunlu değildir
- İlk sürümde yardım içeriği mock/local olabilir; gerçek veritabanı eşleştirmesi ikinci adımda bağlanabilir

İlk sürümde görünürlük kuralı:

`helper görünür mü = IsSurveyHelperEnabled`

İkinci sürüm için önerilen kural:

`helper görünür mü = IsSurveyHelperEnabled && Survey.SupportsHelper`

## 3. Hedef Akış

1. Kullanıcı profil veya session yüklenirken `IsSurveyHelperEnabled` alınır.
2. Kullanıcı anket detay ekranından "Başla" der.
3. `SurveyDetailViewModel`, mevcut route'a helper bilgisini de ekler.
4. `SurveyWebViewPage` açılır.
5. Eğer helper aktifse header'da `?` butonu görünür.
6. Kullanıcı yardıma ihtiyaç duyduğunda `?` butonuna basar.
7. Native taraf WebView içinde JS çalıştırır ve mevcut soruyu okumaya çalışır.
8. Soru metni bulunursa helper servisi eşleşme arar.
9. Sonuç popup veya alert olarak gösterilir.
10. Sonuç bulunamazsa kullanıcıya kısa bir fallback mesajı gösterilir.

## 4. Uygulama Tasarımı

### 4.1 Backend Contract

Backend tarafında profile cevabına yeni alan eklenecek:

- Dosya: `src/PulsePoll.Application/DTOs/SubjectDto.cs`
- Yeni alan: `bool IsSurveyHelperEnabled`

Bu alan `SubjectService` içinde maplenecek:

- Dosya: `src/PulsePoll.Application/Services/SubjectService.cs`

Profile endpoint zaten `SubjectDto` döndürüyor:

- Dosya: `src/PulsePoll.Api/Controllers/ProfileController.cs`

Dolayısıyla profile response zinciri bozulmadan genişletilebilir.

Not:

- Bu ilk sürümde ayrı bir endpoint gerekmez.
- `GET /api/profile` helper capability taşıyan ana kaynak olabilir.

### 4.2 Mobile API Modeli

Mobil tarafta profile DTO'ya karşılık gelen model genişletilecek:

- Dosya: `src/PulsePoll.Mobile/ApiModels/Dtos.cs`
- Record: `ProfileApiDto`
- Yeni alan: `bool IsSurveyHelperEnabled`

Bu sayede mobile taraf capability bilgisini güvenli biçimde deserialize eder.

### 4.3 Session / State Yönetimi

Bu bilgi her seferinde yeniden profil çekilerek değil, hafif bir session state ile taşınmalıdır.

Önerilen yapı:

- Yeni servis: `ISubjectSessionService`
- Yeni implementasyon: `SubjectSessionService`
- Klasör: `src/PulsePoll.Mobile/Services/`

Sorumluluklar:

- `IsSurveyHelperEnabled` bilgisini RAM'de tutmak
- Gerekirse `Preferences` ile hafif cache yapmak
- Profil yüklendiğinde session state'i güncellemek
- Logout durumunda temizlemek

Önerilen minimal arayüz:

```csharp
public interface ISubjectSessionService
{
    bool IsSurveyHelperEnabled { get; }
    void UpdateHelperCapability(bool isEnabled);
    void Clear();
}
```

DI kaydı:

- Dosya: `src/PulsePoll.Mobile/MauiProgram.cs`

Not:

- İlk prototipte direkt `Preferences` da kullanılabilir.
- Ancak `SurveyDetailViewModel` ve `ProfileViewModel` arasında temiz paylaşım için küçük bir session service daha doğru olur.

### 4.4 Profile Yükleme Noktası

Profil zaten mobilde yükleniyor:

- Dosya: `src/PulsePoll.Mobile/ViewModels/ProfileViewModel.cs`

Burada yapılacak iş:

- `ProfileApiDto` içinden `IsSurveyHelperEnabled` okunur
- Session service güncellenir

İsteğe bağlı ek güçlendirme:

- Login sonrası veya app açılışında profile fetch yapılıyorsa aynı yerde de güncellenebilir
- Ancak ilk sürüm için `ProfileViewModel` üzerinden beslemek yeterlidir

Risk:

- Kullanıcı profile ekranına hiç girmeden anket başlatırsa helper bilgisi boş olabilir

Bu yüzden daha sağlam yaklaşım:

- Girişten sonra veya home/surveys ilk yüklemesinde profile/session bilgisi alınmalı

İlk sprint için pratik çözüm:

- Session service, default `false` başlasın
- Helper ilk etapta test kullanıcılarında kontrollü açılacağı için bu kabul edilebilir

### 4.5 Survey Başlatma Akışı

Mevcut anket başlatma noktası:

- Dosya: `src/PulsePoll.Mobile/ViewModels/SurveyDetailViewModel.cs`

Burada route genişletilecek:

Bugün:

```text
surveywebview?projectId=...&title=...&url=...
```

Yeni:

```text
surveywebview?projectId=...&title=...&url=...&helperEnabled=true
```

`SurveyDetailViewModel` sorumluluğu:

- Session service'ten helper capability'yi almak
- Route'a `helperEnabled` query parametresi olarak eklemek

Not:

- Bu bilgi route ile taşınırsa `SurveyWebViewPage` ekranı self-contained kalır
- Shell navigation sırasında bu modun açıkça görülebilmesi debugging'i kolaylaştırır

### 4.6 Survey WebView ViewModel

Mevcut dosya:

- `src/PulsePoll.Mobile/ViewModels/SurveyWebViewViewModel.cs`

Eklenecek alanlar:

- `bool IsHelperEnabled`
- `bool IsHelpBusy`
- `string CurrentQuestionText`

Eklenecek query binding:

```csharp
[QueryProperty(nameof(IsHelperEnabledRaw), "helperEnabled")]
```

`Shell` query parametreleri string geldiği için parse için raw string property kullanmak daha güvenlidir.

Örnek yaklaşım:

```csharp
[ObservableProperty] private string _isHelperEnabledRaw = "false";

public bool IsHelperEnabled =>
    bool.TryParse(IsHelperEnabledRaw, out var value) && value;
```

Alternatif:

- Raw property değiştiğinde `OnPropertyChanged(nameof(IsHelperEnabled))`

Eklenecek command:

- `AskForHelpCommand`

Sorumluluklar:

- Mevcut WebView üzerinden JS çalıştırmak
- Soru metnini almak
- Helper servisini çağırmak
- Sonucu UI'a göstermek

Önemli not:

Mevcut ViewModel'de WebView referansı yok. Bu yüzden iki seçenek vardır:

1. JS çağrısını code-behind'dan yapıp sonucu ViewModel'e vermek
2. WebView abstraction eklemek

İlk sürüm için daha pragmatik seçim:

- JS evaluation code-behind'da kalsın
- Elde edilen question text ViewModel'e veya doğrudan helper servis akışına verilsin

Bu yaklaşım daha az altyapı değiştirir.

### 4.7 Survey WebView UI

Mevcut ekran:

- `src/PulsePoll.Mobile/Views/SurveyWebViewPage.xaml`
- `src/PulsePoll.Mobile/Views/SurveyWebViewPage.xaml.cs`

Header düzeni bugün:

- sol: back
- orta: title
- sağ: loading indicator

Yeni düzen:

- sol: back
- orta: title
- sağ grup: `?` butonu + loading indicator

Davranış:

- `IsHelperEnabled == true` ise `?` butonu görünür
- `IsHelperEnabled == false` ise görünmez

Buton aksiyonu:

- `OnHelpButtonClicked`
- `EvaluateJavaScriptAsync(...)`
- soru dönerse popup
- soru dönmezse fallback mesaj

İlk sürüm için popup tercihi:

- `DisplayAlert` yeterlidir
- Ayrı bir custom popup/bottom sheet ikinci adımda alınabilir

### 4.8 JavaScript Stratejisi

İlk sürümde JS gömülü ve sade tutulmalı.

Önerilen fonksiyon:

```javascript
function getQuestionText(customSelector) {
    if (customSelector) {
        const custom = document.querySelector(customSelector);
        if (custom && custom.innerText && custom.innerText.trim().length > 3)
            return custom.innerText.trim();
    }

    const selectors = [
        '.question-text',
        '.question_body',
        '.QuestionText',
        '.question-title-container',
        '.sg-question-title',
        '[data-question-text]',
        'legend',
        '.question h2',
        '.question h3'
    ];

    for (const selector of selectors) {
        const el = document.querySelector(selector);
        if (el && el.innerText && el.innerText.trim().length > 3)
            return el.innerText.trim();
    }

    const headings = document.querySelectorAll('h1, h2, h3, h4');
    for (const heading of headings) {
        const text = heading.innerText ? heading.innerText.trim() : '';
        if (text.length > 5 && text.length < 500)
            return text;
    }

    return null;
}
```

İlk sürüm kararı:

- JS script tek bir helper class veya sabit method üzerinden üretilecek
- `customSelector` desteği ikinci adım için hazır bırakılabilir
- Ancak MVP'de sabit selector listesiyle başlanabilir

Önerilen yeni dosya:

- `src/PulsePoll.Mobile/Services/SurveyHelperScriptFactory.cs`

Amaç:

- JS string üretimini XAML/code-behind içinden ayırmak
- İleride otomatik mod JS'ini de aynı yerde toplamak

### 4.9 Helper İçerik Servisi

İlk sürümde en kritik risk DOM okuma olduğundan, yardım verisi kısmı basit tutulmalıdır.

Önerilen servis:

- `ISurveyHelperService`
- `SurveyHelperService`
- Klasör: `src/PulsePoll.Mobile/Services/`

İlk sürüm sorumlulukları:

- Soru metnini normalize etmek
- Basit `Contains` tabanlı local eşleşme yapmak
- Yardım metni veya fallback döndürmek

Önerilen minimal model:

```csharp
public record SurveyHelpMatchResult(bool Found, string Message);
```

İlk sürüm veri kaynağı:

- in-memory liste
- sabit test verisi

Örnek:

```csharp
[
    ("hangi il", "Istanbul seçin"),
    ("yas", "35-55 arasi girin")
]
```

Bu bilinçli bir tercih olmalı:

- Önce DOM okumanın gerçekten çalıştığı kanıtlanır
- Sonra backend/veritabanı eşleştirmesi bağlanır

## 5. Dosya Bazlı Implementasyon Checklist'i

### 5.1 Backend

`src/PulsePoll.Application/DTOs/SubjectDto.cs`

- `IsSurveyHelperEnabled` alanını ekle

`src/PulsePoll.Application/Services/SubjectService.cs`

- `BuildDto(...)` içinde helper capability alanını map et
- Kaynak olarak subject üzerindeki özel flag'i kullan

`src/PulsePoll.Api/Controllers/ProfileController.cs`

- Doğrudan ek değişiklik gerekmeyebilir
- Endpoint'in genişlemiş DTO'yu döndüğü doğrulanmalı

### 5.2 Mobile API ve State

`src/PulsePoll.Mobile/ApiModels/Dtos.cs`

- `ProfileApiDto` içine `IsSurveyHelperEnabled` ekle

`src/PulsePoll.Mobile/Services/ISubjectSessionService.cs`

- Yeni arayüz oluştur

`src/PulsePoll.Mobile/Services/SubjectSessionService.cs`

- Yeni implementasyon oluştur

`src/PulsePoll.Mobile/MauiProgram.cs`

- Session service DI kaydı ekle
- Helper service DI kaydı ekle

### 5.3 Mobile Profile / Survey Flow

`src/PulsePoll.Mobile/ViewModels/ProfileViewModel.cs`

- Profile yüklenince helper capability bilgisini session service'e yaz

`src/PulsePoll.Mobile/ViewModels/SurveyDetailViewModel.cs`

- Constructor'a session service enjekte et
- `StartSurvey()` içinde `helperEnabled` query parametresi ekle

### 5.4 WebView Screen

`src/PulsePoll.Mobile/ViewModels/SurveyWebViewViewModel.cs`

- `helperEnabled` query property ekle
- `IsHelperEnabled` hesapla
- helper state alanlarını ekle

`src/PulsePoll.Mobile/Views/SurveyWebViewPage.xaml`

- Header'a `?` yardım butonu ekle
- Görünürlüğü `IsHelperEnabled` ile bağla

`src/PulsePoll.Mobile/Views/SurveyWebViewPage.xaml.cs`

- `OnHelpButtonClicked` ekle
- `EvaluateJavaScriptAsync` ile mevcut soruyu al
- sonucu helper servis akışına gönder
- popup/fallback mesajlarını göster

### 5.5 Helper Altyapısı

`src/PulsePoll.Mobile/Services/ISurveyHelperService.cs`

- Yeni arayüz oluştur

`src/PulsePoll.Mobile/Services/SurveyHelperService.cs`

- normalize + local match mantığını yaz

`src/PulsePoll.Mobile/Services/SurveyHelperScriptFactory.cs`

- `getQuestionText()` script üretimini merkezileştir

## 6. Önerilen Sprint Sırası

### Faz 1: Capability Akışı

Amaç:

- Helper yetkili deneği mobile tarafında ayırt edebilmek

Adımlar:

1. Backend DTO'ya `IsSurveyHelperEnabled` ekle
2. Mobile `ProfileApiDto` genişlet
3. Session service oluştur
4. Profile yüklenince capability'yi cache'le

Çıkış kriteri:

- Test kullanıcısında helper capability okunabiliyor olmalı

### Faz 2: WebView UI ve Routing

Amaç:

- Yardım modu WebView ekranında görünür hale gelsin

Adımlar:

1. `SurveyDetailViewModel` route'a `helperEnabled` eklesin
2. `SurveyWebViewViewModel` query parametresini parse etsin
3. `SurveyWebViewPage` üzerinde `?` butonu koşullu görünsün

Çıkış kriteri:

- Flag'li kullanıcı `?` butonunu görmeli
- Flagsiz kullanıcı görmemeli

### Faz 3: Manuel Yardım Çağrısı

Amaç:

- `?` butonuna basınca soru okunup cevap gösterilsin

Adımlar:

1. JS script factory ekle
2. `OnHelpButtonClicked` yaz
3. `EvaluateJavaScriptAsync` ile soru al
4. Local helper service ile eşleştir
5. Popup/fallback mesajı göster

Çıkış kriteri:

- Desteklenen test survey'sinde butona basınca yardım çıkmalı

### Faz 4: Sertleştirme

Amaç:

- Gerçek cihaz ve gerçek survey davranışlarını stabilize etmek

Adımlar:

1. Null dönen durumda kısa retry ekle
2. Seçici yanlış eşleşmelerini gözden geçir
3. Uzun soru metinlerinde normalize et
4. Gerekirse per-survey custom selector tasarımını başlat

Çıkış kriteri:

- Hedef survey platformunda manuel mod güvenilir çalışmalı

## 7. Riskler ve Önlemler

### 7.1 Profil Yüklenmeden Ankete Girilmesi

Risk:

- Helper capability session'a daha yazılmamış olabilir

Önlem:

- Session default `false`
- İkinci adımda app bootstrap veya login sonrası profile sync

### 7.2 DOM'un Geç Yüklenmesi

Risk:

- Kullanıcı `?` butonuna çok erken basarsa `null` dönebilir

Önlem:

- İlk sürümde kullanıcıya "sayfanın yüklenmesini bekleyin" mesajı ver
- Gerekirse 500ms-1000ms tek retry ekle

### 7.3 Selector Yanlış Soru Okuması

Risk:

- Heading fallback farklı bir başlığı okuyabilir

Önlem:

- İlk canlı denemeyi tek bir survey platformu ile yap
- Gerekirse hedef survey için custom selector sabitle

### 7.4 Yardım Eşleşmesinin Zayıf Kalması

Risk:

- DOM okunsa bile doğru yardım gelmeyebilir

Önlem:

- İlk sprintte eşleşme kalitesi ana hedef değildir
- Önce soru okuma başarısı doğrulansın

## 8. Kabul Kriterleri

Bu planın MVP çıktısı aşağıdaki kriterleri sağlamalı:

- Helper yetkili kullanıcı için `?` butonu görünür
- Yetkisiz kullanıcı için `?` butonu görünmez
- `?` butonuna basılınca WebView içinden soru metni okunur
- Eşleşme varsa yardım popup olarak gösterilir
- Eşleşme yoksa kullanıcı anlamlı fallback mesajı görür
- Mevcut survey result handling akışı bozulmaz
- Ayrı ikinci WebView sayfası oluşmaz

## 9. Sonraki Adımlar

MVP sonrasında mantıklı genişleme sırası:

1. `Survey.SupportsHelper` veya benzeri survey-level capability
2. `CustomSelector` desteği
3. Gerçek backend/veritabanı destekli yardım kayıtları
4. Eşleşme istatistikleri
5. Otomatik mod

## 10. Net Tavsiye

İlk deneme için doğru rota şudur:

- Tek WebView sayfası
- Subject bazlı helper capability
- Manuel `?` butonu
- Local/mock yardım servisi
- Gerçek survey üzerinde DOM okuma doğrulaması

En kritik kanıt şudur:

- "Özel denek" ayrımı değil
- WebView içindeki gerçek survey DOM'undan doğru sorunun güvenilir şekilde okunabilmesi

Bu kanıt alındıktan sonra sistem rahatça veri tabanlı ve otomatik moda genişletilebilir.
