@echo off
setlocal
cd /d "%~dp0"

set "BRANCH="
for /f "delims=" %%B in ('git rev-parse --abbrev-ref HEAD 2^>nul') do set "BRANCH=%%B"
if not defined BRANCH set "BRANCH=UNKNOWN"

set "VERSION="
for /f "usebackq delims=" %%V in ("Version.txt") do set "VERSION=%%V"
if not defined VERSION set "VERSION=0.0.0"

set "DIRTY="
for /f "delims=" %%L in ('git status --porcelain 2^>nul') do set "DIRTY=1"

set "CASE=feature"
if "%BRANCH%"=="dev" set "CASE=dev"
if "%BRANCH%"=="master" set "CASE=master"
if "%BRANCH%"=="main" set "CASE=main"
if defined DIRTY set "CASE=dirty"

if "%CASE%"=="dirty" (
    echo.
    echo REM uncommitted changes - commit first
    echo git status
    echo git add -A
    echo git commit -m "COMMITMESSAGE"
    echo.
    echo git checkout dev
    echo git merge --squash %BRANCH%
    echo.
    goto :eof
)

if "%CASE%"=="feature" (
    echo.
    echo git checkout dev
    echo git merge --squash %BRANCH%
    echo.
    echo git push github dev
    echo git checkout master
    echo git merge dev
    echo git push github master
    echo git tag -a v%VERSION% -m "Release %VERSION%"
    echo git push github v%VERSION%
    echo.
    goto :eof
)

if "%CASE%"=="dev" (
    echo.
    echo git push
    echo.
    echo git checkout master
    echo git merge dev
    echo git push github master
    echo git tag -a v%VERSION% -m "Release %VERSION%"
    echo git push github v%VERSION%
    echo.
    goto :eof
)

if "%CASE%"=="master" (
    echo.
    echo git merge dev
    echo git push github master
    echo git tag -a v%VERSION% -m "Release %VERSION%"
    echo git push github v%VERSION%
    echo.
    goto :eof
)

if "%CASE%"=="main" (
    echo.
    echo git merge dev
    echo git push github main
    echo git tag -a v%VERSION% -m "Release %VERSION%"
    echo git push github v%VERSION%
    echo.
    goto :eof
)
