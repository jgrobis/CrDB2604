# CreateDxDatabook dla Siemens Xpedition 2604 · Windows x64

**Wersja wydania: v26.4.1** (dla Xpedition 2604).

Nieoficjalna adaptacja [CreateDxDatabook](https://github.com/MGC-CrDB/CrDB). Ten wariant jawnie wybiera **XENTP2604 / win64 / COM 151**. Może pracować na komputerze z zainstalowaną równolegle wersją 2409; nie wymaga globalnego przełączania środowiska.

- Odczyt PDB korzysta z wersjonowanego Parts Editor 151. Program sprawdza release i ścieżki serwerów COM przed otwarciem biblioteki.
- Zachowano eksport części, właściwości, symboli i pinów do DB3 oraz konfiguracji DBC. Wspólnie wygenerowane pliki DB3/DBC można przenosić razem.
- **GUI pozostaje otwarte** po eksporcie. **Tryb `--batch` kończy się automatycznie**, zapisuje log i zwraca kod 0/1.

**Układ wydania:** w zakładce Code są tylko źródła i narzędzia do budowy wariantu 2604. Gotowy program dla Windows x64 udostępniany jest oddzielnie jako ZIP w Releases. Do kompilacji nie trzeba zachowywać katalogów wcześniejszych wydań.

**Weryfikacja:** eksport na kopii biblioteki odczytał 23 partycje i 505 części. Wariant z symbolami i pinami zapisał 537 rekordów symboli. Sprawdzono GUI, pracę przy ustawieniach 2409, integralność SQLite oraz odwołania do tabel i pól w DBC. Nie wykonywano ręcznego importu DBC w otwartym Designerze.

**Instalacja:** pobierz ZIP dla Windows x64, rozpakuj wszystkie pliki razem i przeczytaj `START-HERE.md`. Wymagane są .NET Framework 4.8 i działająca instalacja/licencja Xpedition 2604 z zarejestrowanymi komponentami COM x64.
