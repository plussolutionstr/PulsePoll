# PulsePoll Admin — Grid Filtreleme & Sayfalama Planı

> **Tarih:** 2026-03-04
> **Durum:** Uygulandı
> **Kapsam:** Tüm admin panel gridleri

---

## 1. Mevcut Durum Analizi

### 1.1 Genel Mimari

| Özellik | Mevcut Durum |
|---------|-------------|
| Blazor modu | Server-side |
| Veri bağlama | Tüm gridlerde `Data="@_list"` (client-side, tüm veri belleğe yüklenir) |
| DbContext | `AddDbContext<AppDbContext>` (scoped, factory yok) |
| Filtreleme | `ShowFilterRow="true"` (18/18 grid) + DxFilterBuilder (2 grid) |
| Sayfalama | `PageSize` ile client-side (tüm veri zaten bellekte) |
| Server-side | Yok — ne `GridDevExtremeDataSource` ne `GridCustomDataSource` kullanılıyor |

### 1.2 Tüm Gridlerin Envanteri (20 Grid)

| # | Sayfa | Entity/Veri | Tahmini Hacim | Mevcut Filtre | Mevcut PageSize |
|---|-------|-------------|---------------|---------------|-----------------|
| 1 | Subjects.razor | Subject | **50K–100K** | FilterRow + DxFilterBuilder (25+ alan) | 25 |
| 2 | ProjectSubjects.razor | Subject + ProjectAssignment | **50K–100K** | FilterRow + DxFilterBuilder (22 alan) | 25 |
| 3 | WithdrawalRequests.razor | WithdrawalRequest | **Yüksek** (subject sayısıyla orantılı) | FilterRow | 25 |
| 4 | PaymentBatches.razor | PaymentBatch | Düşük (yılda ~50-100) | FilterRow | 25 |
| 5 | PaymentBatches.razor (popup) | Onaylı WithdrawalRequest | Orta-Yüksek | FilterRow | 20 |
| 6 | PaymentBatchDetail.razor | PaymentBatchItem | Düşük (paket başına ~50-200) | FilterRow | 50 |
| 7 | NotificationHistory.razor (SMS) | SmsLog | **Çok Yüksek** | FilterRow | 25 |
| 8 | NotificationHistory.razor (Push) | Notification | **Çok Yüksek** | FilterRow | 25 |
| 9 | Projects.razor | Project | Düşük (yılda ~1K) | FilterRow | 25 |
| 10 | ProjectStatement.razor | ProjectAssignment (fatura) | Düşük (proje başına) | FilterRow | 25 |
| 11 | Customers.razor | Customer | Çok Düşük (<100) | FilterRow | 25 |
| 12 | News.razor | News | Çok Düşük (<50) | FilterRow | 25 |
| 13 | Stories.razor | Story | Çok Düşük (<50) | FilterRow | 25 |
| 14 | AdminUsers.razor | AdminUser | Çok Düşük (<20) | FilterRow | 20 |
| 15 | Roles.razor | Role | Çok Düşük (<10) | FilterRow | 20 |
| 16 | SpecialDaysCalendar.razor | SpecialDay | Düşük (<500) | FilterRow | 25 |
| 17 | MessageAutomations.razor | MessageTemplate + MessageCampaign | Çok Düşük (<50) | FilterRow | 20 |
| 18 | SubjectReferrals.razor | Referral | Orta (denek detay sayfası) | FilterRow | 15 |
| 19 | SubjectProjects.razor | ProjectAssignment (denek detay) | Düşük (denek başına) | FilterRow | 15 |
| 20 | SubjectLedger.razor | WalletTransaction (export) | Orta (denek başına) | Yok (export-only) | 1000 |

### 1.3 Mevcut DxFilterBuilder Alanları (Subjects.razor)

```
FullName, PhoneNumber, Email, Age, Star, CreatedAt,
IsRetired, IsHeadOfFamily, IsHeadOfFamilyRetired,
HeadOfFamilyProfessionName, HeadOfFamilyEducationLevelName,
ReferenceCode, ReferralCode, CityName, DistrictName,
ProfessionName, EducationLevelName, BankName,
SocioeconomicStatusCode, LSMSocioeconomicStatusCode,
Status (enum), Gender (enum), MaritalStatus (enum), GsmOperator (enum)
```

---

## 2. Hedef Mimari

### 2.1 Karar: `GridDevExtremeDataSource<T>` (Büyük Tablolar İçin)

**Neden bu yaklaşım?**

| Kriter | `Data` (mevcut) | `GridDevExtremeDataSource` | `GridCustomDataSource` |
|--------|-----------------|---------------------------|------------------------|
| Kod miktarı | Az | **Çok Az** | Çok Fazla |
| Filtreleme | Client-side | **Otomatik SQL WHERE** | Manuel CriteriaOperator parse |
| Sayfalama | Client-side | **Otomatik SKIP/TAKE** | Manuel StartIndex/Count |
| Sıralama | Client-side | **Otomatik ORDER BY** | Manuel SortInfo parse |
| DxFilterBuilder uyumu | ✅ | **✅ (otomatik)** | ✅ (manuel) |
| FilterRow uyumu | ✅ | **✅ (otomatik)** | ✅ (manuel) |
| Include/Join desteği | N/A | **IQueryable üzerinden** | Manuel |
| Blazor Server uyumu | ✅ | **✅** | ✅ |
| Cell editing | ✅ | ❌ | ❌ |
| Performans (100K satır) | ❌ Bellek patlar | **✅ Sadece sayfa verisi** | ✅ Sadece sayfa verisi |

**Sonuç:** Bizim gridlerde cell editing yok (tüm düzenlemeler popup ile yapılıyor), dolayısıyla `GridDevExtremeDataSource` en uygun seçim. IQueryable veriyoruz, geri kalan her şey (filtre → SQL WHERE, sayfalama → SKIP/TAKE, sıralama → ORDER BY) otomatik.

### 2.2 Altyapı Gereksinimi: `IDbContextFactory`

Blazor Server'da `DbContext` scoped (request başına). Ancak `GridDevExtremeDataSource` kendi yaşam döngüsünü yönetir ve uzun ömürlü bir `IQueryable` tutar. Bu nedenle `IDbContextFactory<AppDbContext>` kullanılmalı.

**Değişiklik:**

```csharp
// DependencyInjection.cs — ÖNCE
services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres"))
       .UseSnakeCaseNamingConvention());

// DependencyInjection.cs — SONRA (ek satır)
services.AddDbContextFactory<AppDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres"))
       .UseSnakeCaseNamingConvention());
// NOT: AddDbContext KALACAK (repository'ler hâlâ constructor injection kullanıyor)
```

### 2.3 Grid Kategorileri

#### Kategori A — Server-Side Geçiş Gerekli (Büyük Veri)

Bu gridler `GridDevExtremeDataSource<T>` kullanacak:

| Grid | Neden |
|------|-------|
| **Subjects.razor** | 50K–100K denek, tüm veri belleğe yüklenemez |
| **ProjectSubjects.razor** | Aynı Subject tablosu + assignment bilgisi |
| **WithdrawalRequests.razor** | Subject sayısıyla orantılı, binlerce talep |
| **NotificationHistory.razor (SMS)** | Her denek × her SMS = çok yüksek hacim |
| **NotificationHistory.razor (Push)** | Her denek × her bildirim = çok yüksek hacim |

#### Kategori B — Client-Side Kalacak (Küçük Veri)

Bu gridler mevcut `Data="@_list"` ile devam edecek:

| Grid | Neden |
|------|-------|
| Projects.razor | Yılda ~1K, client-side yeterli |
| Customers.razor | Asla 1K olmaz |
| PaymentBatches.razor | Yılda ~50-100 paket |
| PaymentBatchDetail.razor | Paket başına ~50-200 satır |
| News.razor | < 50, drag-drop var (server-side desteklemez) |
| Stories.razor | < 50, drag-drop var (server-side desteklemez) |
| AdminUsers.razor | < 20 |
| Roles.razor | < 10 |
| SpecialDaysCalendar.razor | < 500 |
| MessageAutomations.razor | < 50 |
| SubjectReferrals.razor | Denek başına, düşük sayı |
| SubjectProjects.razor | Denek başına, düşük sayı |
| SubjectLedger.razor | Export-only, denek başına |
| ProjectStatement.razor | Proje başına fatura satırları |
| PaymentBatches popup | Onaylı withdrawal'lar (paket oluşturma) |

---

## 3. Detaylı Uygulama Planı

### 3.1 Adım 1 — Altyapı: IDbContextFactory Kaydı

**Dosya:** `src/PulsePoll.Infrastructure/DependencyInjection.cs`

**Değişiklik:**
```csharp
// Mevcut AddDbContext KALACAK
services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres"))
       .UseSnakeCaseNamingConvention());

// YENİ EKLEME — Blazor Server gridler için
services.AddDbContextFactory<AppDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres"))
       .UseSnakeCaseNamingConvention(), ServiceLifetime.Scoped);
```

**Etki:** Mevcut repository'ler etkilenmez. Sadece grid sayfaları `IDbContextFactory` inject edecek.

---

### 3.2 Adım 2 — Subjects.razor (Ana Denek Listesi)

**Mevcut sorun:** `GetAllAsync()` 50K-100K Subject'i Include'larıyla birlikte belleğe yüklüyor.

**Hedef:** Server-side pagination + filtreleme + mevcut DxFilterBuilder korunacak.

#### 3.2.1 Razor Değişiklikleri

```razor
@implements IDisposable
@inject IDbContextFactory<AppDbContext> ContextFactory

@code {
    private AppDbContext? _context;
    private GridDevExtremeDataSource<Subject>? _dataSource;

    protected override void OnInitialized()
    {
        _context = ContextFactory.CreateDbContext();

        var query = _context.Subjects
            .Include(s => s.City)
            .Include(s => s.District)
            .Include(s => s.Profession)
            .Include(s => s.EducationLevel)
            .Include(s => s.HeadOfFamilyProfession)
            .Include(s => s.HeadOfFamilyEducationLevel)
            .Include(s => s.SocioeconomicStatus)
            .Include(s => s.LSMSocioeconomicStatus)
            .Include(s => s.Bank)
            .AsNoTracking();

        _dataSource = new GridDevExtremeDataSource<Subject>(query);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

```razor
<DxGrid @ref="_grid"
        Data="@_dataSource"
        ShowFilterRow="true"
        PageSize="25"
        ...>
```

#### 3.2.2 DxFilterBuilder Uyumu

Mevcut DxFilterBuilder alanları `CityName`, `DistrictName` gibi flattened DTO alanları kullanıyor. `GridDevExtremeDataSource<Subject>` ise entity property'leri üzerinden çalışır.

**Çözüm — Seçenek A (Önerilen): FieldName'leri entity path'lerine çevir**

```razor
<!-- DTO: CityName → Entity: City.Name -->
<DxFilterBuilderField FieldName="City.Name" Caption="Şehir" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="District.Name" Caption="İlçe" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="Profession.Name" Caption="Meslek" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="EducationLevel.Name" Caption="Eğitim Seviyesi" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="HeadOfFamilyProfession.Name" Caption="A.R. Meslek" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="HeadOfFamilyEducationLevel.Name" Caption="A.R. Eğitim" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="Bank.Name" Caption="Banka" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="SocioeconomicStatus.Code" Caption="SES" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="LSMSocioeconomicStatus.Code" Caption="LSM SES" Type="@typeof(string)"/>

<!-- Doğrudan entity alanları (değişiklik yok) -->
<DxFilterBuilderField FieldName="FullName" Caption="Ad Soyad" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="PhoneNumber" Caption="Telefon" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="Email" Caption="E-posta" Type="@typeof(string)"/>
<DxFilterBuilderField FieldName="Age" Caption="Yaş" Type="@typeof(int)"/>
<DxFilterBuilderField FieldName="Gender" Caption="Cinsiyet" Type="@typeof(Gender)"/>
<!-- ...diğer scalar alanlar aynı kalır -->
```

**Grid kolonları da aynı şekilde güncellenir:**

```razor
<!-- DTO: CityName → Entity: City.Name -->
<DxGridDataColumn FieldName="City.Name" Caption="Şehir"/>
<DxGridDataColumn FieldName="District.Name" Caption="İlçe"/>
<DxGridDataColumn FieldName="Profession.Name" Caption="Meslek"/>
<!-- vs. -->
```

#### 3.2.3 Seçim (Multi-Select) Desteği

`GridDevExtremeDataSource` ile `SelectedDataItems` çalışır ancak **SelectAll tüm veriyi yükler**. Bunu engellemek için:

```razor
<DxGrid ...
        SelectAllCheckboxMode="GridSelectAllCheckboxMode.Page"
        KeyFieldName="Id">
```

- `Page` modu: Sadece mevcut sayfa seçilir (performans güvenli)
- `KeyFieldName` zorunlu: `GridDevExtremeDataSource` ile seçim için şart

#### 3.2.4 Excel Export

`GridDevExtremeDataSource` ile `ExportToXlsxAsync()` sadece **yüklenen/filtrelenmiş veriyi** export eder.

Eğer tüm veriyi export etmek gerekiyorsa, özel bir servis metodu ile yapılmalı:

```csharp
// SubjectService'e eklenecek (gerekirse)
Task<List<SubjectExportDto>> GetFilteredForExportAsync(CriteriaOperator? filter);
```

**Not:** Mevcut durumda export zaten filtrelenmiş veriyi alıyor. `GridDevExtremeDataSource` ile de aynı davranışı gösterecek.

#### 3.2.5 SMS/Bildirim Gönderimi

Mevcut akış: Seçili deneklere SMS/push gönder. `SelectedDataItems` ile seçili Subject'ler alınır.

`GridDevExtremeDataSource` ile `SelectedDataItems` hâlâ `IReadOnlyList<object>` döner, cast ile kullanılabilir:

```csharp
var selectedSubjects = _selectedSubjects.Cast<Subject>().ToList();
var phoneNumbers = selectedSubjects.Select(s => s.PhoneNumber).ToList();
```

---

### 3.3 Adım 3 — ProjectSubjects.razor (Proje Denek Atama)

Bu sayfa daha karmaşık çünkü Subject + ProjectAssignment birleştirilmiş bir view gösteriyor.

**Yaklaşım:** DB View veya LINQ projection kullanarak IQueryable oluşturmak.

```csharp
// Sorgu
var query = _context.Subjects
    .Include(s => s.City)
    .Include(s => s.District)
    .Include(s => s.Profession)
    .Include(s => s.EducationLevel)
    .Include(s => s.Bank)
    .Include(s => s.SocioeconomicStatus)
    .Include(s => s.LSMSocioeconomicStatus)
    .AsNoTracking();

_dataSource = new GridDevExtremeDataSource<Subject>(query);
```

**Assignment durumu:** Grid'de Subject gösteriliyor, assignment bilgisi (atanmış mı, tamamlamış mı) ayrı bir mekanizma ile kontrol ediliyor. Mevcut yapı incelenip en uygun çözüm belirlenecek.

**DxFilterBuilder:** Subjects.razor ile aynı alan dönüşümleri geçerli.

---

### 3.4 Adım 4 — WithdrawalRequests.razor

**Mevcut:** Tab yapısı (Bekleyen / Onaylanan), `GetPagedAsync(status, skip, take)` ile sunucu tarafı sayfalama zaten **kısmen** yapılıyor ama grid'e tüm sonuç `Data="@_items"` ile veriliyor.

**Hedef:**

```csharp
_context = ContextFactory.CreateDbContext();

var query = _context.WithdrawalRequests
    .Include(w => w.Subject)
    .Include(w => w.BankAccount)
    .Where(w => w.Status == currentTabStatus)
    .AsNoTracking();

_dataSource = new GridDevExtremeDataSource<WithdrawalRequest>(query);
```

**Tab geçişi:** Tab değiştiğinde `Where` koşulu güncellenip `_dataSource` yeniden oluşturulacak:

```csharp
private void SetTab(string tab)
{
    _activeTab = tab;
    RecreateDataSource();
}

private void RecreateDataSource()
{
    _context?.Dispose();
    _context = ContextFactory.CreateDbContext();

    var status = _activeTab == "pending" ? ApprovalStatus.Pending : ApprovalStatus.Approved;
    var query = _context.WithdrawalRequests
        .Include(w => w.Subject)
        .Include(w => w.BankAccount)
        .Where(w => w.Status == status)
        .AsNoTracking();

    _dataSource = new GridDevExtremeDataSource<WithdrawalRequest>(query);
    StateHasChanged();
}
```

**Grid kolonları:**

```razor
<DxGridDataColumn FieldName="Subject.FullName" Caption="Denek"/>
<DxGridDataColumn FieldName="BankAccount.BankName" Caption="Banka"/>
<DxGridDataColumn FieldName="BankAccount.IbanLast4" Caption="IBAN"/>
<DxGridDataColumn FieldName="Amount" Caption="Tutar"/>
<DxGridDataColumn FieldName="AmountTry" Caption="TL Karşılığı"/>
<DxGridDataColumn FieldName="Status" Caption="Durum"/>
<DxGridDataColumn FieldName="CreatedAt" Caption="Tarih"/>
```

**FilterRow yeterli** — bu grid için DxFilterBuilder'a gerek yok.

---

### 3.5 Adım 5 — NotificationHistory.razor (SMS + Push)

**İki ayrı grid — ikisi de yüksek hacimli.**

#### SMS Grid:

```csharp
var smsQuery = _context.SmsLogs
    .Include(s => s.Subject)
    .OrderByDescending(s => s.CreatedAt)
    .AsNoTracking();

_smsDataSource = new GridDevExtremeDataSource<SmsLog>(smsQuery);
```

#### Push Grid:

```csharp
var pushQuery = _context.Notifications
    .Include(n => n.Subject)
    .OrderByDescending(n => n.CreatedAt)
    .AsNoTracking();

_pushDataSource = new GridDevExtremeDataSource<Notification>(pushQuery);
```

**Grid kolonları entity path'lerine çevrilecek:**

```razor
<!-- SMS -->
<DxGridDataColumn FieldName="PhoneNumber" Caption="Telefon"/>
<DxGridDataColumn FieldName="Subject.FullName" Caption="Denek"/>
<DxGridDataColumn FieldName="Message" Caption="Mesaj"/>
<DxGridDataColumn FieldName="Source" Caption="Kaynak"/>
<DxGridDataColumn FieldName="DeliveryStatus" Caption="Durum"/>
<DxGridDataColumn FieldName="CreatedAt" Caption="Tarih"/>

<!-- Push -->
<DxGridDataColumn FieldName="Subject.FullName" Caption="Denek"/>
<DxGridDataColumn FieldName="Title" Caption="Başlık"/>
<DxGridDataColumn FieldName="Body" Caption="İçerik"/>
<DxGridDataColumn FieldName="DeliveryStatus" Caption="Durum"/>
<DxGridDataColumn FieldName="CreatedAt" Caption="Tarih"/>
```

**FilterRow yeterli** — bu gridler için ekstra filtre paneli gereksiz.

---

## 4. Filtreleme Stratejisi — Grid Bazında Detay

### 4.1 Filtreleme Yöntemleri Karşılaştırması

| Yöntem | Açıklama | Ne Zaman Kullan |
|--------|----------|-----------------|
| **FilterRow** (`ShowFilterRow="true"`) | Her kolonun altında inline filtre | Her grid'de temel filtre olarak |
| **DxFilterBuilder** (açılır panel) | Detaylı, çok koşullu, AND/OR grupları | Çok alanlı detay sorgu gereken gridler |
| **HeaderFilter** (`ShowHeaderFilter="true"`) | Kolon başlığında dropdown ile filtre | Enum/lookup alanları için hızlı filtre |

### 4.2 Grid Bazında Filtreleme Kararları

#### Subjects.razor — Detaylı Filtreleme (DxFilterBuilder KORUNACAK)

**Mevcut DxFilterBuilder KORUNACAK ve geliştirilecek.** Denekler ana iş nesnesi; detaylı sorgu kritik.

**Ek iyileştirmeler:**
- `HeaderFilter` eklenmesi (enum alanları için hızlı filtre):

```razor
<DxGrid ShowFilterRow="true"
        ShowHeaderFilter="true"  <!-- YENİ -->
        ...>
```

- Enum kolonlarına `HeaderFilterMode`:

```razor
<DxGridDataColumn FieldName="Gender" Caption="Cinsiyet">
    <HeaderFilterSettings SearchBoxVisible="false" />
</DxGridDataColumn>

<DxGridDataColumn FieldName="Status" Caption="Durum">
    <HeaderFilterSettings SearchBoxVisible="false" />
</DxGridDataColumn>
```

Bu sayede FilterRow + DxFilterBuilder + HeaderFilter üçü birlikte çalışır. Kullanıcı:
- Hızlı tek kolon filtresi → FilterRow veya HeaderFilter
- Detaylı çoklu koşul → DxFilterBuilder paneli

#### ProjectSubjects.razor — DxFilterBuilder KORUNACAK

Subjects.razor ile aynı yaklaşım. Assignment durumu ek filtre alanı olarak eklenecek.

#### WithdrawalRequests.razor — Sadece FilterRow

**Ek panel gereksiz.** Tab yapısı zaten ana filtrelemeyi sağlıyor (Bekleyen/Onaylanan). FilterRow ile:
- Denek adı
- Banka
- Tutar aralığı
- Tarih

yeterli.

#### NotificationHistory.razor — Sadece FilterRow

SMS ve Push gridleri için FilterRow yeterli. Ana filtreler:
- Denek adı / telefon
- Durum (Sent/Failed/Pending)
- Kaynak (SMS: Otp/AdminBulk/System)
- Tarih

#### Küçük Gridler (Kategori B) — Sadece FilterRow

Projects, Customers, News, Stories, AdminUsers, Roles, SpecialDays, MessageAutomations — tümü FilterRow ile devam edecek. Veri hacmi düşük, ekstra filtreleme mekanizması gereksiz karmaşıklık.

### 4.3 FilterRow'da Between (Aralık) Filtreleme

DxGrid FilterRow'da tarih ve sayısal kolonlar için `between` filtresi otomatik olarak desteklenir:

```razor
<DxGridDataColumn FieldName="Age" Caption="Yaş">
    <FilterRowCellTemplate>
        <DxFilterRowOperationSwitcher />  <!-- Kullanıcı operatörü seçebilir: =, >, <, Between -->
    </FilterRowCellTemplate>
</DxGridDataColumn>

<DxGridDataColumn FieldName="CreatedAt" Caption="Kayıt Tarihi" DisplayFormat="dd.MM.yyyy">
    <FilterRowCellTemplate>
        <DxFilterRowOperationSwitcher />
    </FilterRowCellTemplate>
</DxGridDataColumn>
```

`DxFilterRowOperationSwitcher` sayesinde kullanıcı FilterRow'da operatör seçebilir:
- `=`, `<>` (eşit, eşit değil)
- `>`, `>=`, `<`, `<=` (karşılaştırma)
- `Between` (aralık)
- `Contains`, `StartsWith`, `EndsWith` (metin)

**Bu özellik Subjects grid'inde şu alanlara eklenecek:**
- `Age` (yaş aralığı: 18-25 arası)
- `Star` (yıldız aralığı)
- `CreatedAt` (kayıt tarihi aralığı)
- `BirthDate` (doğum tarihi aralığı)

---

## 5. Uygulama Sırası ve Tahmini Efor

### Faz 1 — Altyapı (Ön koşul)

| Adım | İş | Dosya(lar) |
|------|----|-----------|
| 1.1 | `AddDbContextFactory` kaydı | `Infrastructure/DependencyInjection.cs` |
| 1.2 | `_Imports.razor`'a gerekli using'ler | `Admin/Components/_Imports.razor` |

### Faz 2 — Subjects.razor Geçişi (En Kritik)

| Adım | İş |
|------|----|
| 2.1 | `IDbContextFactory` inject et, `IDisposable` implement et |
| 2.2 | `GetAllAsync()` çağrısını kaldır → `GridDevExtremeDataSource<Subject>` oluştur |
| 2.3 | Grid kolonlarını entity path'lerine çevir (`CityName` → `City.Name` vb.) |
| 2.4 | DxFilterBuilder alanlarını entity path'lerine çevir |
| 2.5 | HeaderFilter ekle (enum alanları) |
| 2.6 | ~~FilterRow'a OperationSwitcher~~ — DevExpress'te yerleşik bileşen yok, DxFilterBuilder between desteği yeterli |
| 2.7 | `KeyFieldName="Id"` + `SelectAllCheckboxMode="Page"` ekle |
| 2.8 | Export, SMS gönder, bildirim gönder akışlarını test et |
| 2.9 | `CellDisplayTemplate` olan kolonları entity property'lerine uyarla |
| 2.10 | TotalSummary'nin server-side çalıştığını doğrula |

### Faz 3 — ProjectSubjects.razor Geçişi

| Adım | İş |
|------|----|
| 3.1 | Subjects.razor ile aynı dönüşüm |
| 3.2 | Assignment durumu (atanmış/atanmamış/tamamlanmış) ayrı mekanizma ile kontrol |
| 3.3 | Seçim + atama akışını test et |

### Faz 4 — WithdrawalRequests.razor Geçişi

| Adım | İş |
|------|----|
| 4.1 | Tab yapısını `GridDevExtremeDataSource` ile uyumlu hale getir |
| 4.2 | `GetPagedAsync` çağrılarını kaldır → IQueryable + Where kullan |
| 4.3 | Onay/Red butonlarını entity ile çalışacak şekilde uyarla |
| 4.4 | Summary (toplam tutar) hesaplamasını doğrula |

### Faz 5 — NotificationHistory.razor Geçişi

| Adım | İş |
|------|----|
| 5.1 | SMS grid → `GridDevExtremeDataSource<SmsLog>` |
| 5.2 | Push grid → `GridDevExtremeDataSource<Notification>` |
| 5.3 | Tab geçişlerini test et |
| 5.4 | Detay popup'ını entity ile uyumlu hale getir |

### Faz 6 — Küçük Gridlere FilterRow İyileştirmesi (Opsiyonel)

| Grid | İyileştirme |
|------|-------------|
| Projects.razor | Status kolonuna HeaderFilter ekle |
| PaymentBatches.razor | Status kolonuna HeaderFilter ekle |
| Tüm küçük gridler | Tarih kolonlarına `DxFilterRowOperationSwitcher` ekle (between desteği) |

---

## 6. Dikkat Edilmesi Gerekenler

### 6.1 Breaking Change'ler

| Konu | Detay | Çözüm |
|------|-------|-------|
| DTO → Entity geçişi | Grid artık DTO değil Entity gösterecek | `CellDisplayTemplate`'ler entity property'lerine uyarlanacak |
| Navigation property path | `CityName` → `City.Name` | Tüm FieldName'ler güncellecek |
| Export | DTO yerine Entity export edilecek | Export metodu güncellecek veya grid'in kendi export'u kullanılacak |
| SelectedDataItems tipi | `SubjectListItemDto` → `Subject` | Cast'ler güncellecek |

### 6.2 Performans Notları

- `AsNoTracking()` **zorunlu** — tracking gereksiz bellek tüketir
- `Include` sayısını minimumda tut — sadece grid'de gösterilen navigation property'ler
- `GridDevExtremeDataSource` varsayılan olarak sayfa verisi kadar çeker
- `TotalSummary` (Count/Sum) ek SQL sorgusu yapar — kabul edilebilir

### 6.3 `GridDevExtremeDataSource` Kısıtlamaları

| Kısıtlama | Etki | Çözüm |
|-----------|------|-------|
| Cell editing yok | Yok — zaten popup ile düzenleme yapılıyor | — |
| Custom sorting yok | Yok — standart sıralama yeterli | — |
| Drag-drop desteklemez | News ve Stories gridleri etkilenmez (küçük, Kategori B) | — |
| SelectAll tüm veriyi çeker | Performans riski | `SelectAllCheckboxMode="Page"` kullan |
| DateTime fonksiyonları | `IsThisMonth()` vb. desteklenmez | Tarih filtreleri `Between` ile yapılacak |

### 6.4 EF Core Thread Safety

> **KESİN KURAL:** `Task.WhenAll(...)` ile paralel DB işlemi YASAK.

`GridDevExtremeDataSource` kendi `DbContext` instance'ını kullanacak (`IDbContextFactory` ile oluşturulan). Bu sayede sayfa içindeki diğer DB işlemleriyle çakışma olmaz.

Ancak aynı sayfa içinde başka DB işlemleri (örn. onay/red butonu) için repository'deki mevcut scoped `DbContext` kullanılmaya devam edecek. İki ayrı DbContext instance = thread-safe.

---

## 7. Geçiş Sonrası Doğrulama Kontrol Listesi

Her grid geçişi sonrası şu kontroller yapılmalı:

- [ ] Sayfa ilk yüklemede veri gösteriyor mu?
- [ ] FilterRow filtreleri çalışıyor mu? (metin, sayı, tarih, enum)
- [ ] DxFilterBuilder filtreleri çalışıyor mu? (Subjects ve ProjectSubjects)
- [ ] Sıralama (kolon başlığına tıklama) çalışıyor mu?
- [ ] Sayfalama çalışıyor mu? (sayfa geçişi, sayfa boyutu değiştirme)
- [ ] Summary satırı doğru hesaplanıyor mu?
- [ ] Excel export çalışıyor mu?
- [ ] Satır seçimi çalışıyor mu? (varsa)
- [ ] Context menü / aksiyon butonları çalışıyor mu?
- [ ] CellDisplayTemplate'ler doğru render oluyor mu?
- [ ] Popup'lar (düzenleme, detay) doğru veriyi gösteriyor mu?
- [ ] Sayfa dispose edildiğinde DbContext temizleniyor mu?
- [ ] Bellek kullanımı kabul edilebilir seviyede mi?

---

## 8. Dosya Değişiklik Özeti

| Dosya | Değişiklik Tipi |
|-------|-----------------|
| `Infrastructure/DependencyInjection.cs` | `AddDbContextFactory` ekleme |
| `Admin/Components/_Imports.razor` | Using ekleme |
| `Admin/Components/Pages/Subjects/Subjects.razor` | Server-side geçiş + filtre güncelleme |
| `Admin/Components/Pages/Projects/ProjectSubjects.razor` | Server-side geçiş + filtre güncelleme |
| `Admin/Components/Pages/Payments/WithdrawalRequests.razor` | Server-side geçiş |
| `Admin/Components/Pages/Notifications/NotificationHistory.razor` | Server-side geçiş |

**Toplam:** 8 dosya değişikliği (4 büyük grid + 1 entity + 1 EF config + 1 altyapı + 1 imports)

---

## 9. Gelecek İyileştirmeler (Bu Plan Kapsamında Değil)

- **Saved Filters:** Kullanıcının sık kullandığı filtreleri kaydetmesi (DB'ye)
- **Server Mode:** Çok büyük veri setleri için `EntityInstantFeedbackSource` geçişi (şimdilik gerek yok)
- **Virtual Scrolling:** Sayfalama yerine sonsuz kaydırma (UX tercihi)
- **Export Service:** Büyük veri export'u için arka plan iş (Hangfire) ile Excel oluşturma

---

## 10. Uygulama Raporu

> **Uygulama Tarihi:** 2026-03-04
> **Branch:** `feature/grid-server-side-filtering`
> **Commit:** `fc9d771`
> **Build:** 3/3 proje — 0 hata, 0 uyarı

### 10.1 Tamamlanan İşler

| Faz | Durum | Detay |
|-----|-------|-------|
| Faz 1 — Altyapı | ✅ Tamamlandı | `AddDbContextFactory` + `_Imports.razor` using'leri |
| Faz 2 — Subjects.razor | ✅ Tamamlandı | Server-side + DxFilterBuilder + HeaderFilter |
| Faz 3 — ProjectSubjects.razor | ✅ Tamamlandı | Server-side + DxFilterBuilder |
| Faz 4 — WithdrawalRequests.razor | ✅ Tamamlandı | Server-side + tab-based query recreation |
| Faz 5 — NotificationHistory.razor | ✅ Tamamlandı | SMS + Push gridleri server-side |
| Faz 6 — Küçük grid iyileştirmeleri | ⏳ Opsiyonel | Henüz uygulanmadı |

### 10.2 Değişen Dosyalar (8 dosya, +412 -362)

| Dosya | Değişiklik |
|-------|-----------|
| `Infrastructure/DependencyInjection.cs` | `AddDbContextFactory<AppDbContext>` kaydı eklendi |
| `Admin/Components/_Imports.razor` | `DevExpress.Data.Filtering`, `Microsoft.EntityFrameworkCore`, `PulsePoll.Infrastructure.Persistence` using'leri eklendi |
| `Domain/Entities/Subject.cs` | `ScoreSnapshot` navigation property eklendi (Star/Score gösterimi için) |
| `Infrastructure/Persistence/Configurations/SubjectScoreConfiguration.cs` | 1:1 ilişki düzeltmesi (`.WithMany()` → `.WithOne(s => s.ScoreSnapshot)`) |
| `Admin/.../Subjects/Subjects.razor` | `GridDevExtremeDataSource<Subject>` + entity path dönüşümleri + HeaderFilter |
| `Admin/.../Projects/ProjectSubjects.razor` | `GridDevExtremeDataSource<Subject>` + entity path dönüşümleri |
| `Admin/.../Payments/WithdrawalRequests.razor` | `GridDevExtremeDataSource<WithdrawalRequest>` + `RecreateDataSource()` pattern |
| `Admin/.../Notifications/NotificationHistory.razor` | SMS: `GridDevExtremeDataSource<SmsLog>`, Push: `GridDevExtremeDataSource<Notification>` (ayrı DbContext'ler) |

### 10.3 Teknik Kararlar ve Sapmalar

| Konu | Plandaki Yaklaşım | Uygulanan Yaklaşım | Neden |
|------|-------------------|---------------------|-------|
| Subject.Star alanı | Doğrudan entity property | `ScoreSnapshot` navigation + unbound column | Star, `SubjectScoreSnapshot` entity'sinde — navigation property gerekti |
| Subject.FullName | Doğrudan FieldName | Unbound column (`[FirstName] + ' ' + [LastName]`) | `FullName` NotMapped property, SQL'e çevrilemiyor |
| Subject.Age | Doğrudan FieldName | Unbound column + CellDisplayTemplate | `Age` hesaplanmış property, SQL'e çevrilemiyor |
| DxFilterBuilder'da FullName | Tek alan | `FirstName` + `LastName` ayrı alanlar | SQL-uyumlu filtreleme için |
| DxFilterBuilder'da Age | Tek alan | `BirthDate` (DateOnly) olarak değiştirildi | SQL-uyumlu filtreleme için |
| NotificationHistory | Tek context | İki ayrı context (`_smsContext`, `_pushContext`) | Her grid kendi DbContext'ini kullanmalı (thread safety) |
| WithdrawalRequests count badge | Ayrı count sorgusu | Badge'ler korundu, grid üzerinden | `GridDevExtremeDataSource` ile count otomatik |

### 10.4 Küçük Gridler — Değişiklik Yapılmadı (Kategori B)

Aşağıdaki gridler client-side (`Data="@_list"`) olarak kaldı — veri hacmi düşük, geçiş gereksiz:

- Projects.razor, Customers.razor, News.razor, Stories.razor
- AdminUsers.razor, Roles.razor, SpecialDaysCalendar.razor
- MessageAutomations.razor, PaymentBatches.razor, PaymentBatchDetail.razor
- SubjectReferrals.razor, SubjectProjects.razor, SubjectLedger.razor
- ProjectStatement.razor
