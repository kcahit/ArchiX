# 14,0680	Güvenlik	Şifreleme Kuralları	Karakter Sayısı, Büyük Çükük Harf ve diğer karakter işareti ve DB de bilgilerin tutulması tasarımı

# Parola Politikası Tasarım Dokümanı (PasswordPolicy)

Revizyon: v2.1 (2025-11-26)

Bu sürüm mevcut implementasyonla uyumludur (provider, validator, hashing, seed/migration).

## 1. Amaç / Kapsam
Parola güvenliği, veritabanındaki JSON parametreleri ile yönetilir; değişiklik için deploy gerekmez. Uygulama, `Parameters` tablosundan politikayı okur ve bellekte önbellekler.

## 2. Parametre Kayıtları (ApplicationId=1)
- Parola politikası (tekleştirilmiş model):
  - Group: `Security`, Key: `PasswordPolicy`, ParameterDataTypeId: `15 (Json)`
- İkili doğrulama (bilgi amaçlı):
  - Group: `TwoFactor`, Key: `Options`, varsayılan `Value`: `{"defaultChannel":"Sms"}`

Not: Eski “Group=PasswordPolicy / Key=Options,Argon2” yaklaşımı yerine tek JSON altında `hash` bölümü bulunan yapı kullanılır.

## 3. PasswordPolicy JSON Şeması (Kesin)
Alanlar:
- version: number
- minLength: number
- maxLength: number
- requireUpper: boolean
- requireLower: boolean
- requireDigit: boolean
- requireSymbol: boolean
- allowedSymbols: string
- minDistinctChars: number
- maxRepeatedSequence: number
- blockList: string[]
- historyCount: number
- lockoutThreshold: number
- lockoutSeconds: number
- hash: object
  - algorithm: "Argon2id"
  - memoryKb: number
  - parallelism: number
  - iterations: number
  - saltLength: number
  - hashLength: number
  - fallback: object
    - algorithm: "PBKDF2-SHA512"
    - iterations: number
  - pepperEnabled: boolean

Örnek Value:
{ "version": 1, "minLength": 12, "maxLength": 128, "requireUpper": true, "requireLower": true, "requireDigit": true, "requireSymbol": true, "allowedSymbols": "!@#$%^&*_-+=:?.,;", "minDistinctChars": 5, "maxRepeatedSequence": 3, "blockList": ["password", "123456", "qwerty", "admin"], "historyCount": 10, "lockoutThreshold": 5, "lockoutSeconds": 900, "hash": { "algorithm": "Argon2id", "memoryKb": 65536, "parallelism": 2, "iterations": 3, "saltLength": 16, "hashLength": 32, "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 }, "pepperEnabled": false } }

## 4. DTO Sınıfları (Gerçek Kod)

{ "version": 1, "minLength": 12, "maxLength": 128, "requireUpper": true, "requireLower": true, "requireDigit": true, "requireSymbol": true, "allowedSymbols": "!@#$%^&*_-+=:?.,;", "minDistinctChars": 5, "maxRepeatedSequence": 3, "blockList": ["password", "123456", "qwerty", "admin"], "historyCount": 10, "lockoutThreshold": 5, "lockoutSeconds": 900, "hash": { "algorithm": "Argon2id", "memoryKb": 65536, "parallelism": 2, "iterations": 3, "saltLength": 16, "hashLength": 32, "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 }, "pepperEnabled": false } }

## 5. Sağlayıcı (Provider) ve Önbellek

Gerçek arayüz:
public interface IPasswordPolicyProvider { ValueTask<PasswordPolicyOptions> GetAsync(int applicationId = 1, CancellationToken ct = default); void Invalidate(int applicationId = 1); }
Davranış:
- `GetAsync` ilk çağrıda DB’den okur, `IMemoryCache` ile önbelleğe alır.
- Politika güncellendiğinde `Invalidate(appId)` çağrılır; sonraki `GetAsync` yeniden yükler.

## 6. Doğrulama (Validator)
Uygulama: statik `PasswordPolicyValidator`.

- İmza: `IReadOnlyList<string> Validate(string password, PasswordPolicyOptions policy)`
- Hata kodları: `EMPTY`, `MIN_LENGTH`, `MAX_LENGTH`, `REQ_UPPER`, `REQ_LOWER`, `REQ_DIGIT`, `REQ_SYMBOL`, `MIN_DISTINCT`, `REPEAT_SEQ`, `BLOCK_LIST`
- Sıra: uzunluk → kategori kontrolleri → ayırt edici karakter → tekrar sekansı → blok liste
- Not: HIBP/Pwned ve yaş/geçmiş kontrolleri yol haritasındadır.

## 7. Hashleme
- Algoritma: Argon2id (Isopoh.Cryptography.Argon2)
- Çıktı: Standart Argon2 encoded string (örn: `$argon2id$v=19$m=...,t=...,p=...$<salt>$<hash>`)
- Pepper: `ARCHIX_PEPPER` ortam değişkeni ile alınır (yoksa fallback). `pepperEnabled` true ise parolaya eklenir.
- Doğrulama: Yeni format doğrudan doğrulanır. Geçiş sürecinde eski `|pep=True/False` soneği varsa toleranslı ayrıştırma desteklenir.

## 8. Seed / Migration
- Relational DB: Migration ile güncellenir (örn. `TwoFactorDefaultChannelSms` migration’ı, `Parameters(Id=1, Group=TwoFactor, Key=Options)` için `Value` alanını `{"defaultChannel":"Sms"}` yapar).
- InMemory testler: Migration çalışmaz; `OnModelCreating` içindeki `HasData` tohumları kullanılır. Bu nedenle model seed’inde de `defaultChannel: "Sms"` ayarlanmıştır.

## 9. Güncelleme Akışı
1) `Parameters.Value` güncellenir  
2) Uygulamada `IPasswordPolicyProvider.Invalidate(appId)` çağrılır  
3) Sonraki `GetAsync` çağrısında yeni değer yüklenir

## 10. İzleme / Metrikler (Öneri)
- `password_validation_total`, `password_validation_error_total{code=...}`
- `password_hash_duration_ms`
- `password_hash_algorithm_info{algo="argon2id", memKb=..., it=..., p=...}`

## 11. Güvenlik Notları
- Düz metin parola saklanmaz, TLS zorunlu
- Pepper gizli tutulmalı (ENV/Secret Store)
- Zaman-tabanlı saldırılara karşı sabit zamanlı karşılaştırma (Argon2.Verify)
- Lockout / AttemptLimiter uygulama katmanında sağlanır

## 12. Yol Haritası
- Pwned Passwords kontrolü (k-anonymity, prefix cache)
- Blacklist genişletme ve parametrik yönetim
- `UserPasswordHistory` tablosu ve son N parolanın reddi
- Razor Pages yönetim ekranları (Policy görüntüle/güncelle)
