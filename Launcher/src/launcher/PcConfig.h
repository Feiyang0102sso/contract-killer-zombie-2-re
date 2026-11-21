// CKZ2 PC launcher: the single table of ckz2.cfg settings.
// Only the keys in kConfigEntries are read from the cfg; anything else is ignored and dropped when the file is rewritten.
// The game never reads the cfg: the launcher passes the values on the command line
// (Unity's own -screen-* arguments plus the -ckz2-* ones read by Source/Runtime/PcPort/PcLaunchArgs.cs).
#pragma once

#include <windows.h>

// ---- Registry ----
// Unity stores screen settings and PlayerPrefs under HKCU\Software\<companyName>\<productName> (Player Settings).
// The launcher deletes the game key before and after every run, so nothing stays in the registry.
const wchar_t* const kCompanyRegistryKey = L"Software\\GLU Mobile";
const wchar_t* const kGameRegistryKey = L"Software\\GLU Mobile\\CK Zombies 2";
// Value written by Unity when the window mode changes (for example Alt+Enter); the suffix is a hash of the name.
const wchar_t* const kFullscreenModeValuePrefix = L"Screenmanager Fullscreen mode_h";
// FullScreenMode.Windowed; every other mode counts as fullscreen.
const DWORD kUnityWindowedMode = 3;

// ---- Single instance ----
const wchar_t* const kInstanceMutexName = L"Local\\CKZ2Launcher";
// Window of an already running game, brought to the front instead of starting a second copy.
const wchar_t* const kUnityWindowClass = L"UnityWndClass";
const wchar_t* const kGameWindowTitle = L"CK Zombies 2";

// ---- Game process ----
// The launcher starts the same exe again with this argument; that process only runs the engine.
// (UnityMain2 never returns: the engine ends the process itself, so cleanup must happen in the parent.)
const wchar_t* const kGameProcessArgument = L"-ckz2-game";

// ---- Game arguments (names must match PcLaunchArgs.cs) ----
const wchar_t* const kGraphDetailArgument = L"-ckz2-graph-detail";
const wchar_t* const kVSyncArgument = L"-ckz2-vsync";
const wchar_t* const kDrawFpsArgument = L"-ckz2-draw-fps";

// ---- Config keys ----
enum ConfigKey
{
    ScreenX,
    ScreenY,
    FullScreen,
    VSync,
    GraphDetail,
    StartDialog,
    DrawFPS,
    ConfigKeyCount
};

struct ConfigEntry
{
    const wchar_t* section;
    const wchar_t* key;
    // 0 for ScreenX / ScreenY means "desktop resolution of the primary monitor".
    int defaultValue;
    int minValue;
    int maxValue;
    const wchar_t* comment;
};

// Order here is the order in the written cfg; entries of one section must stay together.
const ConfigEntry kConfigEntries[ConfigKeyCount] =
{
    { L"graph", L"ScreenX", 0, 640, 16384, L"Window width (windowed mode only)" },
    { L"graph", L"ScreenY", 0, 480, 16384, L"Window height (windowed mode only)" },
    { L"graph", L"FullScreen", 1, 0, 1, L"0 = windowed, 1 = borderless fullscreen at desktop resolution" },
    { L"graph", L"VSync", 1, 0, 1, L"0/1, no FPS cap when 0" },
    { L"graph", L"GraphDetail", 4, 0, 4, L"0 Lowest, 1 Low, 2 Medium, 3 High, 4 Ultra" },
    { L"game", L"StartDialog", 1, 0, 1, L"Show the start dialog (0/1)" },
    { L"game", L"DrawFPS", 0, 0, 1, L"FPS counter (0/1)" },
};

// Dialog names for GraphDetail 0..4. The quality preset behind each level is chosen in PcLaunchArgs.cs.
const wchar_t* const kGraphDetailNames[] = { L"Lowest", L"Low", L"Medium", L"High", L"Ultra" };

// ---- Resolutions offered by the dialog ----
// Only presets that fit the desktop are listed; the desktop resolution and the current cfg value are always listed.
struct Resolution
{
    int width;
    int height;
};

const Resolution kResolutionPresets[] =
{
    // 4:3
    { 1024, 768 }, { 1280, 960 }, { 1600, 1200 },
    // 16:10
    { 1280, 800 }, { 1440, 900 }, { 1680, 1050 }, { 1920, 1200 }, { 2560, 1600 },
    // 16:9
    { 1280, 720 }, { 1366, 768 }, { 1600, 900 }, { 1920, 1080 }, { 2560, 1440 }, { 3840, 2160 },
};
