# CreateDxDatabook dla Xpedition 2604 (Windows x64)

**Wersja wydania: v26.4.0** (dla Xpedition 2604).

Nieoficjalna adaptacja narzędzia [MGC-CrDB/CrDB](https://github.com/MGC-CrDB/CrDB). Wymaga działającej instalacji i licencji Siemens Xpedition 2604, zarejestrowanych komponentów automatyzacji x64 oraz .NET Framework 4.8. Możesz pozostawić 2409 na tym samym komputerze.

## Uruchomienie z oknem

1. Rozpakuj cały ZIP do katalogu, w którym możesz zapisywać pliki. Zostaw `CreateDxDatabook.exe.config`, `DxDbClass151.dll` i `System.Data.SQLite.dll` obok programu.
2. Uruchom `CreateDxDatabook.exe`, wybierz plik `.lmc` z kopii kompletnej biblioteki i zaznacz **Create/Update DBC File**, jeśli chcesz także plik konfiguracyjny. Uruchom generowanie.
3. Po zakończeniu **okno pozostaje otwarte**: możesz przeczytać komunikaty, wybrać inną bibliotekę lub zamknąć program krzyżykiem. Nie musisz go zamykać, aby użyć wyniku.

## Automatyczne zakończenie po jednym eksporcie

W PowerShell otwórz katalog rozpakowanego programu. Zastąp przykładowe ścieżki własnymi; do pierwszego testu użyj pustego katalogu wynikowego. Ponowne uruchomienie zastępuje poprzednie wyniki o tej samej nazwie.

~~~powershell
& .\CreateDxDatabook.exe --batch --lmc 'D:\Biblioteki\Kopia\Library.lmc' --out 'D:\Wyniki\Databook' | Out-Host
$LASTEXITCODE
~~~

Ten tryb kończy się sam, bez naciskania klawisza. **0** oznacza powodzenie, **1** błąd. Po udanym eksporcie w katalogu wynikowym znajdziesz `Library.db3`, `Library.dbc` i `generation.log`. W logu sprawdzisz wybrany release i katalog SDD_HOME. Do automatyzacji używaj `--batch`; historyczna opcja `-con` czeka po eksporcie na klawisz.

W Databook w Designerze 2604 wskaż plik **.dbc**. Gdy DB3 i DBC wygenerowano w jednym katalogu, przenoś je razem. Gdy w GUI wybrano osobne katalogi, konfiguracja wskazuje pełną ścieżkę bazy; po jej przeniesieniu wygeneruj DBC ponownie.

## Pochodzenie komponentów

Podstawą jest narzędzie CreateDxDatabook dla XENTP2409 z repozytorium MGC-CrDB/CrDB. Zachowano noty autorów w aplikacji. Wydanie nie jest oficjalnym produktem Siemens. Zestaw korzysta z biblioteki System.Data.SQLite x64 z pakietu upstream. Komponenty Xpedition i licencję trzeba zapewnić oddzielnie; pakiet ich nie instaluje ani nie przerejestrowuje.

## Budowanie ze źródeł

W katalogu głównym forka, w 64-bitowym Windows PowerShell, uruchom:

~~~powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
~~~

Przy niestandardowej ścieżce instalacji dodaj np. `-SddHome 'D:\SiemensEDA\XENTP2604\SDD_HOME'`. Wynik kompilacji znajduje się w `bin\XENTP2604`. Skrypt jest częścią repozytorium; nie potrzebujesz go do uruchomienia gotowego ZIP.
