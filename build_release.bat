@echo off
rem One-click build: Windows x64 release build to Builds\Release\ckz2.exe (Development flag off; same code as the dev build).
rem The work is done by tools\port_pc\build\unity_build.bat; log: RunLogs\build_release.log.
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
rem The window waits for a key at the end so the result stays readable.
setlocal
call "%~dp0tools\port_pc\build\unity_build.bat" release
set "EXIT_CODE=%ERRORLEVEL%"
echo.
pause
exit /b %EXIT_CODE%
