# WebView Survey Helper — Teknik Plan

## 1. Konsept

PulsePoll uygulamasında kullanıcı bir anketi WebView içinde açar. Sistem, kullanıcıya hangi soruya ne cevap vermesi gerektiğini gösterir. İki mod desteklenir:

- **Manuel Mod:** Kullanıcı "?" butonuna basar, sistem o anki soruyu okur ve yardım gösterir.
- **Otomatik Mod:** Sayfa her değiştiğinde sistem otomatik olarak soruyu algılar ve yardım metnini gösterir.

Her iki modda da akış aynıdır: DOM'dan soru metnini oku → veritabanında eşleştir → sonucu göster.

---

## 2. İki Modun Karşılaştırması

| | Manuel Mod (? Butonu) | Otomatik Mod |
|---|---|---|
| **Tetikleme** | Kullanıcı butona basar | Sayfa değişiminde otomatik |
| **Kullanıcı deneyimi** | Kullanıcı kontrolünde, istediğinde bakar | Hands-free, her soruda otomatik bilgi gelir |
| **Teknik karmaşıklık** | Düşük — tek seferlik JS çalıştırma | Orta — MutationObserver veya polling gerekir |
| **Sayfa değişim algılama** | Gerekmez | Gerekir (URL değişimi + DOM değişimi) |
| **Anket platformu bağımlılığı** | Düşük — butona basıldığında DOM okur | Yüksek — her platform farklı DOM güncelleme yapıyor |
| **Performans etkisi** | Yok | Düşük ama sürekli dinleme var |
| **Önerilen kullanım** | Varsayılan mod | İleri kullanıcılar, tekrarlı anketler |

**Öneri:** Manuel mod varsayılan olsun, otomatik mod ayarlardan açılabilsin.

---

## 3. Genel Layout

```
┌──────────────────────────────┐
│  Anket Adı            [?][⚙] │  ← Native toolbar
├──────────────────────────────┤
│                              │
│          WebView             │
│     (Anket Sayfası)          │
│                              │
│                              │
├──────────────────────────────┤
│ 💡 İstanbul seçin        [X] │  ← Helper banner (otomatik mod)
└──────────────────────────────┘
```

---

## 4. DOM'dan Soru Okuma (JavaScript)

WebView'a inject edilecek JS fonksiyonu her iki modu da besler.

### 4.1 Kademeli Selector Stratejisi

Anket platformları farklı HTML yapıları kullanır. Tek bir selector yerine kademeli arama:

```
Öncelik 1: Anket bazlı custom selector (veritabanından gelir)
Öncelik 2: Bilinen platform selector'ları
  → Lighthouse: .question-text, .question_body
  → Qualtrics: .QuestionText
  → SurveyMonkey: .question-title-container
  → Genel: .sg-question-title, [data-question-text]
Öncelik 3: Heuristik arama
  → h1, h2, h3, h4, legend elementleri
Öncelik 4: Fallback
  → null döner, "Soru algılanamadı" gösterilir
```

### 4.2 Temel JS Fonksiyonu

```javascript
function getQuestionText(customSelector) {
    // Anket bazlı custom selector (varsa)
    if (customSelector) {
        const el = document.querySelector(customSelector);
        if (el && el.innerText.trim().length > 3)
            return el.innerText.trim();
    }

    // Platform selector'ları
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

    for (const sel of selectors) {
        const el = document.querySelector(sel);
        if (el && el.innerText.trim().length > 3)
            return el.innerText.trim();
    }

    // Heuristik: heading elementleri
    const headings = document.querySelectorAll('h1, h2, h3, h4');
    for (const h of headings) {
        const text = h.innerText.trim();
        if (text.length > 5 && text.length < 500)
            return text;
    }

    return null;
}
```

### 4.3 Çoklu Soru Desteği (Tek Sayfada Birden Fazla Soru)

```javascript
function getAllQuestions(customSelector) {
    const selector = customSelector ||
        '.question-text, .QuestionText, .question_body, .sg-question-title';
    const elements = document.querySelectorAll(selector);
    const questions = Array.from(elements)
        .map(q => q.innerText.trim())
        .filter(t => t.length > 3);
    return JSON.stringify(questions);
}
```

### 4.4 Custom Selector Konfigürasyonu

Her anket kaydına opsiyonel bir `CustomSelector` alanı eklenir. Bu sayede yeni bir anket platformu eklendiğinde kod değişikliği gerekmez, sadece veritabanına selector yazılır.

---

## 5. Manuel Mod

### 5.1 Akış

```
Kullanıcı [?] butonuna basar
    ↓
EvaluateJavaScriptAsync("getQuestionText('customSelector')")
    ↓
Dönen soru metni C#'a gelir
    ↓
Eşleştirme servisi çağrılır
    ↓
Eşleşme var  → Popup/BottomSheet ile yardım metni gösterilir
Eşleşme yok → "Bu soru için yardım bulunamadı" gösterilir
Soru null   → "Soru algılanamadı, sayfanın yüklenmesini bekleyin"
```

### 5.2 C# Tarafı (Pseudo)

```csharp
private async void OnHelpButtonClicked(object sender, EventArgs e)
{
    var script = BuildQuestionScript(currentSurvey.CustomSelector);
    var questionText = await webView.EvaluateJavaScriptAsync(script);

    if (string.IsNullOrWhiteSpace(questionText))
    {
        await ShowPopup("Soru algılanamadı.");
        return;
    }

    var helpEntry = await _surveyHelpService.FindMatch(
        currentSurvey.Id, questionText);

    if (helpEntry != null)
        await ShowPopup(helpEntry.HelpText);
    else
        await ShowPopup("Bu soru için yardım bulunamadı.");
}
```

### 5.3 Gösterim: Popup

```
┌──────────────────────────┐
│                          │
│  ╔══════════════════╗    │
│  ║  💡 Yardım       ║    │
│  ║                  ║    │
│  ║  İstanbul        ║    │
│  ║  seçin           ║    │
│  ║                  ║    │
│  ║        [Tamam]   ║    │
│  ╚══════════════════╝    │
│                          │
└──────────────────────────┘
```

---

## 6. Otomatik Mod

### 6.1 Sayfa Değişimini Algılama

İki mekanizma paralel çalışır (anket platformuna göre biri veya ikisi devreye girer):

**A — URL Değişimi (Klasik navigasyon)**

Her soru ayrı URL'ye sahipse `Navigated` event yeterlidir.

```csharp
webView.Navigated += async (s, e) =>
{
    if (isAutoModeEnabled)
        await AutoDetectAndShowHelp();
};
```

**B — DOM Değişimi (SPA tarzı anketler)**

URL değişmeden DOM güncellenen anketler için MutationObserver inject edilir:

```javascript
function startAutoDetect() {
    let lastQuestion = '';
    let debounceTimer = null;

    const observer = new MutationObserver(() => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            const current = getQuestionText();
            if (current && current !== lastQuestion) {
                lastQuestion = current;
                window.location.href = 'surveyhelper://' +
                    encodeURIComponent(current);
            }
        }, 500); // 500ms debounce
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true,
        characterData: true
    });
}
```

C# tarafında custom scheme yakalanır:

```csharp
webView.Navigating += (s, e) =>
{
    if (e.Url.StartsWith("surveyhelper://"))
    {
        e.Cancel = true;
        var questionText = Uri.UnescapeDataString(
            e.Url.Replace("surveyhelper://", ""));
        _ = ProcessAutoHelp(questionText);
    }
};
```

### 6.2 Gösterim: Sabit Banner

Otomatik modda popup yerine kalıcı banner daha uygun — akışı bozmaz:

- WebView'ın altında sabit bir Label/Frame.
- Soru değiştiğinde banner metni güncellenir.
- Eşleşme yoksa banner gizlenir.
- [X] butonu ile o soru için banner kapatılabilir.
- Banner'a dokunulursa detaylı popup açılır (uzun yardım metinleri için).

---

## 7. Eşleştirme Mantığı

Kayıtlar SortOrder'a göre sıralanır, ilk eşleşen kazanır:

| MatchType | Mantık | Örnek |
|-----------|--------|-------|
| **Contains** | Soru metni keyword'ü içeriyor mu? | keyword: "hangi il" → "Hangi ilde ikamet ediyorsunuz?" ✓ |
| **Exact** | Birebir eşleşme (normalize edilmiş) | Nadiren gerekir |
| **Regex** | Pattern eşleşmesi | `yaş.*nız` → "Yaşınız nedir?", "Yaşınız kaçtır?" ✓ |

**Pratik notlar:**

- Contains çoğu durum için yeterli.
- Keyword'ler lowercase normalize edilmeli (Türkçe: `ToLower(new CultureInfo("tr-TR"))`).
- Çok genel keyword'lerden kaçınılmalı ("ne", "bir" gibi her şeyi eşleştirir).
- Birden fazla eşleşmede SortOrder düşük olan (daha spesifik) kazanır.

---

## 8. Edge Case'ler

### Dinamik Yükleme / Geç Gelen İçerik

Bazı anketler soruları AJAX ile yükler. Manuel modda butona basıldığında DOM henüz hazır olmayabilir:

- İlk denemede null dönerse 1-2 saniye bekleyip tekrar dene.
- Otomatik modda MutationObserver bu durumu zaten halleder (debounce ile).

### Birden Fazla Soru Tek Sayfada

`getAllQuestions()` fonksiyonu kullanılır. Banner'da liste halinde gösterilir:

```
💡 1. İstanbul seçin
   2. 35-55 arası girin
   3. Serbest meslek seçin
```

### Eşleşme Hassasiyeti

Aynı anlama gelen farklı soru ifadeleri olabilir ("Yaşınız nedir?" / "Yaşınız kaçtır?" / "Doğum yılınız?"). Çözüm:

- Aynı yardım metni için birden fazla keyword kaydı oluşturulabilir.
- Regex ile esnek pattern tanımlanabilir.

### WebView Debug (Geliştirme Sırasında)

DOM yapısını incelemek için:

- Android: `chrome://inspect` ile remote debugging.
- iOS: Safari Web Inspector.
- MAUI'de: `WebView.EnableWebDevTools()` aktif edilmeli.

---

## 9. Uygulama Adımları

### Faz 1 — Manuel Mod (MVP)

1. WebView sayfasına native "?" butonu ekle (toolbar'da).
2. `getQuestionText()` JS fonksiyonunu yaz.
3. Hedef anket platformunda DOM yapısını incele, doğru selector'ı bul.
4. `EvaluateJavaScriptAsync` ile soruyu oku.
5. Eşleştirme servisini yaz (Contains tabanlı).
6. Popup ile yardım metnini göster.
7. End-to-end test.

### Faz 2 — Otomatik Mod

1. MutationObserver JS'ini yaz.
2. Custom URL scheme bridge'i kur (`surveyhelper://`).
3. Banner UI bileşenini ekle.
4. Debounce mekanizmasını ekle.
5. `Navigated` event handler'ı ekle (URL değişimi için).
6. Ayarlara Manuel/Otomatik mod switch'i ekle.

### Faz 3 — İyileştirmeler

1. Anket bazlı custom selector desteği.
2. Tek sayfada çoklu soru desteği (`getAllQuestions`).
3. Admin paneline yardım verisi CRUD ekranı.
4. Eşleşme istatistikleri (hangi sorularda yardım bulunamadı → eksik veri tespiti).
