#!/usr/bin/env bash
# 关卡巡检：对每个关卡场景的第一个任务，启动一次游戏、直接进入该任务（-pcMission），截图并保存日志。
# 每次运行前把存档恢复成同一份（已过教程、停在地图），保证起点一致。
# 用法（在工程根目录下）：bash tools/scene_tour.sh <exe> <存档备份目录> <输出目录> [任务下标 ...]
#   例：bash tools/scene_tour.sh Builds/ckz2.exe RunLogs/save_backup_0150 RunLogs/tour1
# 任务下标来自 prefabs/basic/GameData.prefab 的 Missions（每个场景取第一个）。
set -u

EXE="$1"
SAVE_BACKUP="$2"
OUT_DIR="$3"
shift 3
# 默认：除教程关以外每个场景的第一个任务
DEFAULT_MISSIONS="0 15 27 39 51 63 75 87 99 100 112 124 136 148 149 150 151 153"
MISSIONS="${*:-$DEFAULT_MISSIONS}"

# 时间线（秒）：40 点掉地图的 "TAP TO CONTINUE"，50 进任务，75/88 点掉关卡的 "TAP TO CONTINUE"，90 起按住瞄准射击
TAPS="40,75,88,90@0.5:0.5~5,97@0.5:0.5>0.6:0.5~3"
SHOTS="72,84,96,104"
SAVE_DIR="$LOCALAPPDATA/../LocalLow/GLU Mobile/CK Zombies 2"

mkdir -p "$OUT_DIR"
for MISSION in $MISSIONS; do
	echo "[tour] mission $MISSION $(date +%H:%M:%S)"
	rm -rf "$SAVE_DIR"
	cp -r "$SAVE_BACKUP" "$SAVE_DIR"
	powershell -File tools/run_and_capture.ps1 -Exe "$EXE" -Log "$OUT_DIR/player_$MISSION.log" \
		-ShotDir "$OUT_DIR/shots_$MISSION" -Times "$SHOTS" -Taps "$TAPS" -Mission "50:$MISSION" -PathCheck 88 > /dev/null
	# 玩家路线：-pcPathCheck 重放 PlayerProgress.BuildPath，统计不完整的路段（PathPartial / PathInvalid）
	BROKEN=$(grep "path node" "$OUT_DIR/player_$MISSION.log" | grep -c "status=Path\(Partial\|Invalid\)")
	echo "[tour] mission $MISSION brokenPathSegments=$BROKEN" | tee -a "$OUT_DIR/summary.txt"
	SCENE=$(grep -o "start mission .* scene=[^ ]*" "$OUT_DIR/player_$MISSION.log" | sed 's/.*scene=//')
	EXCEPTIONS=$(grep -c "Exception" "$OUT_DIR/player_$MISSION.log")
	# 游戏自己的 log.Error 输出以 "ERROR:" 开头；按内容（去掉坐标）归类计数
	GAME_ERRORS=$(grep -c "^ERROR:" "$OUT_DIR/player_$MISSION.log")
	echo "[tour] mission $MISSION scene=$SCENE exceptions=$EXCEPTIONS gameErrors=$GAME_ERRORS" | tee -a "$OUT_DIR/summary.txt"
	grep -h "^ERROR:" "$OUT_DIR/player_$MISSION.log" | sed 's/([-0-9., ]*)/(…)/g' | sort | uniq -c | sed 's/^/    /' | tee -a "$OUT_DIR/summary.txt"
done
echo "[tour] done $(date +%H:%M:%S)"
