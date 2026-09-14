# Discord Unity RPC Design

## Summary

`com.lstudios.discord-unity-rpc` is a Unity Package Manager package that publishes the developer's current Unity Editor context as Discord Rich Presence. It supports Unity 2019.4 LTS and newer, including Unity 6, and adds no runtime code or dependency to player builds.

The first public release targets desktop Unity Editors on Windows, macOS, and Linux. It uses Discord's local IPC transport through a vendored, MIT-licensed managed RPC implementation derived from Lachee's `discord-rpc-csharp`/`discord-rpc-unity` projects. The package uses only the public Discord Application ID `1346887919675113567`; it never requires or stores a bot token, client secret, OAuth token, or public key.

## Goals

- Install from the public Git repository with Unity Package Manager.
- Support Unity 2019.4 LTS through current Unity 6 releases.
- Show the current project and editing context in Discord.
- Detect scene editing, Prefab Mode, Play Mode, and script compilation.
- Select the correct Discord art asset for the running Unity version.
- Protect project information with explicit, per-user opt-in and granular privacy controls.
- Remain silent and unobtrusive when Discord is unavailable.
- Be testable without requiring a running Discord client.

## Non-goals

- Shipping Rich Presence inside player builds.
- Discord account authentication, bots, OAuth, social relationships, voice, lobbies, or messaging.
- Mobile, console, or WebGL support.
- Telemetry or analytics.
- Uploading or modifying Discord Developer Portal assets.
- Using the `game-icon` asset.

## User Experience

The package adds a preferences page at `Edit > Preferences > L.Studios > Discord Unity RPC`. The service is disabled after first installation. The page explains that enabling it publishes selected editor context to Discord and offers these per-user settings:

- Enable Rich Presence.
- Show or hide the project name.
- Show or hide the active scene name.
- Show or hide the Prefab Mode asset name.
- Show or hide the elapsed session timestamp.
- Log level: Off, Errors, or Verbose.
- Up to two optional Rich Presence buttons, each with a label and HTTPS URL.

Preferences are stored per user and per project, and are not committed with the Unity project. Their `EditorPrefs` keys include a locally derived project identifier so enabling the package in one project never enables it automatically in another. Disabling the service immediately clears the current presence and disposes the IPC connection.

## Presence Model

The Discord application name is `Unity`. The default activity is formed as follows:

| Context | Details | State |
| --- | --- | --- |
| Compiling scripts | `Working on {project}` | `Compiling scripts` |
| Play Mode | `Working on {project}` | `Testing scene {scene}` |
| Prefab Mode | `Working on {project}` | `Editing prefab {prefab}` |
| Scene editing | `Working on {project}` | `Editing scene {scene}` |

Hidden values are replaced with neutral text rather than empty or malformed fields. Discord strings are normalized and truncated to protocol limits without splitting Unicode surrogate pairs. Empty optional fields and invalid buttons are omitted.

The context priority is:

1. Script compilation.
2. Play Mode.
3. Prefab Mode.
4. Scene editing.

The start timestamp represents the current Unity Editor session and remains stable across context changes.

## Version-specific Assets

The large image key is selected at compile time:

- Unity 6 (`UNITY_6000_0_OR_NEWER`): `unity6-logo`.
- Unity 2021.1 through Unity 2023: `unity-logo`.
- Unity 2019.4 and Unity 2020: `unity-logo-old`.

The large image text contains the full `Application.unityVersion`. No small image is used in version 1.0, and `game-icon` is always ignored.

## Architecture

### Editor assembly

All L.Studios integration code lives in an Editor-only assembly definition. No L.Studios script is compiled into a player. The vendored RPC implementation is isolated in its own assembly and referenced only by the Editor assembly.

### `UnityEditorContextTracker`

Collects an immutable editor-context snapshot from Unity APIs. It listens to editor lifecycle, play-mode, hierarchy, project, active-scene, Prefab Stage, and compilation changes. Unity 2019/2020 Prefab Stage APIs are isolated behind compile directives because their namespace differs from newer Unity versions.

### `DiscordPresenceFormatter`

A pure C# component that maps a context snapshot plus privacy preferences to an RPC payload. It owns state priority, string normalization, Discord length constraints, version-logo selection, timestamps, and optional button validation.

### `DiscordRpcTransport`

A narrow adapter around the vendored managed RPC client. It exposes initialize, set-presence, clear, dispose, and connection-state events. The rest of the package does not depend directly on third-party RPC types.

### `DiscordPresenceController`

An editor service initialized after the Unity domain loads. It owns the tracker and transport, subscribes to context changes, applies a one-second debounce, and publishes only when the resulting payload differs from the previous one. It clears and disposes the transport before assembly reload, editor shutdown, or user disablement.

### `DiscordUnityRpcPreferences`

Reads and writes namespaced `EditorPrefs` keys. Defaults are privacy-safe: the feature starts disabled, while project, scene, prefab, and timestamp visibility are selected for when the user opts in.

## Data Flow

1. Unity loads the Editor assembly.
2. The controller reads per-user preferences.
3. If disabled, it performs no connection or context publication.
4. If enabled, the tracker emits a context snapshot.
5. The controller waits for one second of stability.
6. The formatter creates and validates the desired presence.
7. If the payload changed, the transport sends it to the local Discord client.
8. Later Unity events repeat steps 4-7.
9. Disable, reload, or shutdown clears the presence and disposes the connection.

No polling-heavy `Update` loop is used. If a lightweight callback pump is required by the vendored library, it performs only the library callback dispatch and is guarded by enabled/initialized state.

## Failure Handling

- Discord not running: retain the latest desired payload and retry with bounded exponential delays, without modal dialogs.
- Invalid Application ID: disable connection attempts for the session and log one actionable error.
- IPC disconnect: transition to disconnected, schedule reconnect, and republish the latest payload after reconnection.
- Invalid or oversized field: sanitize locally; never send a known-invalid payload.
- Editor domain reload or shutdown: best-effort clear followed by deterministic disposal.
- Logging: errors are rate-limited; verbose connection details are opt-in. Logs never contain credentials or private settings beyond context the user chose to publish.

## Package Layout

```text
package.json
README.md
CHANGELOG.md
LICENSE.md
Third Party Notices.md
Editor/
  LStudios.DiscordUnityRpc.Editor.asmdef
  Context/
  Presence/
  Transport/
  Preferences/
ThirdParty/
  DiscordRPC/
Tests/
  Editor/
Samples~/
  Basic Usage/
Documentation~/
```

The package metadata declares Unity `2019.4`, the identifier `com.lstudios.discord-unity-rpc`, semantic version `1.0.0`, an MIT license, documentation links, keywords, and the L.Studios author identity.

## Testing

EditMode tests cover:

- Context priority.
- Project, scene, and prefab privacy combinations.
- Unity 2019/2020, 2021-2023, and Unity 6 asset selection.
- Unicode-safe field limits and empty-field omission.
- Button label and HTTPS URL validation.
- Stable session timestamp behavior.
- Payload equality and duplicate-update suppression.
- Debounce behavior with a controllable clock.
- Reconnection and republishing using a fake transport.
- Disable/reload cleanup.

A manual release checklist verifies installation by Git URL in Unity 2019.4 LTS, Unity 2022.3 LTS, and Unity 6; verifies the three asset keys in Discord; and confirms that no package assembly is present in a desktop player build.

## Repository and Release

The repository will be created publicly as `L-Studios/discord-unity-rpc`, with `main` as the default branch. The package remains at the repository root so users can install:

```text
https://github.com/L-Studios/discord-unity-rpc.git
```

Release `v1.0.0` will be tagged after validation. The README documents installation, privacy behavior, supported versions/platforms, status examples, troubleshooting, API limitations, upstream attribution, and the fact that only the Application ID is used.

## Security and Privacy

- The package contains no token, client secret, OAuth credential, or bot credential.
- The Discord Application ID is public metadata and may be committed.
- No analytics, crash reporting, network API, or background HTTP service is included.
- Rich Presence is sent only to the locally running Discord client after explicit opt-in.
- Users can independently hide project, scene, and prefab names.
- Optional buttons accept only absolute HTTPS URLs.

## Deferred Work

- An optional official Discord Social SDK backend.
- Android support.
- Custom templates and localization.
- Idle detection.
- Custom per-project application IDs and asset mappings.
- Join/spectate secrets or multiplayer integration.
