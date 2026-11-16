# CreateDxDatabook · Xpedition 2604 (Windows x64)

Unofficial port of [MGC-CrDB/CrDB](https://github.com/MGC-CrDB/CrDB) for Siemens Xpedition **2604 / COM 151**. Generates a SQLite parts database (.db3) and a Databook configuration (.dbc) from a central library (.lmc).

**Download the compiled Windows program:** [Releases](https://github.com/jgrobis/CrDB2604/releases). Choose `CreateDxDatabook-Xpedition2604-v26.4.1-win-x64.zip`, not GitHub's automatic “Source code” download. Unzip all five files together. You need a licensed installation of Xpedition 2604 x64 and .NET Framework 4.8.

**Build it yourself:** the [Visual Basic solution](src/CreateDxDatabook_XENTP2604.sln) and build scripts in this branch target 2604 only. From a 64-bit Windows PowerShell, run:

~~~powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
~~~

The first build needs internet access for the .NET reference assemblies and the pinned x64 SQLite dependency. The Siemens COM type libraries come from your installed Xpedition 2604. No installation of Xpedition 2409 is needed. EXE, DLL, user libraries and output databases are not stored in the source tree.

- [Run the GUI or export in batch (Polski)](release-docs/START-HERE.md)
- [What changed in v26.4.1](release-docs/RELEASE-NOTES.md)
- [Package a release](release-docs/FORK-RELEASE.md)
