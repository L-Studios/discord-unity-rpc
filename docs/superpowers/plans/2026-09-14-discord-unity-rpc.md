# Discord Unity RPC Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build and publish an installable Unity Editor package that reports project, scene, Prefab Mode, Play Mode, and compilation context to Discord Rich Presence from Unity 2019.4 LTS onward.

**Architecture:** An Editor-only controller receives event-driven snapshots from a Unity context tracker, formats them through pure C# domain objects, and sends changed payloads through a narrow adapter around a pinned managed Discord RPC binary. Per-user/per-project `EditorPrefs` settings provide explicit opt-in and privacy controls; all transport and time dependencies are injectable for EditMode tests.

**Tech Stack:** Unity 2019.4+ Editor APIs, C# 7.3, Unity Test Framework, Lachee Discord RPC C# v1.6.2 (MIT), Newtonsoft.Json 13 through `com.unity.nuget.newtonsoft-json`, UPM package layout, GitHub Actions metadata checks.

**Spec:** `docs/superpowers/specs/2026-09-14-discord-unity-rpc-design.md`

## Global Constraints

- Package identifier is exactly `com.lstudios.discord-unity-rpc`.
- Unity version floor is exactly `2019.4`.
- Supported editors are Windows, macOS, and Linux; player builds receive no package assembly.
- Discord Application ID is exactly `1346887919675113567` and is public metadata.
- Never include a bot token, client secret, OAuth token, public key, telemetry, or crash reporting.
- Rich Presence is disabled until the user explicitly enables it for the current project.
- Asset mapping is Unity 6 → `unity6-logo`, Unity 2021-2023 → `unity-logo`, Unity 2019-2020 → `unity-logo-old`.
- Never use the `game-icon` asset or a small image in version 1.0.
- Optional buttons accept only absolute HTTPS URLs.
- Code must compile with Unity's C# 7.3 support; do not use records, `init`, file-scoped namespaces, nullable-reference syntax, or newer language features.
- Third-party MIT copyright and license notices must ship with the package.

---

## File Map

### Package and repository metadata

- `package.json`: UPM identity, Unity floor, dependency, links, and sample declaration.
- `LICENSE.md`: L.Studios MIT license.
- `Third Party Notices.md`: Lachee attribution, pinned release/tag, license text location, and binary provenance.
- `CHANGELOG.md`: version `1.0.0` release notes.
- `.gitignore`: Unity and IDE outputs without excluding package source.
- `.gitattributes`: text normalization and binary treatment for DLLs.
- `.github/workflows/validate.yml`: JSON, secret-pattern, and repository-layout validation.
- `Tools~/validate-package.ps1`: deterministic package metadata validator used locally and in CI.

### Editor domain and orchestration

- `Editor/AssemblyInfo.cs`: exposes internals to the EditMode test assembly.
- `Editor/LStudios.DiscordUnityRpc.Editor.asmdef`: Editor-only assembly, explicit precompiled DLL reference.
- `Editor/DiscordApplication.cs`: application ID and asset-key constants.
- `Editor/Context/EditorActivityKind.cs`: activity-priority enum.
- `Editor/Context/EditorContextSnapshot.cs`: immutable Unity context value.
- `Editor/Context/IEditorContextSource.cs`: context-source contract.
- `Editor/Context/UnityEditorContextTracker.cs`: Unity event subscriptions and snapshot capture.
- `Editor/Context/PrefabStageCompatibility.cs`: compile-directive boundary for 2019/2020 versus newer Prefab Stage namespaces.
- `Editor/Presence/PresenceButton.cs`: validated button value.
- `Editor/Presence/PresencePayload.cs`: transport-neutral presence value and equality.
- `Editor/Presence/UnityVersionAssetSelector.cs`: version-to-key mapping.
- `Editor/Presence/DiscordText.cs`: Unicode-safe normalization and truncation.
- `Editor/Presence/DiscordPresenceFormatter.cs`: snapshot/options-to-payload mapping.
- `Editor/Transport/IDiscordRpcTransport.cs`: testable transport contract.
- `Editor/Transport/DiscordRpcTransport.cs`: adapter for the vendored RPC DLL.
- `Editor/Orchestration/IEditorClock.cs`: wall-clock and editor-time abstraction.
- `Editor/Orchestration/UnityEditorClock.cs`: Unity/System clock implementation.
- `Editor/Orchestration/DiscordPresenceController.cs`: debounce, duplicate suppression, reconnect, and republish behavior.
- `Editor/Orchestration/DiscordPresenceBootstrap.cs`: `[InitializeOnLoad]` lifecycle composition and cleanup.

### Preferences

- `Editor/Preferences/DiscordUnityRpcOptions.cs`: mutable options value with privacy-safe defaults.
- `Editor/Preferences/IDiscordUnityRpcPreferences.cs`: read/write contract and change event.
- `Editor/Preferences/EditorPrefsDiscordUnityRpcPreferences.cs`: per-user/per-project persistence.
- `Editor/Preferences/DiscordUnityRpcSettingsProvider.cs`: IMGUI Preferences page.

### Vendored dependency

- `Editor/ThirdParty/DiscordRPC/DiscordRPC.dll`: official v1.6.2 release binary from `Lachee/discord-rpc-csharp`.
- `Editor/ThirdParty/DiscordRPC/DiscordRPC.dll.meta`: Editor-only importer configuration.
- `Editor/ThirdParty/DiscordRPC/LICENSE`: upstream MIT license copied verbatim.
- `Editor/ThirdParty/DiscordRPC/VERSION`: exact tag, commit, source URL, release-asset URL, and SHA-256.

### Tests and documentation

- `Tests/Editor/LStudios.DiscordUnityRpc.Editor.Tests.asmdef`: EditMode tests.
- `Tests/Editor/DiscordPresenceFormatterTests.cs`: state, privacy, asset, Unicode, button, and timestamp tests.
- `Tests/Editor/EditorPrefsDiscordUnityRpcPreferencesTests.cs`: project isolation and defaults.
- `Tests/Editor/DiscordPresenceControllerTests.cs`: debounce, duplicate suppression, retry, republish, and cleanup tests.
- `Tests/Editor/PrefabStageCompatibilityTests.cs`: namespace boundary smoke test for the current editor.
- `Samples~/Basic Usage/README.md`: opt-in and behavior walkthrough.
- `Documentation~/index.md`: installation, privacy, compatibility, troubleshooting, limitations, and API behavior.
- `README.md`: public repository landing page.

---

### Task 1: UPM Scaffold and Pinned RPC Dependency

**Files:**
- Create: `package.json`
- Create: `LICENSE.md`
- Create: `Third Party Notices.md`
- Create: `CHANGELOG.md`
- Create: `.gitignore`
- Create: `.gitattributes`
- Create: `Tools~/validate-package.ps1`
- Create: `Editor/LStudios.DiscordUnityRpc.Editor.asmdef`
- Create: `Editor/AssemblyInfo.cs`
- Create: `Editor/ThirdParty/DiscordRPC/DiscordRPC.dll`
- Create: `Editor/ThirdParty/DiscordRPC/DiscordRPC.dll.meta`
- Create: `Editor/ThirdParty/DiscordRPC/LICENSE`
- Create: `Editor/ThirdParty/DiscordRPC/VERSION`

**Interfaces:**
- Consumes: Lachee release tag `v1.6.2` and its published MIT license.
- Produces: UPM package root; Editor-only assembly `LStudios.DiscordUnityRpc.Editor`; precompiled reference `DiscordRPC.dll`.

- [ ] **Step 1: Write the package validator before the manifest**

Create `Tools~/validate-package.ps1` with assertions that load `package.json`, require the exact name/version/Unity floor, require `com.unity.nuget.newtonsoft-json` version `3.0.3`, verify required notice files, verify that the DLL `.meta` contains `Editor: 1` and no standalone platform, and reject case-insensitive secret patterns `client_secret`, `oauth_token`, `bot_token`, and `BEGIN PRIVATE KEY` in executable source and package configuration.

The script exits `1` with every collected failure and prints `Package validation passed.` on success.

- [ ] **Step 2: Run the validator to verify it fails**

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "Tools~/validate-package.ps1"
```

Expected: FAIL because `package.json` and required package files do not exist.

- [ ] **Step 3: Create the UPM manifest and repository metadata**

Create `package.json` with these required values:

```json
{
  "name": "com.lstudios.discord-unity-rpc",
  "version": "1.0.0",
  "displayName": "L.Studios Discord Unity RPC",
  "description": "Discord Rich Presence for the Unity Editor.",
  "unity": "2019.4",
  "license": "MIT",
  "documentationUrl": "https://github.com/L-Studios/discord-unity-rpc/blob/main/Documentation~/index.md",
  "changelogUrl": "https://github.com/L-Studios/discord-unity-rpc/blob/main/CHANGELOG.md",
  "licensesUrl": "https://github.com/L-Studios/discord-unity-rpc/blob/main/LICENSE.md",
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "3.0.3"
  },
  "samples": [
    {
      "displayName": "Basic Usage",
      "description": "Enable and configure Discord Rich Presence for this project.",
      "path": "Samples~/Basic Usage"
    }
  ],
  "keywords": ["discord", "rich-presence", "editor", "unity"],
  "author": {
    "name": "L.Studios",
    "url": "https://github.com/L-Studios"
  }
}
```

Add the MIT license, initial changelog, Unity/IDE ignores, LF normalization, and `binary` treatment for `*.dll`.

- [ ] **Step 4: Fetch and verify the official RPC artifact**

Use GitHub's release API for `Lachee/discord-rpc-csharp` tag `v1.6.2`; select the .NET Framework 4.5 `DiscordRPC.dll` artifact, download it directly from the release, compute SHA-256, and record tag, commit, asset URL, source URL, and hash in `Editor/ThirdParty/DiscordRPC/VERSION`. Copy the upstream MIT license verbatim and include its copyright in `Third Party Notices.md`.

Do not copy the DLL from the installed CustomRP directory; use the public upstream artifact so provenance is reproducible.

- [ ] **Step 5: Restrict the dependency and package assembly to the Editor**

Create `DiscordRPC.dll.meta` with `Any: 0`, `Editor: 1`, and all player platforms disabled. Create the Editor asmdef with:

```json
{
  "name": "LStudios.DiscordUnityRpc.Editor",
  "rootNamespace": "LStudios.DiscordUnityRpc",
  "references": ["Unity.Newtonsoft.Json"],
  "includePlatforms": ["Editor"],
  "overrideReferences": true,
  "precompiledReferences": ["DiscordRPC.dll"],
  "autoReferenced": true
}
```

Add `[assembly: InternalsVisibleTo("LStudios.DiscordUnityRpc.Editor.Tests")]` to `Editor/AssemblyInfo.cs`.

- [ ] **Step 6: Run package validation**

Run the validation script again.

Expected: `Package validation passed.`

- [ ] **Step 7: Commit the scaffold**

```bash
git add package.json LICENSE.md "Third Party Notices.md" CHANGELOG.md .gitignore .gitattributes "Tools~" Editor
git commit -m "chore: scaffold Unity editor package"
```

---

### Task 2: Presence Domain and Formatter

**Files:**
- Create: `Editor/DiscordApplication.cs`
- Create: `Editor/Context/EditorActivityKind.cs`
- Create: `Editor/Context/EditorContextSnapshot.cs`
- Create: `Editor/Presence/PresenceButton.cs`
- Create: `Editor/Presence/PresencePayload.cs`
- Create: `Editor/Presence/UnityVersionAssetSelector.cs`
- Create: `Editor/Presence/DiscordText.cs`
- Create: `Editor/Presence/DiscordPresenceFormatter.cs`
- Create: `Editor/Preferences/DiscordUnityRpcOptions.cs`
- Create: `Tests/Editor/LStudios.DiscordUnityRpc.Editor.Tests.asmdef`
- Create: `Tests/Editor/DiscordPresenceFormatterTests.cs`

**Interfaces:**
- Consumes: no Unity Editor state; only immutable input values.
- Produces: `PresencePayload DiscordPresenceFormatter.Format(EditorContextSnapshot snapshot, DiscordUnityRpcOptions options, long sessionStartedAtUnixSeconds)` and `string UnityVersionAssetSelector.Select(string unityVersion)`.

- [ ] **Step 1: Write failing formatter tests**

Tests must construct `EditorContextSnapshot` directly and assert these exact cases:

```csharp
[TestCase("6000.0.20f1", "unity6-logo")]
[TestCase("2022.3.62f1", "unity-logo")]
[TestCase("2021.1.0f1", "unity-logo")]
[TestCase("2020.3.48f1", "unity-logo-old")]
[TestCase("2019.4.40f1", "unity-logo-old")]
public void SelectsExpectedLargeImage(string version, string expected) { }

[Test]
public void CompilingTakesPriorityAndHonorsHiddenProject() { }

[Test]
public void PrefabModeUsesPrefabNameWhenVisible() { }

[Test]
public void InvalidOrNonHttpsButtonsAreOmitted() { }

[Test]
public void TruncationDoesNotSplitSurrogatePair() { }
```

The expected hidden-project details string is `Working in Unity`; expected visible string is `Working on {project}`. Scene, prefab, and Play Mode states use the strings in the approved design.

- [ ] **Step 2: Run tests to verify they fail**

Run the EditMode tests in the package test project or Unity Test Runner.

Expected: compilation failure because domain types do not exist.

- [ ] **Step 3: Implement immutable domain values**

Define:

```csharp
internal enum EditorActivityKind { EditingScene, EditingPrefab, Playing, Compiling }

internal sealed class EditorContextSnapshot
{
    public string ProjectName { get; }
    public string SceneName { get; }
    public string PrefabName { get; }
    public string UnityVersion { get; }
    public EditorActivityKind ActivityKind { get; }
}
```

`PresenceButton` and `PresencePayload` implement value equality and stable hash codes without mutable collections. `PresencePayload` includes `Details`, `State`, `LargeImageKey`, `LargeImageText`, nullable `StartTimestamp`, and a copied array of zero to two buttons.

- [ ] **Step 4: Implement formatting and validation**

Add constants:

```csharp
internal const string ApplicationId = "1346887919675113567";
internal const string Unity6Logo = "unity6-logo";
internal const string ModernUnityLogo = "unity-logo";
internal const string LegacyUnityLogo = "unity-logo-old";
```

Parse the leading major version as an integer; major `>= 6000` selects Unity 6, major `>= 2021` selects modern, otherwise legacy. `DiscordText.Normalize(value, maxTextElements)` trims whitespace and enumerates Unicode text elements through `StringInfo` so truncation cannot split a surrogate pair.

Validate button labels as non-empty after normalization and URLs with `Uri.TryCreate`, `IsAbsoluteUri`, and scheme equal to `Uri.UriSchemeHttps`. Keep at most two buttons.

- [ ] **Step 5: Run formatter tests**

Expected: all `DiscordPresenceFormatterTests` pass.

- [ ] **Step 6: Commit the domain layer**

```bash
git add Editor/DiscordApplication.cs Editor/Context Editor/Presence Editor/Preferences/DiscordUnityRpcOptions.cs Tests/Editor
git commit -m "feat: format privacy-aware editor presence"
```

---

### Task 3: Per-user and Per-project Preferences

**Files:**
- Create: `Editor/Preferences/IDiscordUnityRpcPreferences.cs`
- Create: `Editor/Preferences/EditorPrefsDiscordUnityRpcPreferences.cs`
- Create: `Editor/Preferences/DiscordUnityRpcSettingsProvider.cs`
- Create: `Tests/Editor/EditorPrefsDiscordUnityRpcPreferencesTests.cs`

**Interfaces:**
- Consumes: `DiscordUnityRpcOptions` from Task 2.
- Produces: `DiscordUnityRpcOptions Current`, `event Action Changed`, and `void Save(DiscordUnityRpcOptions options)` through `IDiscordUnityRpcPreferences`.

- [ ] **Step 1: Write failing preference-isolation tests**

Use an injected `IKeyValueStore` fake instead of real global `EditorPrefs`. Assert:

```csharp
[Test]
public void NewProjectDefaultsToDisabledAndPrivacyFieldsSelected() { }

[Test]
public void EnablingOneProjectDoesNotEnableAnotherProject() { }

[Test]
public void SavingRaisesChangedExactlyOnce() { }
```

Use two normalized fake project paths and require different SHA-256-derived key prefixes.

- [ ] **Step 2: Run tests to verify they fail**

Expected: compilation failure because preference contracts do not exist.

- [ ] **Step 3: Implement storage contracts and EditorPrefs adapter**

Define an internal key-value abstraction for bool/int/string operations. Build the prefix as `com.lstudios.discord-unity-rpc.` plus lowercase SHA-256 hex of the normalized project root. Never store the raw project path in an EditorPrefs key or value.

Defaults:

```csharp
Enabled = false;
ShowProjectName = true;
ShowSceneName = true;
ShowPrefabName = true;
ShowElapsedTime = true;
LogLevel = DiscordUnityRpcLogLevel.Errors;
```

- [ ] **Step 4: Implement the IMGUI SettingsProvider**

Register `[SettingsProvider]` at path `Preferences/L.Studios/Discord Unity RPC`. Show the opt-in disclosure above the enable toggle, privacy toggles, timestamp/log options, two optional label/URL pairs, connection status, and a `Clear Presence` button. Disable subordinate controls while the feature is off. Validate button URLs inline without transmitting them.

- [ ] **Step 5: Run preference tests**

Expected: all preference tests pass.

- [ ] **Step 6: Commit preferences**

```bash
git add Editor/Preferences Tests/Editor/EditorPrefsDiscordUnityRpcPreferencesTests.cs
git commit -m "feat: add private per-project editor preferences"
```

---

### Task 4: Unity Editor Context Tracking

**Files:**
- Create: `Editor/Context/IEditorContextSource.cs`
- Create: `Editor/Context/PrefabStageCompatibility.cs`
- Create: `Editor/Context/UnityEditorContextTracker.cs`
- Create: `Tests/Editor/PrefabStageCompatibilityTests.cs`

**Interfaces:**
- Consumes: `EditorContextSnapshot` and `EditorActivityKind` from Task 2.
- Produces: `EditorContextSnapshot Capture()` and `event Action ContextChanged` through `IEditorContextSource`; disposable Unity event subscriptions.

- [ ] **Step 1: Write failing compatibility and priority smoke tests**

Assert that `PrefabStageCompatibility.GetCurrentPrefabName()` returns either an empty string outside Prefab Mode or a file name without extension inside a test Prefab Stage. Add a pure `ResolveActivityKind(bool compiling, bool playing, bool prefabOpen)` test table that proves `Compiling > Playing > EditingPrefab > EditingScene`.

- [ ] **Step 2: Run tests to verify they fail**

Expected: compilation failure because the context source and compatibility facade do not exist.

- [ ] **Step 3: Implement the Prefab Stage compatibility boundary**

Use exactly this namespace split:

```csharp
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
```

Keep every reference to `PrefabStage` and `PrefabStageUtility` inside `PrefabStageCompatibility.cs` so no other file needs version directives.

- [ ] **Step 4: Implement event-driven context tracking**

Capture the project name from the directory above `Application.dataPath`, active scene from `SceneManager.GetActiveScene().name`, prefab from the compatibility facade, compilation from `EditorApplication.isCompiling`, play state from `EditorApplication.isPlayingOrWillChangePlaymode`, and version from `Application.unityVersion`.

Subscribe to `EditorApplication.playModeStateChanged`, `EditorApplication.projectChanged`, `EditorApplication.hierarchyChanged`, `EditorSceneManager.activeSceneChangedInEditMode`, and `CompilationPipeline.compilationStarted/compilationFinished`. Prefab Stage events are subscribed inside the compatibility facade. Each callback raises `ContextChanged`; it does not contact Discord directly.

- [ ] **Step 5: Run context tests**

Expected: compatibility and priority tests pass in the current Unity editor.

- [ ] **Step 6: Commit context tracking**

```bash
git add Editor/Context Tests/Editor/PrefabStageCompatibilityTests.cs
git commit -m "feat: track Unity editor context"
```

---

### Task 5: Discord RPC Transport Adapter

**Files:**
- Create: `Editor/Transport/IDiscordRpcTransport.cs`
- Create: `Editor/Transport/DiscordRpcTransport.cs`

**Interfaces:**
- Consumes: `PresencePayload` and `PresenceButton` from Task 2; `DiscordRPC.DiscordRpcClient` from Task 1.
- Produces: `Initialize(string applicationId)`, `SetPresence(PresencePayload payload)`, `ClearPresence()`, `Dispose()`, `bool IsConnected`, `event Action Connected`, and `event Action Disconnected`.

- [ ] **Step 1: Add adapter contract tests to formatter test assembly**

Create a mapping test that calls an internal static `DiscordRpcTransport.CreateRichPresence(PresencePayload)` and asserts exact `DiscordRPC.RichPresence` fields, timestamp omission, asset keys/text, and zero-to-two mapped buttons.

- [ ] **Step 2: Run the mapping test to verify it fails**

Expected: compilation failure because transport types do not exist.

- [ ] **Step 3: Implement the minimal transport**

Initialize one `DiscordRpcClient` with the supplied application ID and `AutoEvents = false`; subscribe to ready/connection events before calling `Initialize()`. Convert the transport-neutral payload to third-party `RichPresence`. Set `Timestamps.StartUnixMilliseconds` from seconds only when enabled. Set no small image, party, secrets, or telemetry.

Create timestamps with `new Timestamps { Start = UnixEpochUtc.AddSeconds(payload.StartTimestamp.Value) }`, where `UnixEpochUtc` is a `static readonly DateTime` constructed with `DateTimeKind.Utc`. `ClearPresence` calls the library clear method only when initialized. `Dispose` is idempotent and unsubscribes events before disposing. Never log third-party payload JSON.

- [ ] **Step 4: Run adapter mapping tests**

Expected: all mapping tests pass without a running Discord client because they exercise only conversion.

- [ ] **Step 5: Commit the adapter**

```bash
git add Editor/Transport Tests/Editor/DiscordPresenceFormatterTests.cs
git commit -m "feat: adapt editor presence to Discord RPC"
```

---

### Task 6: Debounced Controller and Reconnection

**Files:**
- Create: `Editor/Orchestration/IEditorClock.cs`
- Create: `Editor/Orchestration/UnityEditorClock.cs`
- Create: `Editor/Orchestration/DiscordPresenceController.cs`
- Create: `Tests/Editor/DiscordPresenceControllerTests.cs`

**Interfaces:**
- Consumes: `IEditorContextSource`, `IDiscordUnityRpcPreferences`, `DiscordPresenceFormatter`, `IDiscordRpcTransport`, and `IEditorClock`.
- Produces: `Start()`, `Tick()`, `ClearNow()`, and idempotent `Dispose()`; no global singleton.

- [ ] **Step 1: Write failing orchestration tests with fakes**

Create fake context source, preferences, transport, and clock. Assert:

```csharp
[Test] public void DisabledStartDoesNotInitializeTransport() { }
[Test] public void EnabledChangePublishesAfterOneSecond() { }
[Test] public void RapidChangesPublishOnlyLatestPayload() { }
[Test] public void EqualPayloadIsNotPublishedTwice() { }
[Test] public void ConnectedEventRepublishesLatestPayload() { }
[Test] public void DisconnectRetriesAtFiveFifteenThirtyThenSixtySeconds() { }
[Test] public void DisableClearsAndDisposesTransport() { }
[Test] public void DisposeIsIdempotent() { }
```

- [ ] **Step 2: Run controller tests to verify they fail**

Expected: compilation failure because controller and clock types do not exist.

- [ ] **Step 3: Implement clock and debounce behavior**

`IEditorClock` exposes `double TimeSinceStartup` and `long UnixSeconds`. `QueueContext` stores only the latest snapshot and sets `publishAt = TimeSinceStartup + 1.0`. `Tick` returns immediately when disabled, uninitialized, or before `publishAt`; otherwise it formats once and sends only when unequal to the last sent payload.

- [ ] **Step 4: Implement bounded reconnection**

On disconnect, schedule attempts at `5`, `15`, `30`, then `60` seconds, with all later attempts capped at `60`. A ready/connected event resets the attempt counter and republishes the latest desired payload. Log the first error and suppress repeats until state changes.

- [ ] **Step 5: Run controller tests**

Expected: all controller tests pass.

- [ ] **Step 6: Commit orchestration**

```bash
git add Editor/Orchestration Tests/Editor/DiscordPresenceControllerTests.cs
git commit -m "feat: coordinate debounced Discord presence"
```

---

### Task 7: Editor Bootstrap and Lifecycle Cleanup

**Files:**
- Create: `Editor/Orchestration/DiscordPresenceBootstrap.cs`
- Modify: `Editor/Preferences/DiscordUnityRpcSettingsProvider.cs`

**Interfaces:**
- Consumes: concrete implementations from Tasks 3-6.
- Produces: automatic Editor initialization, update pumping, clear command, visible connection status, assembly-reload cleanup, and editor-quit cleanup.

- [ ] **Step 1: Add failing lifecycle tests**

Extract a small internal `DiscordPresenceLifetime` that accepts an `Action` clear/dispose callback. Test that repeated reload/quit notifications invoke cleanup exactly once and that reinitialization creates a fresh controller.

- [ ] **Step 2: Run lifecycle tests to verify they fail**

Expected: compilation failure because bootstrap/lifetime types do not exist.

- [ ] **Step 3: Implement automatic composition**

Use `[InitializeOnLoad]` and `EditorApplication.delayCall` to construct preferences, tracker, formatter, transport, clock, and controller after the domain is ready. Register only a lightweight `EditorApplication.update` callback that calls `controller.Tick()`.

Subscribe cleanup to `AssemblyReloadEvents.beforeAssemblyReload` and `EditorApplication.quitting`. The preferences page `Clear Presence` button calls the bootstrap's `ClearNow()` without disabling the service; the next real context change republishes.

- [ ] **Step 4: Run all EditMode tests**

Expected: all tests pass and Unity Console has no compile errors.

- [ ] **Step 5: Manually verify privacy and state transitions**

In Unity:

1. Confirm no connection before opt-in.
2. Enable the package and verify scene editing presence.
3. Enter Prefab Mode and verify prefab state.
4. Enter Play Mode and verify testing state.
5. Trigger compilation and verify compiling state wins.
6. Hide each name independently and verify neutral text.
7. Disable the package and verify Discord presence clears.

- [ ] **Step 6: Commit lifecycle integration**

```bash
git add Editor Tests/Editor
git commit -m "feat: integrate Discord RPC with Unity lifecycle"
```

---

### Task 8: Documentation, Sample, CI, and Release Validation

**Files:**
- Create: `README.md`
- Create: `Documentation~/index.md`
- Create: `Samples~/Basic Usage/README.md`
- Create: `.github/workflows/validate.yml`
- Modify: `CHANGELOG.md`

**Interfaces:**
- Consumes: completed package behavior.
- Produces: installation/support documentation, reproducible metadata validation, public-release checklist, and Git URL instructions.

- [ ] **Step 1: Write documentation against the implemented UI**

README sections must include: a text overview with no fabricated screenshot, Git URL installation, compatibility matrix, first-run privacy behavior, exact status examples, preference descriptions, troubleshooting for Discord closed/invisible/Flatpak, data-handling statement, limitations, MIT attribution, and contribution instructions.

Use this exact installation URL:

```text
https://github.com/L-Studios/discord-unity-rpc.git
```

- [ ] **Step 2: Add the sample and detailed documentation**

The sample README walks through enabling the package, opening a scene, entering Prefab Mode, testing Play Mode, hiding sensitive names, and clearing/disabling presence. `Documentation~/index.md` documents state priority, one-second debounce, retry schedule, asset mapping, button validation, and log levels.

- [ ] **Step 3: Add CI metadata validation**

Create a Windows GitHub Actions job that checks out the repository and runs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "Tools~/validate-package.ps1"
```

Add a second step that parses every `.asmdef` and `package.json` as JSON. Do not require Unity license secrets for the public metadata workflow.

- [ ] **Step 4: Run complete local validation**

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "Tools~/validate-package.ps1"
git diff --check
```

Run all EditMode tests in each installed target editor available locally. At minimum, the current editor must compile with zero errors; document any unavailable Unity-version matrix entries as unverified rather than claiming success.

- [ ] **Step 5: Inspect package/build boundaries**

Build or dry-run a desktop player from the test project and confirm `LStudios.DiscordUnityRpc.Editor` and `DiscordRPC.dll` are absent from the player output. Inspect the package manager view and confirm display name, version, sample, license, and documentation links.

- [ ] **Step 6: Commit release documentation**

```bash
git add README.md Documentation~ Samples~ .github CHANGELOG.md
git commit -m "docs: prepare Discord Unity RPC 1.0.0"
```

- [ ] **Step 7: Final repository checks**

Run `git status --short`, inspect the full branch diff, scan tracked files for high-entropy secrets, verify `Editor/ThirdParty/DiscordRPC/VERSION` against the committed DLL hash, and confirm CodeGraph reports the expected source files and symbols.

Only after all checks pass: create public repository `L-Studios/discord-unity-rpc`, push `main`, create annotated tag `v1.0.0`, push the tag, and verify the GitHub repository is public and the installation URL resolves.
