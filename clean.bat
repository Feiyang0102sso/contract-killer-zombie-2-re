@echo off
rem Delete intermediate files that Unity, VS and the build regenerate on their own.
rem Kept: Builds\ (build output), RunLogs\ (test logs), UserSettings\ (per-user editor layout).
rem After this, the next Unity launch does a full reimport (several minutes).
rem Comments are in English on purpose: cmd reads .bat files in the OEM code page.
setlocal
cd /d "%~dp0"

rem Unity keeps Temp\UnityLockfile locked while the project is open; deleting it fails in that case.
if exist "Temp\UnityLockfile" (
    del /q "Temp\UnityLockfile" 2>nul
)
if exist "Temp\UnityLockfile" (
    echo [clean] Unity has this project open. Close the editor first.
    exit /b 1
)

rem Unity caches
call :remove_dir "Library"
call :remove_dir "Temp"
call :remove_dir "Obj"
call :remove_dir "Logs"

rem Asset bundles: rebuilt by build.bat before every build
call :remove_dir "Assets\StreamingAssets"
call :remove_file "Assets\StreamingAssets.meta"

rem VS files that Unity generates in the project root, plus VS caches
for %%f in (*.csproj *.sln *.slnx) do call :remove_file "%%f"
call :remove_file ".vsconfig"
call :remove_dir ".vs"

rem Launcher build output and VS cache: rebuilt by build.bat (Launcher\build_launcher.bat)
call :remove_dir "Launcher\out"
call :remove_dir "Launcher\bin"
call :remove_dir "Launcher\vs\.vs"

rem Reports and Python caches
call :remove_file "PcPilotAudit.txt"
call :remove_dir "tools\__pycache__"

echo [clean] done
exit /b 0

:remove_dir
if exist "%~1\" (
    rmdir /s /q "%~1"
    echo [clean] removed %~1\
)
exit /b 0

:remove_file
if exist "%~1" (
    del /q "%~1"
    echo [clean] removed %~1
)
exit /b 0
