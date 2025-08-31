@echo off
REM Her zaman repo köküne git
cd /d C:\_git\ArchiX\Dev\ArchiX

git add docs

for /f "tokens=1-4 delims=/: " %%a in ("%date% %time%") do (
    set datetime=%%a-%%b-%%c_%%d
)
git commit -m "docs güncellendi %datetime%"
git push origin main

pause
