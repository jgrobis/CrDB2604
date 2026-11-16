# Build the unofficial Windows x64 package from an explicit list of runtime files.
[CmdletBinding()]
param([string]$SddHome = 'C:\SiemensEDA\XENTP2604\SDD_HOME')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$stem = 'CreateDxDatabook-Xpedition2604-v26.4.1-win-x64'
$bin = Join-Path $root 'bin\XENTP2604'
$dist = Join-Path $root 'dist'
& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "$root\build.ps1" -SddHome $SddHome
if ($LASTEXITCODE) { throw 'Build failed: package not generated.' }
foreach ($file in 'CreateDxDatabook.exe','DxDbClass151.dll') {
    $product = Get-Item -LiteralPath (Join-Path $bin $file)
    if ($product.VersionInfo.FileVersion -ne '26.4.1.0') { throw "Unexpected file version: $file" }
}
$assets = [ordered]@{}
foreach ($name in 'CreateDxDatabook.exe','CreateDxDatabook.exe.config','DxDbClass151.dll','System.Data.SQLite.dll') {
    $assets[$name] = Join-Path $bin $name
}
$assets['START-HERE.md'] = Join-Path $root 'release-docs\START-HERE.md'
$archive = Join-Path $dist "$stem.zip"
New-Item -ItemType Directory -Path $dist -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$tmp = Join-Path $dist ([IO.Path]::GetRandomFileName())
try {
    $zip = [IO.Compression.ZipFile]::Open($tmp,[IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in $assets.Keys) {
            [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$assets[$file],$file)
        }
    } finally { $zip.Dispose() }
    if ([IO.File]::Exists($archive)) { [IO.File]::Replace($tmp,$archive,[System.Management.Automation.Language.NullString]::Value) } else { [IO.File]::Move($tmp,$archive) }
} finally { if ([IO.File]::Exists($tmp)) { [IO.File]::Delete($tmp) } }
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $dist 'SHA256SUMS.txt'), "$hash  $stem.zip" + [Environment]::NewLine, [Text.Encoding]::ASCII)
Write-Host "$archive  SHA256: $hash"
