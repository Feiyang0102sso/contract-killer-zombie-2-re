@echo off
rem Shared body of build_dev.bat / build_release.bat: one Unity batch build of the Windows x64 player.
rem Usage: unity_build.bat dev|release
rem   dev     -> PcBuild.BuildDev     -> Builds\Dev\ckz2.exe      (Unity Development flag on)
rem   release -> PcBuild.BuildRelease -> Builds\Release\ckz2.exe  (Development flag off, same code)
rem Asset bundles are rebuilt first; PcBuild then replaces Unity's ckz2.exe with the launcher
rem (Launcher\build_launcher.bat, needs MSVC).
rem Unity is looked up in this order: the UNITY_EXE environment variable,
rem D:\Unity\Unity <version>\Editor\Unity.exe, then the default Unity Hub location.
rem The version comes from ProjectSettings\ProjectVersion.txt. Log: RunLogs\build_<dev|release>.log.
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
setlocal
set "FLAVOR=%~1"
if /i "%FLAVOR%"=="dev" goto :flavor_dev
if /i "%FLAVOR%"=="release" goto :flavor_release
echo [build] usage: unity_build.bat dev^|release
exit /b 1

:flavor_dev
set "METHOD=PcBuild.BuildDev"
set "OUT_DIR=Builds\Dev"
goto :find_unity

:flavor_release
set "METHOD=PcBuild.BuildRelease"
set "OUT_DIR=Builds\Release"
goto :find_unity

:find_unity
rem Project root: three levels above tools\port_pc\build\
cd /d "%~dp0..\..\.."

for /f "tokens=2" %%v in ('findstr /b /c:"m_EditorVersion:" "ProjectSettings\ProjectVersion.txt"') do set "UNITY_VERSION=%%v"

if defined UNITY_EXE goto :check_unity
set "UNITY_EXE=D:\Unity\Unity %UNITY_VERSION%\Editor\Unity.exe"
if exist "%UNITY_EXE%" goto :check_unity
set "UNITY_EXE=%ProgramFiles%\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe"

:check_unity
if not exist "%UNITY_EXE%" (
    echo [build] Unity %UNITY_VERSION% not found: %UNITY_EXE%
    echo [build] Set UNITY_EXE to the full path of Unity.exe and run again.
    exit /b 1
)

rem Batch mode cannot open a project that the editor already has open.
if exist "Temp\UnityLockfile" (
    del /q "Temp\UnityLockfile" 2>nul
)
if exist "Temp\UnityLockfile" (
    echo [build] Unity has this project open. Close the editor first.
    exit /b 1
)

if not exist "RunLogs" mkdir "RunLogs"
set "LOG=%CD%\RunLogs\build_%FLAVOR%.log"
echo [build] flavor: %FLAVOR%
echo [build] Unity:  %UNITY_EXE%
echo [build] log:    %LOG%
echo [build] building... (a first run on a fresh checkout reimports everything and takes several minutes)

"%UNITY_EXE%" -batchmode -quit -projectPath "%CD%" -executeMethod %METHOD% -logFile "%LOG%"
set "EXIT_CODE=%ERRORLEVEL%"

findstr /l /c:"[PcAssetBundleRebuild] done" /c:"[PcBuild] result" /c:"[PcBuild] launcher" "%LOG%"
if not "%EXIT_CODE%"=="0" (
    echo [build] FAILED, exit code %EXIT_CODE%. See the log above.
    exit /b %EXIT_CODE%
)
echo [build] done: %CD%\%OUT_DIR%\ckz2.exe
exit /b 0
