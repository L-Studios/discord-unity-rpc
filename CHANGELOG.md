# Changelog

All notable changes to this package are documented in this file.

## [1.1.1] - 2026-09-16

### Added

- A default **Get Unity Rich Presence** button that links to `https://github.com/L-Studios/discord-unity-rpc`. It applies to projects that have never saved a Button 1 value, and clearing it in the settings keeps it cleared.

### Fixed

- The **Verbose** log level did nothing. It now logs to the Unity Console when Discord connects or disconnects, when presence is cleared, and each published state with its button labels. URLs and payload JSON are never logged.

### Documentation

- Explained that Discord never shows your own Rich Presence buttons to you; other users see them on your profile.

## [1.1.0] - 2026-09-14

### Added

- **Player build status:** presence shows `Building for {platform}` as soon as a player build starts, and returns to normal when it succeeds, fails, or is cancelled.
- **Idle state:** presence shows `Idle` after Unity has been unfocused for a configurable number of minutes (default 5, `0` disables it). Idle never hides a build or script compilation, and it does not undo **Clear Presence**.
- **Active tool:** the Animator, Animation, Timeline, Shader Graph, VFX Graph, Tile Palette, Terrain, Profiler, Sprite Editor, and UI Builder windows replace the editing state (for example `Editing a Timeline`). Utility windows such as the Inspector keep the last tool, so presence does not flicker.
- **Build target icon:** the small image shows the active build target for Windows, Linux, Android, iOS, and WebGL.
- New **Activity** settings: **Show active tool**, **Show build target icon**, and **Idle after (minutes)**.

## [1.0.1] - 2026-09-14

### Added

- **Window > Discord Unity RPC** editor window with the same settings, connection status, and **Clear Presence** action as Preferences.

### Fixed

- Added stable `.meta` files for every importable package file and folder. Without them, Unity ignored the package when installed from a git URL and never compiled its code.
- The EditMode test assembly now references `DiscordRPC.dll` and NUnit explicitly, so package tests compile once Unity imports them.
- The package validator now fails when an importable file or folder has no `.meta`, when a `.meta` is orphaned, or when a GUID is invalid or duplicated.

## [1.0.0] - 2026-09-14

### Added

- Initial Unity Editor Rich Presence integration.
- Unity 2019.4 LTS through Unity 6 version-aware artwork.
- Per-user and per-project privacy controls.
- Scene, Prefab Mode, Play Mode, and compilation context tracking.
- One-second debounce, duplicate suppression, and bounded reconnection.
- Two optional, validated HTTPS Rich Presence buttons.
- Editor-only package boundaries and deterministic reload/quit cleanup.
