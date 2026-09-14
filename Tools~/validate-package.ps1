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
        if ($manifest.version -ne '1.0.0') {
            Add-Failure 'package.json version must be 1.0.0.'
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
