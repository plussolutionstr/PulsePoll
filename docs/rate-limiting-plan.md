# PulsePoll API — Rate Limiting Planı

> Tarih: 2026-03-04
> Durum: Henüz uygulanmadı (`ErrorCodes.RateLimitExceeded` tanımlı ama hiçbir yerde aktif değil)

---

## Kritik — Kesinlikle Uygulanmalı

| # | Endpoint | Neden | Partition | Limit |
|---|----------|-------|-----------|-------|
| 1 | `POST /api/auth/send-otp` | SMS maliyeti + spam | IP | **3 istek / 1 dk** |
| 2 | `POST /api/auth/verify-otp` | OTP brute force (6 haneli = 1M kombinasyon) | IP | **5 istek / 1 dk** |
| 3 | `POST /api/auth/login` | Credential stuffing | IP | **5 istek / 1 dk** |
| 4 | `POST /api/auth/register` | Spam kayıt | IP | **3 istek / 5 dk** |
| 5 | `POST /api/wallet/withdraw` | Finansal — para çekme abuse | Kullanıcı (sub) | **3 istek / 1 saat** |
| 6 | `POST /api/webhook/survey-complete` | Dışarıdan çağrılır, sahte tamamlama riski | IP | **30 istek / 1 dk** |
| 7 | Admin `/api/auth/login` (minimal API) | Admin brute force | IP | **5 istek / 5 dk** |

## Yüksek — Önerilir

| # | Endpoint | Neden | Partition | Limit |
|---|----------|-------|-----------|-------|
| 8 | `POST /api/wallet/banks` | Sahte banka hesabı spam | Kullanıcı (sub) | **5 istek / 1 saat** |
| 9 | `POST /api/auth/refresh` | Token refresh abuse | IP | **10 istek / 1 dk** |
| 10 | `POST /api/projects/{id}/start` | Anket başlatma abuse | Kullanıcı (sub) | **10 istek / 1 dk** |
| 11 | `POST /api/telemetry/activity` | Telemetri spam | Kullanıcı (sub) | **30 istek / 1 dk** |

## Global — Genel Koruma Katmanı

| Kapsam | Partition | Limit |
|--------|-----------|-------|
| Tüm authenticated endpointler | Kullanıcı (sub claim) | **60 istek / 1 dk** |
| Tüm anonymous endpointler | IP | **30 istek / 1 dk** |

---

## Teknik Uygulama Notları

### Kullanılacak Altyapı
- **ASP.NET Core Built-in Rate Limiting** (`Microsoft.AspNetCore.RateLimiting` — .NET 7+)
- `AddRateLimiter()` → `ServiceExtensions.AddApiServices()` içine
- `UseRateLimiter()` → `Program.cs` middleware pipeline'ına (`UseAuthentication`'dan **önce**)
- Endpoint bazlı: `[EnableRateLimiting("policy-name")]` attribute

### Policy İsimlendirme
| Policy Adı | Açıklama |
|------------|----------|
| `otp` | send-otp için (IP, 3/1dk) |
| `auth` | login + verify-otp + register (IP, 5/1dk) |
| `auth-register` | register için daha sıkı (IP, 3/5dk) |
| `withdrawal` | para çekme (User, 3/1sa) |
| `webhook` | survey-complete (IP, 30/1dk) |
| `admin-login` | admin giriş (IP, 5/5dk) |
| `bank-add` | banka ekleme (User, 5/1sa) |
| `token-refresh` | refresh token (IP, 10/1dk) |
| `project-start` | anket başlatma (User, 10/1dk) |
| `telemetry` | aktivite takibi (User, 30/1dk) |
| `global-authenticated` | genel kullanıcı limiti (User, 60/1dk) |
| `global-anonymous` | genel IP limiti (IP, 30/1dk) |

### Rate Limit Aşıldığında Response
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Çok fazla istek gönderildi. Lütfen biraz bekleyip tekrar deneyin."
  }
}
```
- HTTP Status: **429 Too Many Requests**
- `Retry-After` header eklenmeli

### Partition Stratejisi
- **IP bazlı**: `X-Forwarded-For` header (reverse proxy arkasında) veya `RemoteIpAddress`
- **Kullanıcı bazlı**: JWT `sub` claim'i — authenticated endpointlerde
- Hem IP hem User limiti bir arada çalışmalı (global + endpoint-specific)

### Middleware Sırası (Program.cs)
```
CorrelationIdMiddleware
SerilogRequestLogging
CORS
UseRateLimiter          ← BURAYA
ExceptionMiddleware
UseAuthentication
UseAuthorization
MapControllers
```
