# Denek Değerleme (5 Yıldız) Tasarımı

## Amaç

Denekleri, atama ve davranış verilerinden üretilen tek bir puanla sıralamak:

- Projeye doğru deneği daha hızlı atamak
- Düşük kaliteli/aktif olmayan denekleri erken görmek
- Operasyonel kararları sadeleştirmek (`1★` - `5★`)

## Neden 5 Yıldız?

Bu model doğru:

- Operasyon ekibi için daha hızlı okunur (A/B/C gibi sınıf ezberi gerektirmez)
- UI’da badge/ikonla anlaşılır gösterim verir
- Filtreleme kolay olur (örn. `>= 4★`)

## Skor Modeli

`NihaiSkor (0-100) = TemelSkor x AktiviteÇarpanı`

- `TemelSkor`: atama/sonuç kalitesi
- `AktiviteÇarpanı`: yakın dönem uygulama aktivitesi

### 1) TemelSkor (0-100)

Önerilen alt metrikler ve ağırlıklar:

1. Katılım (`%25`)
2. Tamamlama (`%30`)
3. Kalite (`%20`)
4. Onay Güveni (`%15`)
5. Hız (`%10`)

#### Ham metrikler

- `totalAssignments`
- `notStarted`
- `started = totalAssignments - notStarted`
- `completed`
- `partial`
- `disqualify`
- `screenOut`
- `quotaFull`
- `rewardApproved`
- `rewardRejected`
- `medianCompletionMinutes`

#### Normalize edilmiş metrikler (0-1)

- `participation = started / max(1, totalAssignments)`
- `completion = completed / max(1, started)`
- `qualityPenalty = (1.0*disqualify + 0.8*screenOut + 0.6*quotaFull + 0.4*partial) / max(1, started)`
- `quality = clamp(0, 1, 1 - qualityPenalty)`
- `approvalTrust = rewardApproved / max(1, rewardApproved + rewardRejected)`
- `speed = clamp(0, 1, (60 - medianCompletionMinutes) / 50)`  
  Not: `10 dk => ~1`, `60 dk+ => 0`

#### Temel skor formülü

`coreRaw = 100 * (0.25*participation + 0.30*completion + 0.20*quality + 0.15*approvalTrust + 0.10*speed)`

Küçük örneklem dalgalanmasını azaltmak için güven katsayısı:

- `confidence = min(1, totalAssignments / 20.0)`
- `coreScore = 60 + (coreRaw - 60) * confidence`

Bu sayede az atamalı yeni denekler aşırı şişmez/çökmez.

### 2) Aktivite Çarpanı (0.85 - 1.15)

İdeal veri: gerçek app session event’i (open/close).  
Geçiş aşaması: `refresh_tokens.created_at` ile aktif gün ve recency türetilebilir.

Öneri:

- `activeDays30`: son 30 gündeki aktif gün sayısı
- `lastSeenDays`: son aktiviteden beri geçen gün

Çarpan:

- `lastSeenDays <= 3 && activeDays30 >= 10` => `1.15`
- `lastSeenDays <= 7` => `1.08`
- `lastSeenDays <= 14` => `1.00`
- `lastSeenDays <= 30` => `0.93`
- `> 30` => `0.85`

### 3) Yıldız Dönüşümü

Sabit eşikler (ilk sürüm):

- `0-44` => `1★`
- `45-59` => `2★`
- `60-74` => `3★`
- `75-89` => `4★`
- `90-100` => `5★`

Not: Sonradan percentile tabanlı dinamik eşiklere geçilebilir.

## Veri Modeli Önerisi

## 1) Snapshot tablosu (önerilen)

`subject_score_snapshots`

- `id`
- `subject_id`
- `score` (0-100)
- `star` (1-5)
- `core_score`
- `activity_multiplier`
- `total_assignments`
- `started`
- `completed`
- `not_started`
- `partial`
- `disqualify`
- `screen_out`
- `quota_full`
- `reward_approved`
- `reward_rejected`
- `median_completion_minutes`
- `active_days_30`
- `last_seen_at`
- `calculated_at`

Amaç:

- Gridde hızlı gösterim
- Trend takibi (zaman içinde yıldız değişimi)
- Hesaplamayı request sırasında ağır yapmamak

## 2) (Opsiyonel, Faz-2) Session event tablosu

`subject_app_sessions`

- `id`, `subject_id`, `started_at`, `ended_at`, `platform`, `app_version`, `device_id_hash`

Bu tablo gelirse aktivite çarpanı çok daha doğru olur.

## Servis Tasarımı

`ISubjectScoreService`

- `Task<SubjectScoreDto> GetCurrentAsync(int subjectId)`
- `Task<List<SubjectScoreDto>> GetCurrentBulkAsync(IEnumerable<int> subjectIds)`
- `Task RecalculateAsync(int subjectId)`
- `Task RecalculateAllAsync()`

`SubjectScoreService`

- Atama verisini `ProjectAssignments` üzerinden toplar
- Aktiviteyi `RefreshTokens` (faz-1) veya `SubjectAppSessions` (faz-2) üzerinden hesaplar
- Snapshot’a yazar

`SubjectScoreRecalculateJob` (Worker/Cron)

- Gece 02:00 toplu hesaplama
- Ayrıca manuel tetikleme endpoint’i

## API Önerisi (Admin)

- `GET /api/admin/subjects/{id}/score`
- `GET /api/admin/subjects/scores?ids=...`
- `POST /api/admin/subjects/{id}/score/recalculate`
- `POST /api/admin/subjects/scores/recalculate-all`

## Admin UI Önerisi

### Subjects grid

- Yeni kolon: `Değer` (`★` görsel + numerik skor tooltip)
- Filtre: `Min Yıldız`
- Sıralama: varsayılan `5★ -> 1★`

### ProjectSubjects grid

- `Atama önerisi` için yıldız gösterimi
- Toplu atamada düşük yıldızlıları uyarı etiketiyle ayırma (örn. `<3★`)

### Detay popup

- Skor kırılımı (katılım/tamamlama/kalite/onay/hız)
- Son 30 gün aktivite bilgisi
- Son 90 gün yıldız trendi (opsiyonel)

## Kurallar ve Edge Case Notları

- `NotStarted` yüksekliği her zaman denek suçu değildir; katılım metriği ağırlığını çok yükseltmeyin.
- `QuotaFull` proje tasarımından kaynaklanabilir; ceza katsayısı `disqualify` kadar sert olmamalı.
- Çok yeni deneğe yıldız verirken confidence uygulanmalı.
- Manuel müdahale gerekiyorsa sadece `override_reason` ile audit’li yapılmalı (opsiyonel).

## Fazlama Planı

1. Faz-1 (hızlı kazanım): mevcut verilerle score + yıldız + admin gridde gösterim
2. Faz-2: app session telemetry ekleme ve aktivite çarpanını iyileştirme
3. Faz-3: eşik tuning (gerçek operasyon verisine göre ağırlık revizyonu)

