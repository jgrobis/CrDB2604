# Przygotowanie wydania

Pracuj na czystym drzewie źródeł z tego forka. Kompilacja wymaga Windows x64, .NET Framework 4.8 oraz działającej instalacji Xpedition 2604; zależności do pierwszego buildu pobiera build.ps1.

~~~powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\PackRelease.ps1
Get-Content .\dist\SHA256SUMS.txt
~~~

Dla niestandardowej instalacji dodaj `-SddHome 'D:\SiemensEDA\XENTP2604\SDD_HOME'`. Przetestuj EXE z rozpakowanego ZIP-a na kopii biblioteki. Gotowych plików z dist nie dodawaj do commita.

W Releases dla właściwego tagu dołącz **ZIP programu dla win-x64** i **SHA256SUMS.txt**. [Opis wydania](RELEASE-NOTES.md) można podać w `--notes-file`. Przykład dla utrzymującego fork, po wysłaniu commita i tagu:

~~~powershell
gh release create v26.4.1 .\dist\CreateDxDatabook-Xpedition2604-v26.4.1-win-x64.zip .\dist\SHA256SUMS.txt --repo jgrobis/CrDB2604 --verify-tag --title "CreateDxDatabook v26.4.1 for Xpedition 2604 (Windows x64, unofficial)" --notes-file .\release-docs\RELEASE-NOTES.md
~~~

GitHub dodaje także archiwa źródeł z tagu automatycznie. Starsze pliki można odnaleźć w historii projektu macierzystego; nie są potrzebne w bieżącym drzewie forka.
