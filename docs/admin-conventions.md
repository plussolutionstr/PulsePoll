# Admin UI ve İş Kuralı Konvansiyonları

Bu dosya, tekrar eden revizyonları azaltmak için proje içi referans standarttır.

## Genel UI

- Metinlerde Türkçe karakter kullanılmalı (`İptal`, `Reddet`, `Çekim`, `Ödül` vb.).
- DevExpress `DxPopup` kullanımlarında `ShowFooter="true"` olmalı.
- Popup footer buton düzeni:
  - Aksiyon butonu önce, `İptal` sonra.
  - `RenderStyleMode="ButtonRenderStyleMode.Outline"` kullanılmalı.
  - Diğer popup düzen/stiliyle tutarlı olmalı.
- Gridlerde proje standardı 3 noktalı context menü tercih edilir.

## Grid Davranışları

- Uzun metin alanları (SMS/Push gibi) gridde kısaltılmış gösterilmeli, detay popup ile açılmalı.
- Gerekmedikçe `Id` kolonu gösterilmemeli.
- Listeleme varsayılanı çoğu yönetim ekranında `Z-A` (yeniden eskiye) olmalı.
- Sayfalama olmayan gridlerde mümkünse standarda uygun pagination eklenmeli.

## Ödeme Ekranları

- Excel export buton etiketi sade `Excel` olmalı (ör. `Bekleyen Excel`, `Onaylanan Excel` kullanılmamalı).
- `withdrawals` ekranında IBAN maskeleme yerine tam IBAN gösterilir (operasyonel teyit ihtiyacı).

## Proje / Denek Kazanç Akışı

- `ProjectSubjects` grid başlıkları:
  - `Status` => `Anket Durumu`
  - `RewardStatus` => `Onay Durumu`
- Anket tamamlanınca kazanç doğrudan kullanılabilir bakiyeye geçmez; önce bekleyen onaya düşer.
- Onayda cüzdana yansır, redde cüzdana yansımaz.

## Cüzdan (Denek Bakiye Yönetimi)

- Ekstre ekranında 3 kart gösterilir:
  - `Kullanılabilir Bakiye`
  - `Bekleyen Bakiye`
  - `Reddedilen Bakiye`
- Admin manuel hareket ekleyebilir:
  - Artı/Eksi tipli hareket.
  - Açıklama zorunludur.
- Sadece manuel hareketler silinebilir.
- Denek tarafında manuel işlem açıklaması sade görünmelidir (`Manuel hareket:` prefix’i gösterilmez).

## İçerik Yönetimi (Haber/Story)

- Sıra yönetimi manuel sıra no alanıyla değil, sürükle-bırak ile yapılır.
- Görseller MinIO + Medya Kütüphanesi entegrasyonundan seçilir.

