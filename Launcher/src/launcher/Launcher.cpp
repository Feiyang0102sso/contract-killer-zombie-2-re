// CKZ2 PC launcher: replaces Unity's WindowsPlayer stub as ckz2.exe. One exe, two processes:
// Launcher process (started by the user):
//   1. Reads <exe name>.cfg (keys in PcConfig.h), shows the start dialog when StartDialog=1 and saves the choice.
//   2. Starts this exe again with -ckz2-game and the settings as command-line arguments, then waits for it.
//   3. When the game process ends (quit or crash), writes the window mode (Alt+Enter) back to the cfg and deletes
//      Unity's registry key.
// Game process (-ckz2-game): runs the engine (UnityMain2 in UnityPlayer.dll). UnityMain2 never returns, the engine
// ends the process itself, which is why the cleanup lives in the launcher process.
// Log: Logs\<exe name>.launcher.log next to the exe (with the game's logs), rewritten every run (launcher process only).
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include <algorithm>
#include <cstdarg>
#include <cstdio>
#include <cwchar>
#include <string>
#include <vector>

#include "PcConfig.h"
#include "resource.h"

// GPU hints and the D3D12 Agility SDK location, copied from Unity 6000.3.28f1 WindowsPlayer/Main.cpp.
// D3D12SDKVersion must match Builds/<Dev|Release>/D3D12/D3D12Core.dll: compare with Main.cpp again after a Unity upgrade.
extern "C"
{
    __declspec(dllexport) DWORD NvOptimusEnablement = 0x00000001;
    __declspec(dllexport) int AmdPowerXpressRequestHighPerformance = 1;
    __declspec(dllexport) extern const UINT D3D12SDKVersion = 618;
    __declspec(dllexport) extern const char* D3D12SDKPath = ".\\D3D12\\";
}

// Entry point exported by UnityPlayer.dll (see Unity's UnityPlayerStub/Exports.h).
typedef int (*UnityMain2Function)(HINSTANCE instance, LPCWSTR customDataFolder, LPWSTR commandLine, int showCommand);

// Width of "Key=Value" before the comment in the written cfg.
const size_t kCommentColumn = 20;

// Current value of every config key.
static int g_values[ConfigKeyCount];
static FILE* g_logFile = nullptr;

// ---------------------------------------------------------------- Logging

static void Log(const wchar_t* format, ...)
{
    if (g_logFile == nullptr)
    {
        return;
    }
    va_list arguments;
    va_start(arguments, format);
    vfwprintf(g_logFile, format, arguments);
    va_end(arguments);
    fputwc(L'\n', g_logFile);
    fflush(g_logFile);
}

// ---------------------------------------------------------------- Helpers

static std::wstring Trim(const std::wstring& text)
{
    size_t first = text.find_first_not_of(L" \t\r\n");
    if (first == std::wstring::npos)
    {
        return std::wstring();
    }
    size_t last = text.find_last_not_of(L" \t\r\n");
    return text.substr(first, last - first + 1);
}

/// Parses a whole string as a decimal integer; false for empty text or trailing garbage.
static bool ParseInt(const std::wstring& text, int* value)
{
    if (text.empty())
    {
        return false;
    }
    wchar_t* end = nullptr;
    long parsed = wcstol(text.c_str(), &end, 10);
    if (*end != L'\0')
    {
        return false;
    }
    *value = static_cast<int>(parsed);
    return true;
}

/// Exe path without the ".exe" extension, e.g. "E:\Game\ckz2".
static std::wstring GetExeBasePath()
{
    wchar_t buffer[MAX_PATH];
    DWORD length = GetModuleFileNameW(nullptr, buffer, MAX_PATH);
    std::wstring path(buffer, length);
    size_t dot = path.find_last_of(L'.');
    size_t slash = path.find_last_of(L"\\/");
    if (dot != std::wstring::npos && dot > slash)
    {
        path.resize(dot);
    }
    return path;
}

static std::wstring GetDirectory(const std::wstring& path)
{
    size_t slash = path.find_last_of(L"\\/");
    return path.substr(0, slash);
}

/// Resolution of the primary monitor in physical pixels.
static Resolution GetDesktopResolution()
{
    DEVMODEW mode = {};
    mode.dmSize = sizeof(mode);
    EnumDisplaySettingsW(nullptr, ENUM_CURRENT_SETTINGS, &mode);
    Resolution desktop;
    desktop.width = static_cast<int>(mode.dmPelsWidth);
    desktop.height = static_cast<int>(mode.dmPelsHeight);
    return desktop;
}

// ---------------------------------------------------------------- Config file

static int GetDefaultValue(int key)
{
    if (key == ScreenX)
    {
        return GetDesktopResolution().width;
    }
    if (key == ScreenY)
    {
        return GetDesktopResolution().height;
    }
    return kConfigEntries[key].defaultValue;
}

/// Index of the entry for section/key (case-insensitive), or -1.
static int FindConfigKey(const std::wstring& section, const std::wstring& key)
{
    for (int i = 0; i < ConfigKeyCount; i++)
    {
        bool sameSection = _wcsicmp(section.c_str(), kConfigEntries[i].section) == 0;
        bool sameKey = _wcsicmp(key.c_str(), kConfigEntries[i].key) == 0;
        if (sameSection && sameKey)
        {
            return i;
        }
    }
    return -1;
}

/// Loads the cfg into g_values. Returns true when the file should be rewritten
/// (missing file, missing keys, invalid values or unknown lines).
static bool LoadConfig(const std::wstring& path)
{
    for (int i = 0; i < ConfigKeyCount; i++)
    {
        g_values[i] = GetDefaultValue(i);
    }

    FILE* file = _wfopen(path.c_str(), L"rb");
    if (file == nullptr)
    {
        Log(L"[config] %ls not found, using defaults", path.c_str());
        return true;
    }
    std::string bytes;
    char chunk[4096];
    size_t count = fread(chunk, 1, sizeof(chunk), file);
    while (count > 0)
    {
        bytes.append(chunk, count);
        count = fread(chunk, 1, sizeof(chunk), file);
    }
    fclose(file);

    // UTF-8, with or without BOM
    if (bytes.size() >= 3 && bytes.compare(0, 3, "\xEF\xBB\xBF") == 0)
    {
        bytes.erase(0, 3);
    }
    std::wstring text;
    if (!bytes.empty())
    {
        int length = MultiByteToWideChar(CP_UTF8, 0, bytes.data(), static_cast<int>(bytes.size()), nullptr, 0);
        text.resize(length);
        MultiByteToWideChar(CP_UTF8, 0, bytes.data(), static_cast<int>(bytes.size()), &text[0], length);
    }

    bool needsSave = false;
    bool found[ConfigKeyCount] = {};
    std::wstring section;
    size_t lineStart = 0;
    int lineNumber = 0;
    while (lineStart <= text.size())
    {
        size_t lineEnd = text.find(L'\n', lineStart);
        if (lineEnd == std::wstring::npos)
        {
            lineEnd = text.size();
        }
        std::wstring line = text.substr(lineStart, lineEnd - lineStart);
        lineStart = lineEnd + 1;
        lineNumber++;

        size_t commentStart = line.find(L';');
        if (commentStart != std::wstring::npos)
        {
            line.resize(commentStart);
        }
        line = Trim(line);
        if (line.empty())
        {
            continue;
        }
        if (line.front() == L'[' && line.back() == L']')
        {
            section = Trim(line.substr(1, line.size() - 2));
            continue;
        }
        size_t equals = line.find(L'=');
        if (equals == std::wstring::npos)
        {
            Log(L"[config] line %d ignored: %ls", lineNumber, line.c_str());
            needsSave = true;
            continue;
        }
        std::wstring key = Trim(line.substr(0, equals));
        std::wstring valueText = Trim(line.substr(equals + 1));
        int index = FindConfigKey(section, key);
        if (index < 0)
        {
            Log(L"[config] line %d ignored, unknown key [%ls] %ls", lineNumber, section.c_str(), key.c_str());
            needsSave = true;
            continue;
        }
        int value = 0;
        bool valid = ParseInt(valueText, &value);
        if (valid && (value < kConfigEntries[index].minValue || value > kConfigEntries[index].maxValue))
        {
            valid = false;
        }
        if (!valid)
        {
            Log(L"[config] line %d: %ls=%ls is invalid, using %d", lineNumber, key.c_str(), valueText.c_str(), g_values[index]);
            needsSave = true;
            continue;
        }
        g_values[index] = value;
        found[index] = true;
    }

    for (int i = 0; i < ConfigKeyCount; i++)
    {
        if (!found[i])
        {
            Log(L"[config] %ls missing, using %d", kConfigEntries[i].key, g_values[i]);
            needsSave = true;
        }
    }
    return needsSave;
}

/// Writes g_values as a commented cfg (UTF-8, CRLF).
static void SaveConfig(const std::wstring& path)
{
    std::wstring text;
    text += L"; CK Zombies 2 PC config, read by the exe of the same name at every launch.\r\n";
    text += L"; Only the keys below are used; anything else is ignored and removed.\r\n";
    const wchar_t* section = L"";
    for (int i = 0; i < ConfigKeyCount; i++)
    {
        const ConfigEntry& entry = kConfigEntries[i];
        if (wcscmp(section, entry.section) != 0)
        {
            section = entry.section;
            text += L"\r\n[";
            text += section;
            text += L"]\r\n";
        }
        std::wstring line = entry.key;
        line += L"=";
        line += std::to_wstring(g_values[i]);
        while (line.size() < kCommentColumn)
        {
            line += L' ';
        }
        line += L"; ";
        line += entry.comment;
        text += line;
        text += L"\r\n";
    }

    int length = WideCharToMultiByte(CP_UTF8, 0, text.data(), static_cast<int>(text.size()), nullptr, 0, nullptr, nullptr);
    std::string bytes(length, '\0');
    WideCharToMultiByte(CP_UTF8, 0, text.data(), static_cast<int>(text.size()), &bytes[0], length, nullptr, nullptr);

    FILE* file = _wfopen(path.c_str(), L"wb");
    if (file == nullptr)
    {
        Log(L"[config] cannot write %ls", path.c_str());
        return;
    }
    fwrite(bytes.data(), 1, bytes.size(), file);
    fclose(file);
    Log(L"[config] saved %ls", path.c_str());
}

// ---------------------------------------------------------------- Start dialog

// Resolutions shown in the dialog, in combo box order.
static std::vector<Resolution> g_dialogResolutions;

static bool IsSmaller(const Resolution& left, const Resolution& right)
{
    if (left.width != right.width)
    {
        return left.width < right.width;
    }
    return left.height < right.height;
}

static void AddResolution(int width, int height)
{
    for (const Resolution& existing : g_dialogResolutions)
    {
        if (existing.width == width && existing.height == height)
        {
            return;
        }
    }
    Resolution resolution;
    resolution.width = width;
    resolution.height = height;
    g_dialogResolutions.push_back(resolution);
}

static void FillResolutionList(HWND dialog)
{
    Resolution desktop = GetDesktopResolution();
    g_dialogResolutions.clear();
    for (const Resolution& preset : kResolutionPresets)
    {
        if (preset.width <= desktop.width && preset.height <= desktop.height)
        {
            AddResolution(preset.width, preset.height);
        }
    }
    AddResolution(desktop.width, desktop.height);
    AddResolution(g_values[ScreenX], g_values[ScreenY]);
    std::sort(g_dialogResolutions.begin(), g_dialogResolutions.end(), IsSmaller);

    HWND combo = GetDlgItem(dialog, IDC_RESOLUTION);
    for (size_t i = 0; i < g_dialogResolutions.size(); i++)
    {
        const Resolution& resolution = g_dialogResolutions[i];
        wchar_t label[64];
        swprintf(label, 64, L"%d x %d", resolution.width, resolution.height);
        std::wstring text = label;
        if (resolution.width == desktop.width && resolution.height == desktop.height)
        {
            text += L" (desktop)";
        }
        SendMessageW(combo, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(text.c_str()));
        if (resolution.width == g_values[ScreenX] && resolution.height == g_values[ScreenY])
        {
            SendMessageW(combo, CB_SETCURSEL, i, 0);
        }
    }
}

static void SetCheck(HWND dialog, int controlId, bool checked)
{
    UINT state = BST_UNCHECKED;
    if (checked)
    {
        state = BST_CHECKED;
    }
    CheckDlgButton(dialog, controlId, state);
}

static bool GetCheck(HWND dialog, int controlId)
{
    return IsDlgButtonChecked(dialog, controlId) == BST_CHECKED;
}

/// The window size only applies to windowed mode, so the list is greyed out while Fullscreen is checked.
static void UpdateResolutionEnabled(HWND dialog)
{
    bool windowed = !GetCheck(dialog, IDC_FULLSCREEN);
    EnableWindow(GetDlgItem(dialog, IDC_RESOLUTION), windowed);
}

static void InitDialog(HWND dialog)
{
    HINSTANCE instance = GetModuleHandleW(nullptr);
    HICON bigIcon = static_cast<HICON>(LoadImageW(instance, MAKEINTRESOURCEW(IDI_APP), IMAGE_ICON,
        GetSystemMetrics(SM_CXICON), GetSystemMetrics(SM_CYICON), 0));
    HICON smallIcon = static_cast<HICON>(LoadImageW(instance, MAKEINTRESOURCEW(IDI_APP), IMAGE_ICON,
        GetSystemMetrics(SM_CXSMICON), GetSystemMetrics(SM_CYSMICON), 0));
    SendMessageW(dialog, WM_SETICON, ICON_BIG, reinterpret_cast<LPARAM>(bigIcon));
    SendMessageW(dialog, WM_SETICON, ICON_SMALL, reinterpret_cast<LPARAM>(smallIcon));

    FillResolutionList(dialog);

    HWND detailCombo = GetDlgItem(dialog, IDC_GRAPH_DETAIL);
    for (const wchar_t* name : kGraphDetailNames)
    {
        SendMessageW(detailCombo, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(name));
    }
    SendMessageW(detailCombo, CB_SETCURSEL, g_values[GraphDetail], 0);

    SetDlgItemTextW(dialog, IDC_LANGUAGE_LABEL, kLanguageLabel);
    HWND languageCombo = GetDlgItem(dialog, IDC_LANGUAGE);
    for (const wchar_t* name : kLanguageNames)
    {
        SendMessageW(languageCombo, CB_ADDSTRING, 0, reinterpret_cast<LPARAM>(name));
    }
    SendMessageW(languageCombo, CB_SETCURSEL, g_values[Language], 0);

    SetCheck(dialog, IDC_FULLSCREEN, g_values[FullScreen] == 1);
    SetCheck(dialog, IDC_VSYNC, g_values[VSync] == 1);
    SetCheck(dialog, IDC_DRAW_FPS, g_values[DrawFPS] == 1);
    SetCheck(dialog, IDC_HIDE_DIALOG, g_values[StartDialog] == 0);
    UpdateResolutionEnabled(dialog);
}

/// Copies the dialog state into g_values.
static void ReadDialog(HWND dialog)
{
    LRESULT resolutionIndex = SendDlgItemMessageW(dialog, IDC_RESOLUTION, CB_GETCURSEL, 0, 0);
    if (resolutionIndex >= 0 && resolutionIndex < static_cast<LRESULT>(g_dialogResolutions.size()))
    {
        g_values[ScreenX] = g_dialogResolutions[resolutionIndex].width;
        g_values[ScreenY] = g_dialogResolutions[resolutionIndex].height;
    }
    LRESULT detailIndex = SendDlgItemMessageW(dialog, IDC_GRAPH_DETAIL, CB_GETCURSEL, 0, 0);
    if (detailIndex >= 0)
    {
        g_values[GraphDetail] = static_cast<int>(detailIndex);
    }
    LRESULT languageIndex = SendDlgItemMessageW(dialog, IDC_LANGUAGE, CB_GETCURSEL, 0, 0);
    if (languageIndex >= 0)
    {
        g_values[Language] = static_cast<int>(languageIndex);
    }
    g_values[FullScreen] = GetCheck(dialog, IDC_FULLSCREEN);
    g_values[VSync] = GetCheck(dialog, IDC_VSYNC);
    g_values[DrawFPS] = GetCheck(dialog, IDC_DRAW_FPS);
    g_values[StartDialog] = !GetCheck(dialog, IDC_HIDE_DIALOG);
}

static INT_PTR CALLBACK DialogProc(HWND dialog, UINT message, WPARAM wParam, LPARAM /*lParam*/)
{
    if (message == WM_INITDIALOG)
    {
        InitDialog(dialog);
        return TRUE;
    }
    if (message == WM_COMMAND)
    {
        int command = LOWORD(wParam);
        if (command == IDC_FULLSCREEN && HIWORD(wParam) == BN_CLICKED)
        {
            UpdateResolutionEnabled(dialog);
            return TRUE;
        }
        if (command == IDOK)
        {
            ReadDialog(dialog);
            EndDialog(dialog, IDOK);
            return TRUE;
        }
        if (command == IDCANCEL)
        {
            EndDialog(dialog, IDCANCEL);
            return TRUE;
        }
    }
    return FALSE;
}

// ---------------------------------------------------------------- Registry

static void DeleteUnityRegistryKey()
{
    LSTATUS status = RegDeleteTreeW(HKEY_CURRENT_USER, kGameRegistryKey);
    if (status == ERROR_SUCCESS)
    {
        Log(L"[registry] deleted HKCU\\%ls", kGameRegistryKey);
    }
    // Only succeeds when the company key is empty, so other games' settings are never touched.
    RegDeleteKeyW(HKEY_CURRENT_USER, kCompanyRegistryKey);
}

/// Reads the window mode Unity saved (Alt+Enter changes it) and stores it in FullScreen. Returns true on change.
static bool ReadBackFullScreen()
{
    HKEY key = nullptr;
    if (RegOpenKeyExW(HKEY_CURRENT_USER, kGameRegistryKey, 0, KEY_READ, &key) != ERROR_SUCCESS)
    {
        Log(L"[registry] no game key, window mode not read back");
        return false;
    }
    size_t prefixLength = wcslen(kFullscreenModeValuePrefix);
    bool found = false;
    DWORD mode = 0;
    for (DWORD index = 0; ; index++)
    {
        wchar_t name[256];
        DWORD nameLength = 256;
        DWORD type = 0;
        DWORD data = 0;
        DWORD dataSize = sizeof(data);
        LSTATUS status = RegEnumValueW(key, index, name, &nameLength, nullptr, &type, reinterpret_cast<BYTE*>(&data), &dataSize);
        if (status == ERROR_NO_MORE_ITEMS)
        {
            break;
        }
        if (status != ERROR_SUCCESS || type != REG_DWORD)
        {
            continue;
        }
        if (wcsncmp(name, kFullscreenModeValuePrefix, prefixLength) == 0)
        {
            mode = data;
            found = true;
            break;
        }
    }
    RegCloseKey(key);
    if (!found)
    {
        Log(L"[registry] window mode value not found");
        return false;
    }

    int fullScreen = 1;
    if (mode == kUnityWindowedMode)
    {
        fullScreen = 0;
    }
    Log(L"[registry] window mode at exit %lu -> FullScreen=%d", mode, fullScreen);
    if (fullScreen == g_values[FullScreen])
    {
        return false;
    }
    g_values[FullScreen] = fullScreen;
    return true;
}

// ---------------------------------------------------------------- Game start

/// Command line of the game process: exe, game marker, the user's own arguments, then the settings.
static std::wstring BuildGameCommandLine(const std::wstring& exePath, const wchar_t* originalCommandLine)
{
    std::wstring commandLine = L"\"" + exePath + L"\" " + kGameProcessArgument + L" " + originalCommandLine;
    // Fullscreen always renders at the desktop resolution (what Alt+Enter gives); ScreenX/ScreenY are the window size.
    int width = g_values[ScreenX];
    int height = g_values[ScreenY];
    if (g_values[FullScreen] == 1)
    {
        Resolution desktop = GetDesktopResolution();
        width = desktop.width;
        height = desktop.height;
    }
    wchar_t arguments[512];
    swprintf(arguments, 512, L" -screen-width %d -screen-height %d -screen-fullscreen %d",
        width, height, g_values[FullScreen]);
    commandLine += arguments;
    if (g_values[FullScreen] == 1)
    {
        commandLine += L" -window-mode borderless";
    }
    swprintf(arguments, 512, L" %ls %d %ls %d %ls %d %ls %ls",
        kGraphDetailArgument, g_values[GraphDetail],
        kVSyncArgument, g_values[VSync],
        kDrawFpsArgument, g_values[DrawFPS],
        kLanguageArgument, kLanguageLocales[g_values[Language]]);
    commandLine += arguments;
    return Trim(commandLine);
}

/// Brings an already running game to the front. Returns true when another instance owns the mutex.
static bool IsAlreadyRunning()
{
    CreateMutexW(nullptr, TRUE, kInstanceMutexName);
    if (GetLastError() != ERROR_ALREADY_EXISTS)
    {
        return false;
    }
    HWND gameWindow = FindWindowW(kUnityWindowClass, kGameWindowTitle);
    if (gameWindow != nullptr)
    {
        ShowWindow(gameWindow, SW_RESTORE);
        SetForegroundWindow(gameWindow);
    }
    return true;
}

/// Game process: runs the engine in this process. Does not return when the engine starts.
static int RunEngine(HINSTANCE instance, LPWSTR commandLine, int showCommand)
{
    std::wstring playerPath = GetDirectory(GetExeBasePath()) + L"\\UnityPlayer.dll";
    HMODULE player = LoadLibraryW(playerPath.c_str());
    if (player == nullptr)
    {
        MessageBoxW(nullptr, L"UnityPlayer.dll was not found next to the game exe.", kGameWindowTitle, MB_ICONERROR);
        return 1;
    }
    UnityMain2Function unityMain2 = reinterpret_cast<UnityMain2Function>(GetProcAddress(player, "UnityMain2"));
    if (unityMain2 == nullptr)
    {
        MessageBoxW(nullptr, L"UnityPlayer.dll does not export UnityMain2.", kGameWindowTitle, MB_ICONERROR);
        return 1;
    }
    return unityMain2(instance, nullptr, commandLine, showCommand);
}

/// Launcher process: cfg, dialog, start and wait for the game process, then write back and clean up.
static int RunLauncher(LPWSTR commandLine)
{
    std::wstring basePath = GetExeBasePath();
    std::wstring exePath = basePath + L".exe";
    std::wstring configPath = basePath + L".cfg";
    // Logs\ next to the exe, shared with the game's logs (PcPaths.LogDir)
    std::wstring exeDirectory = GetDirectory(basePath);
    std::wstring logDirectory = exeDirectory + L"\\" + kLogDirectoryName;
    CreateDirectoryW(logDirectory.c_str(), nullptr);
    std::wstring logPath = logDirectory + basePath.substr(exeDirectory.size()) + L".launcher.log";
    g_logFile = _wfopen(logPath.c_str(), L"w, ccs=UTF-8");
    Log(L"[launcher] start, command line: %ls", commandLine);

    if (IsAlreadyRunning())
    {
        Log(L"[launcher] already running, focused the existing window");
        return 0;
    }

    bool needsSave = LoadConfig(configPath);
    if (g_values[StartDialog] == 1)
    {
        INT_PTR result = DialogBoxParamW(GetModuleHandleW(nullptr), MAKEINTRESOURCEW(IDD_LAUNCHER), nullptr, DialogProc, 0);
        if (result != IDOK)
        {
            Log(L"[launcher] dialog closed without playing");
            return 0;
        }
        needsSave = true;
    }
    if (needsSave)
    {
        SaveConfig(configPath);
    }

    // Leftovers of an earlier run would otherwise compete with the cfg.
    DeleteUnityRegistryKey();

    std::wstring gameCommandLine = BuildGameCommandLine(exePath, commandLine);
    Log(L"[launcher] game command line: %ls", gameCommandLine.c_str());
    // CreateProcessW needs a writable buffer.
    std::vector<wchar_t> buffer(gameCommandLine.begin(), gameCommandLine.end());
    buffer.push_back(L'\0');
    STARTUPINFOW startupInfo = {};
    startupInfo.cb = sizeof(startupInfo);
    PROCESS_INFORMATION processInfo = {};
    if (!CreateProcessW(exePath.c_str(), buffer.data(), nullptr, nullptr, FALSE, 0, nullptr, nullptr, &startupInfo, &processInfo))
    {
        Log(L"[launcher] cannot start the game process (error %lu)", GetLastError());
        MessageBoxW(nullptr, L"Cannot start the game.", kGameWindowTitle, MB_ICONERROR);
        return 1;
    }
    CloseHandle(processInfo.hThread);
    WaitForSingleObject(processInfo.hProcess, INFINITE);
    DWORD exitCode = 0;
    GetExitCodeProcess(processInfo.hProcess, &exitCode);
    CloseHandle(processInfo.hProcess);
    Log(L"[launcher] game exited with code 0x%08lX", exitCode);

    if (ReadBackFullScreen())
    {
        SaveConfig(configPath);
    }
    DeleteUnityRegistryKey();
    Log(L"[launcher] done");
    if (g_logFile != nullptr)
    {
        fclose(g_logFile);
    }
    return static_cast<int>(exitCode);
}

int WINAPI wWinMain(HINSTANCE instance, HINSTANCE, LPWSTR commandLine, int showCommand)
{
    if (wcsstr(commandLine, kGameProcessArgument) != nullptr)
    {
        return RunEngine(instance, commandLine, showCommand);
    }
    return RunLauncher(commandLine);
}
