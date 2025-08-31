cd C:\_git\ArchiX\Dev\ArchiX

# 0) Excel açıksa kapat
# 1) (Varsa) docs içindeki Excel’i notlarim’a taşı
move .\docs\ArchiX_Is_Takip_Listesi.xlsx C:\_git\ArchiX\notlarim\ 2>$null

# 2) docs klasörünü sil (normal klasör)
Remove-Item -Recurse -Force .\docs

# 3) notlarim’a junction oluştur (docs -> notlarim)
cmd /c mklink /J docs C:\_git\ArchiX\notlarim

# 4) kontrol: artık 4 dosya görünmeli
dir .\docs

# 5) git’e ekle, commit et, pushla
git add -f docs/
git commit -m "docs -> notlarim junction; tüm notlar eklendi" 2>$null
git push origin main
