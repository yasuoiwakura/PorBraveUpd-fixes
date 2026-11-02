@echo off
setlocal
cd /d "%~dp0"

"Portable Brave Updater.exe" --update=existing --launch-brave=stable:x64 --quit-after --log "%~dp0PortableAppsUpdate.log"

exit /b %ERRORLEVEL%
