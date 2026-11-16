# Validate the source contribution of the 2604 branch. No git or disk mutations.
[CmdletBinding()] param([switch]$Staged)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$manifest=Join-Path $root 'packaging/source-files.txt'
$paths=[IO.File]::ReadAllLines($manifest)
$allowed=@('README.md','.gitignore','build.ps1','run.ps1','packaging/source-files.txt',
           'release-docs/FORK-RELEASE.md','release-docs/RELEASE-NOTES.md','release-docs/START-HERE.md',
           'tools/PackRelease.ps1','tools/CheckReleaseSource.ps1','tools/Import2604.ps1')
foreach($projectPath in 'src/CreateDxDatabook/CreateDxDatabook_XENTP2604.vbproj','src/DxDbClass/DxDbClass_XENTP2604.vbproj') {
    $project=Join-Path $root $projectPath
    $allowed+=$projectPath
    [xml]$proj=Get-Content -LiteralPath $project -Raw
    $ns=New-Object Xml.XmlNamespaceManager $proj.NameTable
    $ns.AddNamespace('m',$proj.DocumentElement.NamespaceURI)
    foreach($entry in $proj.SelectNodes('//m:Compile|//m:EmbeddedResource|//m:Content|//m:None|//m:BaseApplicationManifest',$ns)) {
        $item=[IO.Path]::GetFullPath((Join-Path (Split-Path $project -Parent) ($entry.GetAttribute('Include'))))
        if(-not $item.StartsWith((Join-Path $root 'src\'),[StringComparison]::OrdinalIgnoreCase)) {throw "Project input outside src: $item"}
        $allowed+=$item.Substring($root.Length+1).Replace('\','/')
    }
}
$allowed+='src/CreateDxDatabook_XENTP2604.sln'
$mismatch=@(Compare-Object ($allowed | Sort-Object -Unique) ($paths | Sort-Object -Unique))
if($mismatch.Count){throw ("Manifest differs from project inputs and permitted release files: "+(($mismatch|ForEach-Object InputObject) -join ', '))}
foreach($p in $paths){
    if ($p -match '(^|/)(\.git|\.vs|bin|obj|build|logs|test-data|test-output|dist|Sqlite)(/|$)|\.(pfx|pdb|dll|exe|db3|dbc|lmc|cpd|ini|log)$') {throw "Local data or binary in source list: $p"}
    if(-not (Test-Path -LiteralPath (Join-Path $root $p) -PathType Leaf)){throw "Missing source file: $p"}
}
if ($Staged) {
    $top = & git -C $root rev-parse --show-toplevel
    if ($LASTEXITCODE -or [IO.Path]::GetFullPath($top) -ne [IO.Path]::GetFullPath($root)) { throw 'Expected a Git worktree at the source root.' }
    $indexed = @(& git -C $root -c core.quotepath=false ls-files --cached)
    if ($LASTEXITCODE) { throw 'Unable to inspect Git index.' }
    $delta = @(Compare-Object -CaseSensitive ($paths | Sort-Object) ($indexed | Sort-Object))
    if ($delta.Count) { throw ('Git index differs from the source allowlist: ' + (($delta | ForEach-Object InputObject) -join ', ')) }
    & git -C $root diff --quiet --
    if ($LASTEXITCODE) { throw 'Working tree files do not match the reviewed index.' }
}
Write-Host ("Validated {0} source paths." -f $paths.Count)
