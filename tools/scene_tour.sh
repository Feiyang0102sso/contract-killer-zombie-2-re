#!/usr/bin/env bash
# Level tour: for the first mission of each level scene, launch the game once, enter that mission directly (-pcMission), take screenshots and keep the log.
# Before each run the save is restored to the same copy (past the tutorial, on the map) so every run starts the same way.
# Usage (from the project root): bash tools/scene_tour.sh <exe> <save backup dir> <output dir> [mission index ...]
#   Example: bash tools/scene_tour.sh Builds/ckz2.exe RunLogs/save_backup_0150 RunLogs/tour1
# Mission indices come from Missions in prefabs/basic/GameData.prefab (the first one of each scene).
set -u

EXE="$1"
SAVE_BACKUP="$2"
OUT_DIR="$3"
shift 3
# Default: the first mission of every scene except the tutorial level
DEFAULT_MISSIONS="0 15 27 39 51 63 75 87 99 100 112 124 136 148 149 150 151 153"
MISSIONS="${*:-$DEFAULT_MISSIONS}"

# Timeline (seconds): 40 dismisses the map's "TAP TO CONTINUE", 50 enters the mission, 75/88 dismiss the level's "TAP TO CONTINUE", from 90 hold to aim and shoot
TAPS="40,75,88,90@0.5:0.5~5,97@0.5:0.5>0.6:0.5~3"
SHOTS="72,84,96,104"
# Saves are in the Save folder next to the exe (StorageManager.PersistentDataPath)
SAVE_DIR="$(dirname "$EXE")/Save"

mkdir -p "$OUT_DIR"
for MISSION in $MISSIONS; do
	echo "[tour] mission $MISSION $(date +%H:%M:%S)"
	rm -rf "$SAVE_DIR"
	cp -r "$SAVE_BACKUP" "$SAVE_DIR"
	powershell -File tools/run_and_capture.ps1 -Exe "$EXE" -Log "$OUT_DIR/player_$MISSION.log" \
		-ShotDir "$OUT_DIR/shots_$MISSION" -Times "$SHOTS" -Taps "$TAPS" -Mission "50:$MISSION" -PathCheck 88 > /dev/null
	# Player route: -pcPathCheck replays PlayerProgress.BuildPath; count incomplete segments (PathPartial / PathInvalid)
	BROKEN=$(grep "path node" "$OUT_DIR/player_$MISSION.log" | grep -c "status=Path\(Partial\|Invalid\)")
	echo "[tour] mission $MISSION brokenPathSegments=$BROKEN" | tee -a "$OUT_DIR/summary.txt"
	SCENE=$(grep -o "start mission .* scene=[^ ]*" "$OUT_DIR/player_$MISSION.log" | sed 's/.*scene=//')
	EXCEPTIONS=$(grep -c "Exception" "$OUT_DIR/player_$MISSION.log")
	# The game's own log.Error output starts with "ERROR:"; group and count by content (coordinates stripped)
	GAME_ERRORS=$(grep -c "^ERROR:" "$OUT_DIR/player_$MISSION.log")
	echo "[tour] mission $MISSION scene=$SCENE exceptions=$EXCEPTIONS gameErrors=$GAME_ERRORS" | tee -a "$OUT_DIR/summary.txt"
	grep -h "^ERROR:" "$OUT_DIR/player_$MISSION.log" | sed 's/([-0-9., ]*)/(…)/g' | sort | uniq -c | sed 's/^/    /' | tee -a "$OUT_DIR/summary.txt"
done
echo "[tour] done $(date +%H:%M:%S)"
