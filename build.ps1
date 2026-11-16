param([string]$SddHome = 'C:\SiemensEDA\XENTP2604\SDD_HOME')
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
New-Item -ItemType Directory -Path build,logs,bin\XENTP2604 -Force | Out-Null
$env:TEMP = Join-Path $PSScriptRoot 'build'
$env:TMP = $env:TEMP
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
