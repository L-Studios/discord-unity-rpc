# Changelog

All notable changes to this package are documented in this file.

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
