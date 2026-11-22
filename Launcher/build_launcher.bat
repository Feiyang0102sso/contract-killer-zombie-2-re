@echo off
rem Builds the launcher (vs\Launcher.sln, Release x64) with MSBuild to bin\x64\Release\ckz2.exe,
rem then copies it over ckz2.exe of a Unity build folder.
rem Usage: build_launcher.bat [build folder]
rem   With a folder (PcBuild passes Builds\Dev or Builds\Release): replace ckz2.exe there.
rem   Without one: replace ckz2.exe in every one of Builds\Dev and Builds\Release that holds a Unity build.
rem Needs Visual Studio or Build Tools with the "Desktop development with C++" workload (found with vswhere).
rem PcBuild (build_dev.bat, build_release.bat or the CKZ2/Build menu) calls this after the Unity build;
rem run it alone to update only the exe.
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
setlocal
rem Resolve the folder against the caller's directory before changing it
set "TARGET_DIR="
if not "%~1"=="" set "TARGET_DIR=%~f1"
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

if defined TARGET_DIR (
    call :replace_exe "%TARGET_DIR%"
    exit /b
)
call :replace_exe "%CD%\..\Builds\Dev" || exit /b 1
call :replace_exe "%CD%\..\Builds\Release" || exit /b 1
exit /b 0

rem Copies the launcher over <folder>\ckz2.exe; skips folders without a Unity build
:replace_exe
if not exist "%~1\UnityPlayer.dll" (
    echo [launcher] no Unity build in %~1, skipped
    exit /b 0
)
copy /y "%EXE%" "%~1\ckz2.exe" >nul
if errorlevel 1 (
    echo [launcher] cannot replace %~1\ckz2.exe - is the game running?
    exit /b 1
)
echo [launcher] replaced %~1\ckz2.exe
exit /b 0
