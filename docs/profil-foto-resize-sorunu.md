# Profil Fotoğrafı Resize + EXIF Orientation Sorunu

## Problem
Android'de kamera ile çekilen fotoğraf resize edildiğinde yan/ters dönük görünüyor.

## Neden
- Android kamera fotoğrafı her zaman landscape orientation'da çeker, doğru yönü **EXIF metadata** (`ExifInterface.TagOrientation`) içinde saklar.
- `Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream()` Android'de `BitmapFactory.DecodeStream` kullanır — bu EXIF rotation tag'ini **yok sayar**.
- Dolayısıyla resize edilen görsel EXIF bilgisi olmadan kaydedilir ve fotoğraf yan/ters görünür.
- iOS'ta bu sorun yok çünkü `UIImage` EXIF orientation'ı otomatik uygular.

## Gereksinim
- Profil fotoğrafı max **512x512**, JPEG, ~%75 kalite olmalı (sunucu limiti 5MB, küçük tutmak istiyoruz).
- EXIF orientation doğru uygulanmalı (düz çekilen fotoğraf düz görünmeli).

## Çözüm Seçenekleri

### 1. Platform-specific resize (dependency yok)
Android'de `ExifInterface` + `BitmapFactory` + `Matrix.PostRotate`, iOS'te `UIImage`:
```csharp
#if ANDROID
var exif = new Android.Media.ExifInterface(filePath);
var orientation = exif.GetAttributeInt(
    Android.Media.ExifInterface.TagOrientation,
    (int)Android.Media.Orientation.Normal);

var bitmap = Android.Graphics.BitmapFactory.DecodeFile(filePath);
var matrix = new Android.Graphics.Matrix();
switch ((Android.Media.Orientation)orientation)
{
    case Android.Media.Orientation.Rotate90:  matrix.PostRotate(90);  break;
    case Android.Media.Orientation.Rotate180: matrix.PostRotate(180); break;
    case Android.Media.Orientation.Rotate270: matrix.PostRotate(270); break;
}
var rotated = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
// ... scale + compress to JPEG stream
#elif IOS
var uiImage = UIKit.UIImage.FromFile(filePath);
// UIImage handles EXIF automatically, just resize with CGContext or UIGraphics
#endif
```

### 2. SkiaSharp (cross-platform, ekstra NuGet)
`SkiaSharp.Views.Maui.Controls` paketi + `SKBitmap.Decode` + `SKCanvas.RotateDegrees`:
- EXIF'i otomatik handle eden `SKCodec` var.
- Tek kod, iki platformda çalışır.

### 3. Sunucu tarafında resize (mobilde hiç resize yok)
- Fotoğrafı olduğu gibi gönder (EXIF korunur).
- API tarafında ImageSharp/SkiaSharp ile resize et.
- Dezavantaj: büyük dosya upload edilir (3-8 MB).

## Mevcut Durum
Şu an resize kaldırıldı, fotoğraf olduğu gibi gönderiliyor. EXIF korunduğu için orientation doğru ama dosya boyutu büyük olabilir.

## Dosyalar
- `src/PulsePoll.Mobile/ViewModels/ProfileViewModel.cs` → `PickPhotoAsync` metodu
- `src/PulsePoll.Api/Controllers/ProfileController.cs` → `UploadPhoto` endpoint (5MB limit)
