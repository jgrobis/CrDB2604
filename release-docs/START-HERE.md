# Run CreateDxDatabook for Xpedition 2604

**v26.4.2 · Windows x64.** This unofficial build requires .NET Framework 4.8 and a licensed, working installation of Siemens Xpedition 2604 with the x64 automation components registered. Xpedition 2409 can remain installed.

## Export using the GUI

1. Download the `win-x64.zip` asset from [Releases](https://github.com/jgrobis/CrDB2604/releases/latest) and extract it to a writable folder. Keep `CreateDxDatabook.exe.config`, `DxDbClass151.dll`, and `System.Data.SQLite.dll` beside `CreateDxDatabook.exe`.
2. Open `CreateDxDatabook.exe`, click **Select LMC File**, and choose the `.lmc` file in a working copy of your **complete** central library. The `.lmc` file by itself does not contain all the parts data. The GUI can invalidate the library's existing `.cpd` cache after an export.
3. Select **Create/Update DBC File** if you want the Databook configuration, then click **Create Database**. If a DBC already exists, the GUI asks what to do with it. The window stays open after the export so you can read the transcript.

## Export once and exit automatically

In PowerShell, open the folder containing the extracted executable. Replace the sample paths below with your library copy and a separate output folder. Use an empty output folder for the first run: subsequent exports can replace files with the same name.

```powershell
& .\CreateDxDatabook.exe --batch --lmc 'D:\Libraries\Copy\Library.lmc' --out 'D:\Exports\Databook' | Out-Host
$LASTEXITCODE
```

Batch mode exits without waiting for a keypress. An exit code of **0** means success; **1** means failure. For the example above, a successful run creates `Library.db3`, `Library.dbc`, and `generation.log` in the output folder. The log identifies the selected release and `SDD_HOME`. To include the separate symbols table or cell pin counts, add `--symbols` or `--cell-pins`.

## Use the result

Select the exported **.dbc** file in Databook in Xpedition Designer 2604. When DBC and DB3 are generated together, keep them in the same folder if you move them. If you use the GUI to place them in separate folders, the DBC records the full database path; regenerate the configuration if that path changes.

## Build from source instead

From the repository root, use 64-bit Windows PowerShell:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

For a non-default installation, add `-SddHome 'D:\SiemensEDA\XENTP2604\SDD_HOME'`. The program is written to `bin\XENTP2604`. Building is unnecessary when using the release ZIP.

This build is based on [MGC-CrDB/CrDB](https://github.com/MGC-CrDB/CrDB) and uses the upstream x64 System.Data.SQLite provider. It does not install or re-register Siemens components.