# Discord Unity RPC documentation

## Installation and first run

Install `https://github.com/L-Studios/discord-unity-rpc.git` through Unity Package Manager's **Add package from git URL** action. The package requires Unity 2019.4 or newer.

Open **Window > Discord Unity RPC**, or **Edit > Preferences > L.Studios > Discord Unity RPC**. Both show the same settings. Rich Presence is off until the current user explicitly enables it for the current project. Enabling one project does not enable another.

## Preferences

The window and the Preferences page edit the same stored settings and show the same connection status.


- **Enable Rich Presence:** creates the local Discord IPC connection and begins publishing editor context.
- **Show project name:** uses `Working on {project}`; otherwise uses `Working in Unity`.
- **Show scene name:** controls scene names in editing and Play Mode states.
- **Show prefab name:** controls the current Prefab Mode asset name.
- **Show elapsed session time:** includes the stable Unity Editor session start time.
- **Show active tool:** replaces the editing state with the focused tool (see [Active tool](#active-tool)).
- **Show build target icon:** sends the active build target as the small image (see [Artwork](#artwork)).
- **Idle after (minutes):** `0`–`120`, default `5`. After Unity has been unfocused this long, the state becomes `Idle`. `0` disables Idle.
- **Log level:** `Off`, `Errors`, or `Verbose`. Errors are rate-limited and logs never contain credentials or payload JSON.
- **Buttons:** up to two optional label/URL pairs. A button is transmitted only when its label is non-empty and its URL is absolute HTTPS.
- **Clear Presence:** removes the current activity without disabling the service. The next real context change publishes again.

Settings live in the current user's `EditorPrefs`. The project path is normalized and SHA-256 hashed for the key prefix; the raw path is not stored in a key or value.

## State model

The formatter uses this strict priority:

1. Player build.
2. Compiling scripts.
3. Idle.
4. Play Mode.
5. Active tool.
6. Prefab Mode.
7. Scene editing.

Context changes are debounced for one second. Only the latest snapshot is formatted, and a payload equal to the previous payload is not sent again. Two transitions skip the debounce: the start of a player build, because the build blocks the editor loop, and entering or leaving Idle.

| Context | Visible state | Hidden-name state |
| --- | --- | --- |
| Player build | `Building for {platform}` | `Building for {platform}` |
| Compiling | `Compiling scripts` | `Compiling scripts` |
| Idle | `Idle` | `Idle` |
| Play Mode | `Testing scene {scene}` | `Testing a scene` |
| Active tool | See [Active tool](#active-tool) | Same |
| Prefab Mode | `Editing prefab {prefab}` | `Editing a prefab` |
| Scene editing | `Editing scene {scene}` | `Editing a scene` |

For build targets without a platform label, the build state is `Building a player`.

### Player builds

The package implements `IPreprocessBuildWithReport` to publish the build state immediately, and `IPostprocessBuildWithReport` to end it. Failed or cancelled builds do not run post-process callbacks, so the tracker also ends the build state on the first editor update after `BuildPipeline.isBuildingPlayer` becomes false.

### Idle

Idle is based on focus: it starts when Unity is not the focused application (`InternalEditorUtility.isApplicationActive`) for the configured number of minutes, and ends as soon as Unity regains focus. Idle never replaces a player build or compilation. Entering or leaving Idle does not republish after **Clear Presence**.

### Active tool

The focused editor window is checked twice per second:

| Focused window | State |
| --- | --- |
| Animator | `Editing an Animator` |
| Animation | `Animating` |
| Timeline | `Editing a Timeline` |
| Shader Graph | `Editing a Shader Graph` |
| VFX Graph | `Editing a VFX Graph` |
| Tile Palette | `Painting tilemaps` |
| Scene view with a Terrain selected | `Editing terrain` |
| Profiler | `Profiling` |
| Sprite Editor | `Editing sprites` |
| UI Builder | `Designing UI` |

Focusing the Scene view (without a Terrain selected) or the Game view clears the tool. Other windows, such as the Inspector, Hierarchy, Project, or Console, keep the last tool so presence does not flicker. Windows are matched by type name, so Timeline, Shader Graph, and VFX Graph are optional dependencies. The active tool is not shown in Play Mode.

Strings are trimmed and limited to 128 Unicode text elements without splitting surrogate pairs. Button labels are limited to 32 Unicode text elements.

## Artwork

The large image depends on the Unity version:

- Unity 6 / version major `6000` or newer: `unity6-logo`.
- Unity 2021 through 2023: `unity-logo`.
- Unity 2019.4 and 2020: `unity-logo-old`.

The large-image tooltip is the full Unity version.

When **Show build target icon** is enabled, the small image shows the active build target. During a player build, it shows the target being built. The tooltip is `Build target: {platform}`.

| Build target | Small image |
| --- | --- |
| Windows (32/64-bit) | `windows-logo` |
| Linux | `linux-logo` |
| Android | `android-logo` |
| iOS | `ios-logo` |
| WebGL | `webgl-logo` |
| macOS and other targets | None |

The Discord application asset `game-icon` is never used.

## Connection lifecycle

The managed RPC adapter connects to Discord's local IPC endpoint with Application ID `1346887919675113567`. It does not authenticate a Discord account.

When disconnected, the controller retains the latest desired payload and retries after 5, 15, 30, then 60 seconds. Later retries remain capped at 60 seconds. A successful connection resets the schedule and republishes the latest desired payload.

Disabling the setting, reloading assemblies, or quitting Unity performs a best-effort clear followed by deterministic disposal.

## Compatibility and package boundaries

All package C# code and the vendored DLL are constrained to the Editor platform by assembly and plugin import metadata. Nothing is intended for a player build.

Prefab Stage references are isolated behind this compile-time boundary:

- Unity 2021.2+: `UnityEditor.SceneManagement`.
- Unity 2019.4 through 2021.1: `UnityEditor.Experimental.SceneManagement`.

The implementation stays within Unity's C# 7.3 language surface.

## Troubleshooting

- Confirm the desktop Discord client is running and Activity Privacy is enabled.
- An Invisible Discord status can hide activity from other users.
- Flatpak or other sandboxed Linux Discord clients may block access to the IPC socket.
- Correct invalid button fields shown inline in Preferences.
- Set logging to `Errors` for actionable failures or `Verbose` while diagnosing connection changes.

## Security and privacy

The Application ID is public metadata. The package contains no bot token, client secret, OAuth token, private key, telemetry, analytics, or crash reporter. Context selected by the user travels only to the local Discord client through Discord RPC.
