@echo off
rem One-click build: Windows x64 development build to Builds\Dev\ckz2.exe (Unity Development flag on: debugger, "Development Build" watermark).
rem The work is done by tools\port_pc\build\unity_build.bat; log: RunLogs\build_dev.log.
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
rem The window waits for a key at the end so the result stays readable.
setlocal
call "%~dp0tools\port_pc\build\unity_build.bat" dev
set "EXIT_CODE=%ERRORLEVEL%"
echo.
pause
exit /b %EXIT_CODE%
