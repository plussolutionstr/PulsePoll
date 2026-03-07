# iOS TestFlight Build

## 1. Build numarasını artır

`src/PulsePoll.Mobile/PulsePoll.Mobile.csproj` dosyasında `ApplicationVersion` değerini 1 artır:

```xml
<ApplicationVersion>5</ApplicationVersion>  <!-- her seferinde 1 artır -->
```

## 2. IPA oluştur

```bash
/opt/homebrew/bin/dotnet publish src/PulsePoll.Mobile/PulsePoll.Mobile.csproj \
  -f net10.0-ios \
  -c Release \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="Apple Distribution: ilhan cakmak (88VFGVWM3B)" \
  -p:CodesignProvision="00475119-e531-4e15-b62d-a3af500a3353" \
  -p:ArchiveOnBuild=true \
  -p:MtouchLink=SdkOnly \
  -v minimal
```

IPA çıktısı: `src/PulsePoll.Mobile/bin/Release/net10.0-ios/ios-arm64/publish/PulsePoll.Mobile.ipa`

## 3. Transporter ile yükle

1. Transporter uygulamasını aç (Mac App Store'dan indirilebilir)
2. IPA dosyasını sürükle bırak
3. **Deliver** butonuna bas
4. App Store Connect'te TestFlight sekmesinden build'in işlenmesini bekle
