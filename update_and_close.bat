@echo off
setlocal
cd /d "%~dp0"

"Portable Brave Updater.exe" --update=existing --quit-after --log "%~dp0update.log"

set EXITCODE=%ERRORLEVEL%
echo Exit code: %EXITCODE%
pause
exit /b %EXITCODE%
