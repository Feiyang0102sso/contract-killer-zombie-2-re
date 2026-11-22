# Launch the pilot build and have the game take screenshots by itself at the given times (-pcShots / -pcShotDir of PcBootDiagnostics), then close the game.
# The game takes the screenshots itself: it does not capture the screen, so it works even when the window is covered and never captures other programs.
# Usage: powershell -File run_and_capture.ps1 <exe> <log> <screenshot dir> <list of seconds, comma-separated>
# Optional: -Taps "40,60@0.5:0.5>0.7:0.5~3" is passed to the game's -pcAutoTap (injects taps / holds / drags at the given times; format in PcBootDiagnostics.AutoTaps).
# Optional: -FinishTutorial 50 is passed to the game's -pcFinishTutorial (skips the tutorial level at the given time).
# Optional: -PostEffect '60:LowHealth,80:Dying' is passed to the game's -pcPostEffect (forces a post-processing effect switch at the given times).
# Optional: -Mission '50:27' is passed to the game's -pcMission (enters the level of GameData.Missions[27] at the given time).
# Optional: -Effects 60 is passed to the game's -pcEffects (continuously emits hit particles at the screen center for 3 seconds starting at that second).
# Optional: -NavCheck 85 is passed to the game's -pcNavCheck (checks pathfinding from the spawn point to the waypoint at the given time).
# Optional: -CompleteMission 95 is passed to the game's -pcCompleteMission (completes the current mission via the cheat interface at the given time).
# Optional: -AddMoney '45:5000' is passed to the game's -pcAddMoney (adds soft currency at the given time).
# Optional: -Perf 30 is passed to the game's -pcPerf (records frame rate and memory every 30 seconds).
# Optional: -PathCheck 88 is passed to the game's -pcPathCheck (replays the construction of the player's route at the given time).
# Optional: -AutoAim '45:300' is passed to the game's -pcAutoAim (from 45 to 300 seconds, automatically aims at the nearest enemy and holds fire).
# Optional: -CameraWatch 2 is passed to the game's -pcCameraWatch (lists all cameras and post-processing states every 2 seconds).
# Optional: -AutoMove '90:400' is passed to the game's -pcAutoMove (from 90 to 400 seconds, holds the left stick and walks toward the mission arrow's target).
# Optional: -TaskTap 'melee=0.746:0.09' is passed to the game's -pcTaskTap (taps every 2.5 seconds while the current mission name contains the given text).
# Optional: -ZombieWatch 2 is passed to the game's -pcZombieWatch (prints the state and pathfinding state of every Unit in the scene every 2 seconds).
param([string]$Exe, [string]$Log, [string]$ShotDir, [string]$Times, [string]$Taps = '', [string]$FinishTutorial = '', [string]$PostEffect = '', [string]$Mission = '', [string]$Effects = '', [string]$NavCheck = '', [string]$CompleteMission = '', [string]$AddMoney = '', [string]$Perf = '', [string]$PathCheck = '', [string]$ZombieWatch = '', [string]$AutoAim = '', [string]$CameraWatch = '', [string]$AutoMove = '', [string]$TaskTap = '')
# The game resolves relative paths against its own working directory and the screenshots would be written to the wrong place, so convert to absolute paths first
$ShotDir = (New-Item -ItemType Directory -Force -Path $ShotDir).FullName
$Log = [System.IO.Path]::Combine((Get-Location).Path, $Log)
$arguments = @('-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', $Log, '-pcShots', $Times, '-pcShotDir', $ShotDir)
if ($Taps -ne '') { $arguments += @('-pcAutoTap', $Taps) }
if ($FinishTutorial -ne '') { $arguments += @('-pcFinishTutorial', $FinishTutorial) }
if ($PostEffect -ne '') { $arguments += @('-pcPostEffect', $PostEffect) }
if ($Mission -ne '') { $arguments += @('-pcMission', $Mission) }
if ($Effects -ne '') { $arguments += @('-pcEffects', $Effects) }
if ($NavCheck -ne '') { $arguments += @('-pcNavCheck', $NavCheck) }
if ($CompleteMission -ne '') { $arguments += @('-pcCompleteMission', $CompleteMission) }
if ($AddMoney -ne '') { $arguments += @('-pcAddMoney', $AddMoney) }
if ($Perf -ne '') { $arguments += @('-pcPerf', $Perf) }
if ($PathCheck -ne '') { $arguments += @('-pcPathCheck', $PathCheck) }
if ($ZombieWatch -ne '') { $arguments += @('-pcZombieWatch', $ZombieWatch) }
if ($AutoAim -ne '') { $arguments += @('-pcAutoAim', $AutoAim) }
if ($CameraWatch -ne '') { $arguments += @('-pcCameraWatch', $CameraWatch) }
if ($AutoMove -ne '') { $arguments += @('-pcAutoMove', $AutoMove) }
if ($TaskTap -ne '') { $arguments += @('-pcTaskTap', $TaskTap) }
$proc = Start-Process -FilePath $Exe -ArgumentList $arguments -PassThru
Write-Host "started pid $($proc.Id)"
# Wait 5 more seconds after the last screenshot time to leave time for the screenshots to be written to disk
$last = 0
foreach ($t in $Times.Split(',')) { if ([int]$t -gt $last) { $last = [int]$t } }
Start-Sleep -Seconds ($last + 5)
if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force; Write-Host "stopped" }
Get-ChildItem $ShotDir -Filter *.png | ForEach-Object { Write-Host "shot $($_.Name)" }
