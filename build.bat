@echo off
rem One-click build in the ckz2-build WSL distro; result in bin\ (ckzombies2 + Data\Managed\).
rem Usage: build.bat [--skip-dll]
setlocal
set DISTRO=ckz2-build
wsl.exe -d %DISTRO% -u root --cd "%~dp0." --exec bash toolchain/build.sh %*
set STATUS=%ERRORLEVEL%
if "%STATUS%"=="0" (
    echo build finished: %~dp0bin\ckzombies2
) else (
    echo build failed with exit code %STATUS%
)
pause
exit /b %STATUS%
