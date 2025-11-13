# 14-0655 — Güvenlik: Parametrik Yapının Kurulması

Özet
- Amaç: Konfigürasyon/veri odaklı parametre altyapısının kurulması, `ArchiXSetting` yapısının kaldırılması ve yerine `Parameter` + `ParameterDataType` tablo yapılarının devreye alınması.
- Sonuç: Parametreler tek `Value` kolonu (nvarchar(max)) üzerinden, beklenen tip bilgisi `ParameterDataType` ile (FK) yönetilir. İkili doğrulama (2FA) için varsayılan kanal SMS olacak şekilde JSON parametresi seed edildi. Connection Policy artık `Parameters` tablosundan okunuyor.

İçindekiler
- Teslim edilen değişiklikler
- Şema ve model ayrıntıları
- Seed içerikleri
- Çalışma zamanı davranışı
- Operasyon akışları (ekleme/güncelleme)
- Yayına alma (migrasyon) notları
- Testler
- Geri dönüş (rollback) notları

Teslim Edilen Değişiklikler
1) ArchiXSetting kaldırıldı
- `ArchiXSetting` EF modelinden çıkarıldı (DB’de drop edilmesi için migration üretildi).
- Connection Policy sağlayıcısı `ArchiXSetting` yerine `Parameters`’tan okur.

2) Yeni tablolar
- `ParameterDataType` (BaseEntity): Kod aralıkları ve tip meta bilgisi (Name, Category, Description).
- `Parameter` (BaseEntity): `(Group, Key)` benzersiz, `ParameterDataTypeId` FK, `Value` nvarchar(max), `Template` (örn. JSON şablonu), `Description`.

3) İkili Doğrulama (2FA) default parametresi
- `Group = "TwoFactor"`, `Key = "Options"`, `DataType = Json`.
- `Value` varsayılanı: `{"defaultChannel":"Sms"}` (SMS varsayılan).
- `Template`: Sms/Email/Authenticator için örnek alanları içeren JSON.

Şema ve Model Ayrıntıları
- ParameterDataType (unique: Code, Name)
  - NVARCHAR (1–100): 60,70,80,90,100 → NVarChar_50/100/250/500/Max
  - Numeric (200+): 200..240 → Byte, SmallInt, Int, BigInt, Decimal18_6
  - Temporal (300+): 300..320 → Date, Time, DateTime
  - Other (900+): 900..920 → Bool, Json, Secret

- Parameter
  - PK: Id
  - Unique: (Group, Key)
  - FK: ParameterDataTypeId → ParameterDataType(Id) (Restrict)
  - Group: nvarchar(75), Key: nvarchar(150), Description: nvarchar(500)
  - Value, Template: nvarchar(max)

Seed İçerikleri
- ParameterDataType: Yukarıdaki tüm kodlar ve adlar idempotent seed edildi.
- TwoFactor template (özet):