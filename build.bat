@echo off
rem One-click build: Windows x64 development build to Builds\ckz2.exe (asset bundles are rebuilt first).
rem Unity is looked up in this order: the UNITY_EXE environment variable,
rem D:\Unity\Unity <version>\Editor\Unity.exe, then the default Unity Hub location.
rem The version comes from ProjectSettings\ProjectVersion.txt. Log: RunLogs\build.log.
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
rem The window waits for a key at the end so the result stays readable.
setlocal
cd /d "%~dp0"

for /f "tokens=2" %%v in ('findstr /b /c:"m_EditorVersion:" "ProjectSettings\ProjectVersion.txt"') do set "UNITY_VERSION=%%v"

if defined UNITY_EXE goto :check_unity
set "UNITY_EXE=D:\Unity\Unity %UNITY_VERSION%\Editor\Unity.exe"
if exist "%UNITY_EXE%" goto :check_unity
set "UNITY_EXE=%ProgramFiles%\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe"

:check_unity
if not exist "%UNITY_EXE%" (
    echo [build] Unity %UNITY_VERSION% not found: %UNITY_EXE%
    echo [build] Set UNITY_EXE to the full path of Unity.exe and run again.
    set "EXIT_CODE=1"
    goto :finish
)

rem Batch mode cannot open a project that the editor already has open.
if exist "Temp\UnityLockfile" (
    del /q "Temp\UnityLockfile" 2>nul
)
if exist "Temp\UnityLockfile" (
    echo [build] Unity has this project open. Close the editor first.
    set "EXIT_CODE=1"
    goto :finish
)

if not exist "RunLogs" mkdir "RunLogs"
set "LOG=%CD%\RunLogs\build.log"
echo [build] Unity: %UNITY_EXE%
echo [build] log:   %LOG%
echo [build] building... (a first run after clean.bat reimports everything and takes several minutes)

"%UNITY_EXE%" -batchmode -quit -projectPath "%CD%" -executeMethod PcPilotBuild.Build -logFile "%LOG%"
set "EXIT_CODE=%ERRORLEVEL%"

findstr /l /c:"[PcAssetBundleRebuild] done" /c:"[PcPilotBuild] result" "%LOG%"
if not "%EXIT_CODE%"=="0" (
    echo [build] FAILED, exit code %EXIT_CODE%. See the log above.
    goto :finish
)
echo [build] done: %CD%\Builds\ckz2.exe

:finish
echo.
pause
exit /b %EXIT_CODE%
