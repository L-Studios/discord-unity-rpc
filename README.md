# L.Studios Discord Unity RPC

Privacy-aware Discord Rich Presence for the Unity Editor. It can show the project, active scene, Prefab Mode, Play Mode, and script compilation status while you work.

The package is Editor-only: it does not add Discord code or `DiscordRPC.dll` to player builds. It uses the public Discord Application ID for the L.Studios `Unity` application and communicates only with the local Discord desktop client.

## Install

1. In Unity, open **Window > Package Manager**.
2. Select **+ > Add package from git URL...**.
3. Enter:

   ```text
   https://github.com/L-Studios/discord-unity-rpc.git
   ```

For a reproducible install, append a release tag such as `#v1.0.1`.

## Enable it

Open **Window > Discord Unity RPC** and enable **Rich Presence**. It is disabled by default for every user and every project. The same settings are also available in **Edit > Preferences > L.Studios > Discord Unity RPC**.

You can independently hide the project, scene, or prefab name; hide elapsed session time; configure logging; and add up to two buttons with absolute HTTPS URLs. These choices are stored in `EditorPrefs` under a one-way hash of the project path, never in the project repository.

## Presence examples

| Editor context | Details | State |
| --- | --- | --- |
| Compiling | `Working on MyProject` | `Compiling scripts` |
| Play Mode | `Working on MyProject` | `Testing scene Main` |
| Prefab Mode | `Working on MyProject` | `Editing prefab Player` |
| Scene editing | `Working on MyProject` | `Editing scene Main` |

Hidden names are replaced with neutral text such as `Working in Unity` and `Editing a scene`. State priority is compilation, Play Mode, Prefab Mode, then scene editing.

## Compatibility

| Unity version | Support | Discord artwork |
| --- | --- | --- |
| 2019.4 LTS–2020.x | Supported | `unity-logo-old` |
| 2021.x–2023.x | Supported | `unity-logo` |
| Unity 6 (`6000.x`) | Supported | `unity6-logo` |

Windows, macOS, and Linux Editors are supported. Player builds, mobile Editors, WebGL, OAuth, bots, voice, lobbies, and in-game Rich Presence are outside the scope of version 1.0.

## Troubleshooting

- **No presence:** launch the Discord desktop client, enable Activity Privacy in Discord, then cause a real editor-context change or toggle the package off and on.
- **Discord shows you as invisible:** Rich Presence may not be visible to other people while your Discord status is Invisible.
- **Linux/Flatpak:** sandboxed Discord packages may not expose their IPC socket to Unity. Use compatible host permissions or a non-sandboxed Discord installation.
- **Invalid button warning:** both a label and an absolute `https://` URL are required.
- **Stale presence:** use **Clear Presence** in **Window > Discord Unity RPC** or in preferences. The next editor-context change publishes again.
- **Package installed but no menu or preferences appear:** update to `v1.0.1` or newer. Version 1.0.0 shipped without `.meta` files, so Unity ignored the package contents.

The connection retries after 5, 15, 30, and then 60 seconds while Discord is unavailable. Repeated failures are rate-limited unless the context changes.

## Data handling

No analytics, telemetry, crash reporting, account login, web API, or background HTTP service is included. After explicit opt-in, selected context is sent to the Discord client running on the same computer. No bot token, client secret, OAuth credential, or Discord public key is required or stored.

## Development

Unity ignores files without a `.meta` in git-installed packages, so every new importable file or folder must ship with a committed `.meta` that has a unique GUID. Run the metadata and safety validator from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "Tools~/validate-package.ps1"
```

Contributions should retain Unity 2019.4/C# 7.3 compatibility, keep all implementation in the Editor assembly, add EditMode coverage for behavior changes, and preserve the opt-in privacy defaults.

## License and attribution

L.Studios Discord Unity RPC is available under the [MIT License](LICENSE.md). The vendored [Lachee Discord RPC C#](https://github.com/Lachee/discord-rpc-csharp) binary is also MIT-licensed; exact provenance and hashes are recorded in [Third Party Notices.md](Third%20Party%20Notices.md) and `Editor/ThirdParty/DiscordRPC/VERSION`.

Discord is a trademark of Discord Inc. Unity is a trademark of Unity Technologies. This project is not affiliated with or endorsed by either company.
