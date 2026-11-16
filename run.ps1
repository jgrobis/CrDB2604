param([Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$Lmc,
      [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$Out,
      [string]$SddHome = '', [switch]$Symbols, [switch]$CellPins,
      [switch]$AllowDuplicates, [string]$PropertyFile = '', [string]$Delimiter = '|')
$ErrorActionPreference = 'Stop'
# Resolve all user paths relative to the calling shell, not the repository.
$Lmc = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Lmc)
$Out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
if ($PropertyFile) { $PropertyFile = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PropertyFile) }
if ($SddHome) { $SddHome = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($SddHome) }
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
