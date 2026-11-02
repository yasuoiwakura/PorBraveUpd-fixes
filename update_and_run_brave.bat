@echo off
setlocal
cd /d "%~dp0"

set "EDITION=%~1"
if "%EDITION%"=="" (
    "Portable Brave Updater.exe" --update=existing --launch-brave --quit-after --log "%~dp0update-and-run.log"
) else (
    "Portable Brave Updater.exe" --update=existing --launch-brave=%EDITION% --quit-after --log "%~dp0update-and-run.log"
)

set EXITCODE=%ERRORLEVEL%
echo Exit code: %EXITCODE%
pause
exit /b %EXITCODE%
