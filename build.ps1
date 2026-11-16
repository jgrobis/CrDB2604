param([string]$SddHome = 'C:\SiemensEDA\XENTP2604\SDD_HOME')
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
New-Item -ItemType Directory -Path build,logs,bin\XENTP2604 -Force | Out-Null
$env:TEMP = Join-Path $PSScriptRoot 'build'
$env:TMP = $env:TEMP
# Restore the exact mixed-mode x64 provider used by the original exporter.
# Only the ignored build cache receives the binary, never the source tree.
$dependencyDir = Join-Path $PSScriptRoot 'build\deps\x64'
New-Item -ItemType Directory -Force -Path $dependencyDir | Out-Null
$sqliteDll = Join-Path $dependencyDir 'System.Data.SQLite.dll'
$sqliteSha = '9866DCEB181852EAB3735AFEEBBAC1EA7810857201319C2C5A2AF4667EAA8D45'
$sqliteUri = 'https://raw.githubusercontent.com/MGC-CrDB/CrDB/48d3c280088c4184e44cba1c0f85fc04487632d3/CreateDxDatabook/Sqlite/netFx40/1.0.66/x64/System.Data.SQLite.dll'
if (-not (Test-Path -LiteralPath $sqliteDll -PathType Leaf)) {
    $download = Join-Path $dependencyDir ([IO.Path]::GetRandomFileName())
    try {
        Invoke-WebRequest -Uri $sqliteUri -OutFile $download -UseBasicParsing
        if ((Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash -ne $sqliteSha) { throw 'Unexpected SHA-256 for downloaded SQLite x64 provider.' }
        [IO.File]::Move($download, $sqliteDll)
    } finally {
        if (Test-Path -LiteralPath $download -PathType Leaf) { Remove-Item -LiteralPath $download -Force }
    }
}
if ((Get-FileHash -LiteralPath $sqliteDll -Algorithm SHA256).Hash -ne $sqliteSha) { throw "Incorrect cached dependency: $sqliteDll. Remove it and rebuild." }
$package = Join-Path $PSScriptRoot 'build\net48.nupkg'
$refs = Join-Path $PSScriptRoot 'build\net48\build'
if (-not (Test-Path "$refs\.NETFramework\v4.8\mscorlib.dll")) {
    Invoke-WebRequest 'https://api.nuget.org/v3-flatcontainer/microsoft.netframework.referenceassemblies.net48/1.0.3/microsoft.netframework.referenceassemblies.net48.1.0.3.nupkg' -OutFile $package -UseBasicParsing
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($package, (Join-Path $PSScriptRoot 'build\net48'))
}
$ps = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
& $ps -NoProfile -ExecutionPolicy Bypass -File tools\Import2604.ps1 -SddHome $SddHome > logs\interop.txt 2>&1
if ($LASTEXITCODE -ne 0) { throw '2604 type library import failed; see logs\interop.txt' }
$msbuild = "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
& $msbuild src\CreateDxDatabook_XENTP2604.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64 "/p:TargetFrameworkRootPath=$refs\" "/p:OutDir=$PSScriptRoot\bin\XENTP2604\" /v:minimal /nologo > logs\build.txt 2>&1
Get-Content logs\build.txt
if ($LASTEXITCODE -ne 0) { throw 'Build failed; see logs\build.txt' }
Write-Host "Built $PSScriptRoot\bin\XENTP2604\CreateDxDatabook.exe"
