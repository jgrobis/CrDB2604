# CreateDxDatabook v26.4.2 for Xpedition 2604 (Windows x64)

The [quick-start guide](https://github.com/jgrobis/CrDB2604/blob/v26.4.2/release-docs/START-HERE.md), release notes, and maintainer instructions are now in English. The quick-start guide is also included in the Windows ZIP. The examples use generic paths you can replace with your own.

The Help menu describes the SQLite export options and uses generic examples. The obsolete Access DAO registry repair command has been removed.

**Download:** choose `CreateDxDatabook-Xpedition2604-v26.4.2-win-x64.zip` and extract all its files together. GitHub's automatic “Source code” downloads are for building the program yourself. The Windows package requires .NET Framework 4.8 and a licensed installation of Siemens Xpedition 2604 with x64 automation registered.

This unofficial port selects **Xpedition 2604 / COM 151** explicitly. The GUI stays open after an export. For unattended runs, `--batch` writes the SQLite database, DBC configuration, and log, then exits with code 0 on success or 1 on failure.

The 2604 exporter has been tested on a copy of a central library, including selection of 2604 when 2409 environment variables are present. Checks covered GUI operation, SQLite integrity, and DBC table and field mappings. Opening the DBC interactively in Designer was not part of those tests.