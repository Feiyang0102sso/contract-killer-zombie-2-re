# Create the build WSL distro inside this repository and install the toolchain (called by setup.bat).
# Usage: setup_wsl.ps1 [-XcodeDmg <path>] [-Distro <name>]
#   XcodeDmg  Xcode 4.5 installer, default toolchain\downloads\Xcode_4.5.dmg
#   Distro    WSL distro name, default ckz2-build
# The distro disk lives in <repo>\.wsl\<name>, the downloaded Ubuntu image in <repo>\.cache (both git-ignored).
# The Unity 3.5.7 macOS installer is downloaded on the Windows side (WSL may not reach every download host).
param(
    [string]$XcodeDmg = "",
    [string]$Distro = "ckz2-build"
)
$ErrorActionPreference = "Stop"
# make wsl.exe print UTF-8 instead of UTF-16
$env:WSL_UTF8 = "1"

# ---- constants ----
$RootfsBase = "https://cloud-images.ubuntu.com/wsl/releases/24.04/current"
$RootfsName = "ubuntu-noble-wsl-amd64-24.04lts.rootfs.tar.gz"
$UnityUrl = "https://download.unity3d.com/download_unity/unity-3.5.7.dmg"
$UnitySize = 980625713

$Repo = Split-Path -Parent $PSScriptRoot
$CacheDir = Join-Path $Repo ".cache"
$DistroDir = Join-Path $Repo ".wsl\$Distro"
$RootfsPath = Join-Path $CacheDir $RootfsName
$DownloadDir = Join-Path $Repo "toolchain\downloads"
$UnityDmg = Join-Path $DownloadDir "unity-3.5.7.dmg"
if ($XcodeDmg -eq "") {
    $XcodeDmg = Join-Path $DownloadDir "Xcode_4.5.dmg"
}

# Fail early: the Xcode dmg cannot be downloaded automatically (Apple ID required)
if (-not (Test-Path $XcodeDmg)) {
    Write-Host "Xcode installer not found: $XcodeDmg"
    Write-Host "Download Xcode 4.5 (xcode_4.5.dmg, 4G182) from developer.apple.com"
    Write-Host "and put it at toolchain\downloads\Xcode_4.5.dmg, or run: setup.bat <path to dmg>"
    exit 1
}
if (-not (Get-Command wsl.exe -ErrorAction SilentlyContinue)) {
    Write-Host "wsl.exe not found. Install WSL first (admin PowerShell: wsl --install --no-distribution), then reboot."
    exit 1
}

# 1. distro: import the official Ubuntu 24.04 WSL image into the repository
# (wsl -l -q prints UTF-16; strip the NUL characters before comparing)
$existing = (wsl.exe -l -q) -replace "`0", "" | Where-Object { $_.Trim() -eq $Distro }
if ($existing) {
    Write-Host "== distro $Distro already exists"
} else {
    New-Item -ItemType Directory -Force $CacheDir | Out-Null
    if (-not (Test-Path $RootfsPath)) {
        Write-Host "== download $RootfsName (about 350 MB)"
        curl.exe -L --fail -o "$RootfsPath.part" "$RootfsBase/$RootfsName"
        if ($LASTEXITCODE -ne 0) { throw "download failed" }
        Move-Item -Force "$RootfsPath.part" $RootfsPath
    }
    Write-Host "== verify $RootfsName"
    $sums = (curl.exe -sL --fail "$RootfsBase/SHA256SUMS")
    if ($LASTEXITCODE -ne 0) { throw "cannot fetch SHA256SUMS" }
    $expected = ""
    foreach ($line in $sums) {
        if ($line -match "^([0-9a-f]{64})\s+\*?$([regex]::Escape($RootfsName))$") {
            $expected = $Matches[1]
        }
    }
    $actual = (Get-FileHash -Algorithm SHA256 $RootfsPath).Hash.ToLower()
    if ($expected -ne $actual) {
        # "current" may have moved on: drop the cached image so the next run downloads it again
        Remove-Item -Force $RootfsPath
        throw "sha256 mismatch for $RootfsName (expected '$expected', got $actual); run setup again"
    }
    Write-Host "== import distro $Distro -> $DistroDir"
    New-Item -ItemType Directory -Force $DistroDir | Out-Null
    wsl.exe --import $Distro $DistroDir $RootfsPath --version 2
    if ($LASTEXITCODE -ne 0) { throw "wsl --import failed" }
}

# 2. Unity 3.5.7 macOS installer (public download; its SHA-256 is checked inside the distro)
$unityPresent = $false
if (Test-Path $UnityDmg) {
    if ((Get-Item $UnityDmg).Length -eq $UnitySize) {
        $unityPresent = $true
    }
}
if ($unityPresent) {
    Write-Host "== unity-3.5.7.dmg present"
} else {
    New-Item -ItemType Directory -Force $DownloadDir | Out-Null
    Write-Host "== download unity-3.5.7.dmg (about 980 MB)"
    curl.exe -L --fail -o "$UnityDmg.part" $UnityUrl
    if ($LASTEXITCODE -ne 0) { throw "download failed" }
    Move-Item -Force "$UnityDmg.part" $UnityDmg
}

# 3. toolchain inside the distro (paths converted by wslpath)
$dmgLinux = (wsl.exe -d $Distro -u root --exec wslpath -a $XcodeDmg)
Write-Host "== toolchain setup in $Distro"
wsl.exe -d $Distro -u root --cd $Repo --exec bash toolchain/setup_distro.sh $dmgLinux
if ($LASTEXITCODE -ne 0) { throw "toolchain setup failed" }
Write-Host ""
Write-Host "Setup finished. Run build.bat to build ckzombies2."
