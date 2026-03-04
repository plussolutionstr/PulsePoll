# PulsePoll — Görsel Boyut Spesifikasyonları

## Genel Kurallar
- **Max dosya boyutu:** 5 MB
- **Desteklenen formatlar:** JPEG, PNG, GIF, WebP
- **Depolama:** MinIO `media-library` bucket
- **Retina desteği:** Görseller 2-3x çözünürlükte yüklenmeli (aşağıdaki boyutlar buna göre belirlenmiştir)

---

## Görsel Boyutları

| Alan | Önerilen Boyut | En-Boy Oranı | UI Container | Açıklama |
|------|---------------|-------------|-------------|----------|
| **Story** | 200 × 200 px | 1:1 (kare) | 60 × 60 dp daire | Daire şeklinde crop edilir. Brand/kullanıcı avatarı. |
| **Haber Banner** | 900 × 400 px | 2.25:1 (yatay) | ~375 × 160 dp | Full-width slider. Alt ~80dp gradient overlay ile text gösterilir. |
| **Anket Kartı** | 900 × 300 px | 3:1 (yatay) | ~335 × 120 dp | Proje/anket kapak görseli. Admin paneldeki kapak görseli ile aynı oran. |

---

## Notlar

### Story (200×200)
- Mobilde 60dp dairesel container içinde gösterilir
- 3x retina = 180px → 200px yuvarlak sayı olarak yeterli
- Görsel dairesel crop edileceği için kare format zorunlu
- Önemli içerik görselin merkezinde olmalı

### Haber Banner (900×400)
- CarouselView slider'da full-width gösterilir
- Alt kısımda ~80dp yüksekliğinde gradient overlay + metin var
- Önemli içerik görselin üst-orta bölgesinde olmalı, alt kısım text ile kaplanır
- Dekoratif daireler (yarı saydam) görselin üzerine bindirilir

### Anket Kartı (900×300)
- Survey card'ın üst bölgesinde banner olarak gösterilir
- Sağ üstte kazanç badge'i (₺XX) görselin üzerine bindirilir
- Backend'deki `Project.CoverMediaId` ile aynı görseli kullanır
- Admin panelde `object-fit: contain` ile gösterilir

---

## Backend Referansları
- Entity: `MediaAsset` (Domain/Entities/)
- Servis: `MediaAssetService` (Application/Services/) — 5MB limit, content type kontrolü
- Bucket: `media-library`
- Presigned URL süresi: 7 gün (604.800 saniye)
