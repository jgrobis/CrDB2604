param([string]$Lmc = "$PSScriptRoot\test-data\MASTER_LIBRARY\MASTER_LIBRARY.lmc",
      [string]$Out = "$PSScriptRoot\test-output\default",
      [string]$SddHome = '', [switch]$Symbols, [switch]$CellPins,
      [switch]$AllowDuplicates, [string]$PropertyFile = '', [string]$Delimiter = '|')
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
New-Item -ItemType Directory -Path "$PSScriptRoot\build\runtime" -Force | Out-Null
$env:TEMP="$PSScriptRoot\build\runtime"; $env:TMP=$env:TEMP; $env:WDIR=$env:TEMP
if ($PSBoundParameters.ContainsKey('SddHome')) { $env:CRDB2604_SDD_HOME=$SddHome }
$arguments=@('--batch','--lmc',$Lmc,'--out',$Out)
if($Symbols) {$arguments+='--symbols'}
if($CellPins) {$arguments+='--cell-pins'}
if($AllowDuplicates) {$arguments+='--allow-duplicates'}
if($PropertyFile) {$arguments+=@('--psf',$PropertyFile,'--delimiter',$Delimiter)}
& "$PSScriptRoot\bin\XENTP2604\CreateDxDatabook.exe" @arguments | Out-Host
exit $LASTEXITCODE
