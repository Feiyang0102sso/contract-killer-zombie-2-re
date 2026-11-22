@echo off
rem Builds the launcher (vs\Launcher.sln, Release x64) with MSBuild to bin\x64\Release\ckz2.exe,
rem then copies it over Builds\ckz2.exe when that folder holds a Unity build.
rem Needs Visual Studio or Build Tools with the "Desktop development with C++" workload (found with vswhere).
rem PcBuild.Build (build.bat or the CKZ2/Build menu) calls this after the Unity build; run it alone to update only the exe.
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
setlocal
cd /d "%~dp0"

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo [launcher] vswhere.exe not found: install Visual Studio or Build Tools with C++.
    exit /b 1
)
set "MSBUILD="
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
if not defined MSBUILD (
    echo [launcher] no Visual Studio with the C++ x64 tools found.
    exit /b 1
)

"%MSBUILD%" "vs\Launcher.sln" /p:Configuration=Release /p:Platform=x64 /m /nologo /v:minimal
if errorlevel 1 (
    echo [launcher] MSBuild failed
    exit /b 1
)
set "EXE=%CD%\bin\x64\Release\ckz2.exe"
echo [launcher] built %EXE%

if exist "..\Builds\UnityPlayer.dll" (
    copy /y "%EXE%" "..\Builds\ckz2.exe" >nul
    if errorlevel 1 (
        echo [launcher] cannot replace Builds\ckz2.exe - is the game running?
        exit /b 1
    )
    echo [launcher] replaced Builds\ckz2.exe
)
exit /b 0
