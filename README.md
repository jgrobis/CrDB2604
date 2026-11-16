# CreateDxDatabook for Xpedition 2604 (Windows x64)

CreateDxDatabook reads a Siemens Xpedition central library (`.lmc`) and exports its parts to a SQLite database (`.db3`) and a Databook configuration (`.dbc`). This is an unofficial port of [MGC-CrDB/CrDB](https://github.com/MGC-CrDB/CrDB) for Xpedition 2604. It selects the registered 2604 automation components (COM 151), including on systems with 2409 installed alongside 2604.

OpenAI Codex assisted with porting CreateDxDatabook to Xpedition 2604 and testing the GUI and batch exports.

**[Download the Windows x64 program](https://github.com/jgrobis/CrDB2604/releases/latest)** and choose the `win-x64.zip` asset. Extract all five files into the same folder. You need a licensed installation of Xpedition 2604 with its x64 automation components and .NET Framework 4.8.

- [Run the GUI or export in batch mode](release-docs/START-HERE.md)
- [Release notes for v26.4.2](release-docs/RELEASE-NOTES.md)
- [Package a release from source](release-docs/FORK-RELEASE.md)

To build the [Visual Basic solution](src/CreateDxDatabook_XENTP2604.sln), open a 64-bit Windows PowerShell session in the repository root and run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

On the first build, the script downloads the .NET reference assemblies and an x64 SQLite provider. It verifies the provider against its pinned SHA-256 digest and reads the Siemens type libraries from your installed copy of Xpedition 2604. The executable is written to `bin\XENTP2604`.