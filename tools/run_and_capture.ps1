# 启动试点包，让游戏在指定时间点自己截图（PcBootDiagnostics 的 -pcShots / -pcShotDir），最后关闭游戏。
# 截图由游戏自己完成：不截屏幕，窗口被遮挡也能截到，也不会截到其他程序。
# 用法：powershell -File run_and_capture.ps1 <exe> <日志> <截图目录> <秒数列表，逗号分隔>
# 可选：-Taps "40,60@0.5:0.5>0.7:0.5~3" 转给游戏的 -pcAutoTap（到点注入点击 / 按住 / 拖动，格式见 PcBootDiagnostics.AutoTaps）。
# 可选：-FinishTutorial 50 转给游戏的 -pcFinishTutorial（到点跳过教程关）。
# 可选：-PostEffect '60:LowHealth,80:Dying' 转给游戏的 -pcPostEffect（到点强制切换后处理效果）。
# 可选：-Mission '50:27' 转给游戏的 -pcMission（到点进入 GameData.Missions[27] 的关卡）。
# 可选：-Effects 60 转给游戏的 -pcEffects（从该秒起 3 秒内在屏幕中心连续发射击中粒子）。
# 可选：-NavCheck 85 转给游戏的 -pcNavCheck（到点检查出生点到路径点的寻路）。
# 可选：-CompleteMission 95 转给游戏的 -pcCompleteMission（到点用作弊接口完成当前任务）。
# 可选：-AddMoney '45:5000' 转给游戏的 -pcAddMoney（到点加软货币）。
# 可选：-Perf 30 转给游戏的 -pcPerf（每隔 30 秒记录帧率和内存）。
# 可选：-PathCheck 88 转给游戏的 -pcPathCheck（到点重放玩家路线的构建过程）。
# 可选：-AutoAim '45:300' 转给游戏的 -pcAutoAim（45～300 秒内自动瞄准最近的敌人并按住开火）。
# 可选：-CameraWatch 2 转给游戏的 -pcCameraWatch（每隔 2 秒列出所有相机和后处理状态）。
# 可选：-AutoMove '90:400' 转给游戏的 -pcAutoMove（90～400 秒内按住左摇杆朝任务箭头的目标走）。
# 可选：-TaskTap 'melee=0.746:0.09' 转给游戏的 -pcTaskTap（当前任务名包含指定文字时每 2.5 秒点一下）。
# 可选：-ZombieWatch 2 转给游戏的 -pcZombieWatch（每隔 2 秒打印场景里每个 Unit 的状态和寻路状态）。
param([string]$Exe, [string]$Log, [string]$ShotDir, [string]$Times, [string]$Taps = '', [string]$FinishTutorial = '', [string]$PostEffect = '', [string]$Mission = '', [string]$Effects = '', [string]$NavCheck = '', [string]$CompleteMission = '', [string]$AddMoney = '', [string]$Perf = '', [string]$PathCheck = '', [string]$ZombieWatch = '', [string]$AutoAim = '', [string]$CameraWatch = '', [string]$AutoMove = '', [string]$TaskTap = '')
# 游戏按自己的工作目录解析相对路径，截图会写丢，所以先转成绝对路径
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
# 最后一个截图时间点之后再多等 5 秒，给截图写盘留时间
$last = 0
foreach ($t in $Times.Split(',')) { if ([int]$t -gt $last) { $last = [int]$t } }
Start-Sleep -Seconds ($last + 5)
if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force; Write-Host "stopped" }
Get-ChildItem $ShotDir -Filter *.png | ForEach-Object { Write-Host "shot $($_.Name)" }
