import re

file_path = r'D:\_git\ArchiX\Dev\ArchiX\src\ArchiX.Library.Web\Templates\Modern\Pages\grid-template-filtreli.cshtml'

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# toggleFilter('column', e) -> toggleFilter('column', event) dönüþümü
content = re.sub(r"toggleFilter\('([^']+)', e\)", r"toggleFilter('\1', event)", content)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print('Düzeltme baþarýyla tamamlandý!')
print('Deðiþiklikler:')
print('  - toggleFilter(..., e) -> toggleFilter(..., event)')
