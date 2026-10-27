# Changelog

All notable changes to this project are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

Nothing yet.

## [1.2.0] - 2026-10-27

One click from the editor to the phone. Building to a wireless device no longer
goes through the Build Settings *Run Device* dropdown.

### Added

- **Build & Run on Device** — builds an APK, installs it over adb and launches the
  app, in the Wireless ADB window and under
  `Tools ▸ Unity Toolbox ▸ ADB ▸ Build & Run on Device`.
- Live device resolution from `adb devices`, preferring the wireless device that
  was last connected, with a clear message for an unauthorized or missing device.
- Automatic switch to the Android build target when the project is on another one.

### Why it bypasses the Run Device dropdown

Unity's device list is populated and refreshed on its own schedule, and it is not
settable through a public API in Unity 6 (`EditorUserBuildSettings` has no
`activeAndroidDevice`). Worse, a wireless device's **port changes every time
wireless debugging is turned on**, so a saved selection is stale by definition.
Querying `adb devices` always reflects the live state, so there is nothing to select.

### Changed

- `App Build Support` (`.aab`) is temporarily disabled during a deploy build, since
  `adb install` needs an APK. The previous setting is restored afterwards.

## [1.1.0] - 2026-10-27

Wireless debugging no longer needs a terminal. The Android 11+ pairing step — a
different port, a one-time six-digit code — is now handled inside the editor.

### Added

- **Wireless ADB window** (`Tools ▸ Unity Toolbox ▸ ADB ▸ Wireless Debugging…`)
  that pairs and connects in one action.
- **mDNS discovery**: the window reads the temporary pairing port and the connect
  port from `adb mdns services`, so the only thing the user types is the six-digit
  code shown on the phone.
- **Connect automatically when Unity starts**, so a paired device reconnects on
  editor load with no input.
- `adb` auto-detection from `ANDROID_HOME`, `ANDROID_SDK_ROOT`, the Android module
  bundled with the running Unity editor, and `PATH`.
- Menu shortcuts: *Connect Now*, *Disconnect All*, *Restart ADB Server*.
- A pass-through output pane showing every adb invocation and its result.

### Changed

- The ADB tool is no longer a settings window plus a fixed `host:port` string. It
  discovers ports itself and remembers the last device between sessions.
- Pairing codes are masked in the log and never written to `EditorPrefs`.
- `adb` calls run with async output capture and a timeout, so a wedged device
  cannot freeze the editor.

### Removed

- `Editor/AutoConnectADB.cs` and `Editor/AdbSettingsWindow.cs`, replaced by
  `AdbWirelessDebuggerWindow`, `AdbCommandRunner` and `AdbAutoConnect`.

## [1.0.0] - 2026-10-27

The first formalised release. Unity Toolbox is now an installable UPM package with
`Runtime` and `Editor` assembly definitions instead of a folder of loose scripts.

### Added

- `package.json` and assembly definitions (`Adeel.UnityToolbox.Runtime`,
  `Adeel.UnityToolbox.Editor`) so the repository can be installed through the
  Package Manager via a git URL.
- `.gitignore` for Unity, plus entries for keystore files and local secret files.
- `CHANGELOG.md`, `CONTRIBUTING.md` and a CI workflow that validates the package
  structure on every push.
- Namespaces `Adeel.UnityToolbox` and `Adeel.UnityToolbox.EditorTools`.
- `ObjectPool.Return` and `ObjectPool.ReturnAll`.
- `TimeManager.StopSlowMo` and a `DoSlowMo(float)` overload.
- `PerformanceManager.SetTargetFrameRate` and Inspector-exposed settings.
- `FPSCounterController.CurrentFps` and an adjustable refresh interval.
- `AutoConnectADB` settings window with automatic ADB path detection.
- Per-sample READMEs with setup instructions.

### Changed

- `Runtime/` and `Editor/` are now separate assemblies; editor tooling no longer
  compiles into player builds.
- All editor tools moved under the **Tools ▸ Unity Toolbox** menu path.
- `SlowMoTime` is now `TimeManager.cs` (the class was already `TimeManager`).
- `AnchorSmartFixer` now accounts for the parent's pivot, so anchors are chosen
  correctly when the parent is not centred.
- `SceneBootstrapper` uses prefixed `EditorPrefs` keys, guards against an empty
  Build Settings list, and no longer restores a deleted scene.
- `FPSCounterController` refreshes its label a few times per second instead of
  every frame, and exposes `targetFrameRate` in the Inspector.
- `AutoConnectADB` no longer runs on editor load, so it cannot log an error before
  it has been configured.
- `NetworkEventManager` logs are prefixed `Network:` and the in-flight flag is
  always reset.

### Fixed

- **Player builds broke** when `AnchorSmartFixer` and `SceneBootstrapper` shipped
  under `Runtime/` while referencing `UnityEditor`. Both are now in `Editor/`.
- **`Runtime/ObjectPool`, `Runtime/SlowMoTime` and `Runtime/World To Screen Position`
  were missing their `.cs` extension**, so Unity never compiled them.
- `Singleton<T>` overwrote `Instance` with the duplicate it was destroying, and
  `SingletonPersistent<T>` called `DontDestroyOnLoad` on a destroyed object.
- `TimeManager` left `Time.fixedDeltaTime` scaled after the slow-motion effect,
  which made physics permanently mismatched.
- `ObjectPool` threw a `NullReferenceException` when every instance was in use.
- `QRCodeScanner` threw on devices without a camera instead of logging a warning.
- `PinchScrollDetection` leaked its `InputAction` instances and zoomed on a single
  touch.
- `JsonConverter` collided with `Newtonsoft.Json.JsonConverter`; the sample class is
  now `JsonSchemaMigration` and is wrapped in `#if UNITY_EDITOR`.
- `LaunchUnity.bat` no longer ships with a password value in the file.

### Removed

- `Runtime/World To Screen Position` — an incomplete code fragment with no class,
  no `using` directives and undefined members (`overLayCamera`, `canvas_rect`,
  `DOJumpAnchorPos`). It could never compile; see the DOTween/anchor snippets in
  the issues if you need that behaviour.
- `DevTools/Another File/New Text Document.txt` — an empty file.
- `Packages/FPSCounterController/` — duplicated documentation (one copy pointed at
  a non-existent image) and a `.unitypackage` blob.
- `Runtime/FirebaseManager.cs`, `Runtime/QRCodeScanner.cs`,
  `Runtime/PinchScrollDetection.cs` and `Extensions/JsonConverter.cs` moved to
  `Samples~/`, because each needs a third-party package the core must not require.

### Breaking changes

If you copied files into your own project before v1.0.0:

| Before | Now |
|---|---|
| Global namespace types (`Singleton`, `ObjectPool`, `TimeManager`, …) | `Adeel.UnityToolbox.*` — add `using Adeel.UnityToolbox;` |
| `Runtime/AnchorSmartFixer.cs`, `Runtime/SceneBootstrapper.cs` | `Editor/` |
| `SlowMoTime` file | `TimeManager.cs` (same class name) |
| Menu items under `Tools/`, `Build/`, `DesignPatterns/` | `Tools/Unity Toolbox/…` |
| Serialized field names on `ObjectPool` / `TimeManager` | Renamed, with `[FormerlySerializedAs]` preserving Inspector values |

[Unreleased]: https://github.com/AdeelGameDev/UnityToolbox/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/AdeelGameDev/UnityToolbox/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/AdeelGameDev/UnityToolbox/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/AdeelGameDev/UnityToolbox/releases/tag/v1.0.0
