[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$packageRoot = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure {
    param([string]$Message)
    $failures.Add($Message)
}

function Require-File {
    param([string]$RelativePath)

    $fullPath = Join-Path $packageRoot $RelativePath
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        Add-Failure "Missing required file: $RelativePath"
        return $null
    }

    return $fullPath
}

$manifestPath = Require-File 'package.json'
if ($null -ne $manifestPath) {
    try {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.name -ne 'com.lstudios.discord-unity-rpc') {
            Add-Failure 'package.json name must be com.lstudios.discord-unity-rpc.'
        }
        if ($manifest.version -notmatch '^\d+\.\d+\.\d+$') {
            Add-Failure 'package.json version must be MAJOR.MINOR.PATCH.'
        }
        else {
            $changelogPath = Join-Path $packageRoot 'CHANGELOG.md'
            if ((Test-Path -LiteralPath $changelogPath -PathType Leaf) -and
                (Get-Content -LiteralPath $changelogPath -Raw) -notmatch ('(?m)^## \[' + [regex]::Escape($manifest.version) + '\]')) {
                Add-Failure "CHANGELOG.md must contain an entry for version $($manifest.version)."
            }
        }
        if ($manifest.unity -ne '2019.4') {
            Add-Failure 'package.json unity must be 2019.4.'
        }
        if ($manifest.dependencies.'com.unity.nuget.newtonsoft-json' -ne '3.0.3') {
            Add-Failure 'package.json must depend on com.unity.nuget.newtonsoft-json 3.0.3.'
        }
    }
    catch {
        Add-Failure "package.json is invalid JSON: $($_.Exception.Message)"
    }
}

foreach ($requiredFile in @(
    'LICENSE.md',
    'Third Party Notices.md',
    'CHANGELOG.md',
    'Editor/LStudios.DiscordUnityRpc.Editor.asmdef',
    'Editor/ThirdParty/DiscordRPC/DiscordRPC.dll',
    'Editor/ThirdParty/DiscordRPC/DiscordRPC.dll.meta',
    'Editor/ThirdParty/DiscordRPC/LICENSE',
    'Editor/ThirdParty/DiscordRPC/VERSION'
)) {
    [void](Require-File $requiredFile)
}

$pluginMetaPath = Join-Path $packageRoot 'Editor/ThirdParty/DiscordRPC/DiscordRPC.dll.meta'
if (Test-Path -LiteralPath $pluginMetaPath -PathType Leaf) {
    $pluginMeta = Get-Content -LiteralPath $pluginMetaPath -Raw
    if ($pluginMeta -notmatch '(?ms)^\s*Editor:\s*Editor\s*$.*?^\s*enabled:\s*1\s*$') {
        Add-Failure 'DiscordRPC.dll.meta must enable the Editor platform.'
    }
    if ($pluginMeta -notmatch '(?ms)^\s*Any:\s*$.*?^\s*enabled:\s*0\s*$') {
        Add-Failure 'DiscordRPC.dll.meta must disable the Any platform.'
    }
    if ($pluginMeta -match '(?m)^\s*(Standalone|Win|Win64|OSXUniversal|Linux64):\s*1\s*$') {
        Add-Failure 'DiscordRPC.dll.meta must not enable a player platform.'
    }
}

# Unity ignores any asset without a .meta inside an immutable (git or registry) package,
# so every importable file and folder must ship with one.
function Test-HiddenFromUnity {
    param([string]$RelativePath)

    foreach ($segment in $RelativePath.Split('/')) {
        if ($segment.StartsWith('.') -or $segment.EndsWith('~') -or $segment -ieq 'cvs' -or $segment.EndsWith('.tmp')) {
            return $true
        }
    }

    return $false
}

$packageRootFull = (Resolve-Path -LiteralPath $packageRoot).Path.TrimEnd('\', '/')
$guidOwners = @{}
$importableEntries = Get-ChildItem -LiteralPath $packageRootFull -Recurse -Force |
    ForEach-Object {
        [pscustomobject]@{
            Item = $_
            RelativePath = $_.FullName.Substring($packageRootFull.Length).TrimStart('\', '/').Replace('\', '/')
        }
    } |
    Where-Object { -not (Test-HiddenFromUnity $_.RelativePath) }

foreach ($entry in $importableEntries) {
    if ($entry.Item.Extension -eq '.meta' -and -not $entry.Item.PSIsContainer) {
        $assetPath = $entry.Item.FullName.Substring(0, $entry.Item.FullName.Length - '.meta'.Length)
        if (-not (Test-Path -LiteralPath $assetPath)) {
            Add-Failure "Orphaned meta file: $($entry.RelativePath)"
        }
        continue
    }

    # Git never publishes empty folders, so a local empty folder cannot reach users.
    if ($entry.Item.PSIsContainer -and $null -eq (Get-ChildItem -LiteralPath $entry.Item.FullName -Recurse -Force -File | Select-Object -First 1)) {
        continue
    }

    $metaPath = $entry.Item.FullName + '.meta'
    if (-not (Test-Path -LiteralPath $metaPath -PathType Leaf)) {
        Add-Failure "Missing meta file: $($entry.RelativePath).meta"
        continue
    }

    $metaContent = Get-Content -LiteralPath $metaPath -Raw
    $guidMatch = [regex]::Match($metaContent, '(?m)^guid:\s*([0-9a-f]{32})\s*$')
    if (-not $guidMatch.Success) {
        Add-Failure "Meta file has no valid GUID: $($entry.RelativePath).meta"
        continue
    }

    $guid = $guidMatch.Groups[1].Value
    if ($guidOwners.ContainsKey($guid)) {
        Add-Failure "Duplicate GUID $guid in $($entry.RelativePath).meta and $($guidOwners[$guid]).meta"
    }
    else {
        $guidOwners[$guid] = $entry.RelativePath
    }

    if ($entry.Item.PSIsContainer -and $metaContent -notmatch '(?m)^folderAsset:\s*yes\s*$') {
        Add-Failure "Folder meta file must declare folderAsset: yes: $($entry.RelativePath).meta"
    }
}

$sourceExtensions = @('.cs', '.json', '.asmdef', '.yml', '.yaml', '.ps1')
$secretPatterns = @('client_secret', 'oauth_token', 'bot_token', 'BEGIN PRIVATE KEY')
$scanRoots = @('Editor', 'Tests', '.github', 'package.json', 'Tools~')

foreach ($scanRoot in $scanRoots) {
    $fullScanRoot = Join-Path $packageRoot $scanRoot
    if (-not (Test-Path -LiteralPath $fullScanRoot)) {
        continue
    }

    $files = if (Test-Path -LiteralPath $fullScanRoot -PathType Leaf) {
        @(Get-Item -LiteralPath $fullScanRoot)
    }
    else {
        @(Get-ChildItem -LiteralPath $fullScanRoot -Recurse -File)
    }

    foreach ($file in $files) {
        if ($file.FullName -eq $PSCommandPath) {
            continue
        }
        if ($sourceExtensions -notcontains $file.Extension -and $file.Name -ne 'package.json') {
            continue
        }

        $content = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($pattern in $secretPatterns) {
            if ($content -match [regex]::Escape($pattern)) {
                $relativePath = $file.FullName.Substring($packageRoot.Length).TrimStart('\', '/')
                Add-Failure "Potential secret pattern '$pattern' found in $relativePath."
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Error ("Package validation failed:`n- " + ($failures -join "`n- "))
    exit 1
}

Write-Output 'Package validation passed.'
