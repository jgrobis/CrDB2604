# Fork i nieoficjalne wydanie v26.4.0

Wydanie: **CreateDxDatabook v26.4.0 for Xpedition 2604 (Windows x64, unofficial)**. Nazwa produktu Siemens opisuje kompatybilność. Tag Git to `v26.4.0`, wersja pliku EXE to `26.4.0.0`, a wersja COM to `151`.

## 1. Sprawdź przygotowane wydanie

Z katalogu głównego lokalnej kopii, w PowerShell:

~~~powershell
git log -1 --oneline v26.4.0
git diff --name-status 48d3c28 v26.4.0
Get-FileHash .\dist\CreateDxDatabook-Xpedition2604-v26.4.0-win-x64.zip -Algorithm SHA256
Get-Content .\dist\SHA256SUMS.txt
~~~

Gotowe pliki do załączenia leżą w `dist/`. Aby odtworzyć paczkę z czystej kopii forka, pobierz właściwy tag i uruchom z katalogu repozytorium:

~~~powershell
git switch --detach v26.4.0
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\PackRelease.ps1
~~~

Buduj na Windows x64 z zainstalowanym Xpedition 2604 i .NET Framework 4.8. Jeżeli instalacja leży poza standardowym katalogiem, dodaj `-SddHome 'D:\SiemensEDA\XENTP2604\SDD_HOME'`. Skrypt buduje EXE z lokalnych bibliotek typów Siemens i umieszcza w ZIP wyłącznie cztery pliki uruchomieniowe oraz START-HERE.md. Pierwszy build może pobrać referencje .NET z NuGet. Sama kompilacja, bez ZIP: `.\build.ps1`. Po każdym ponownym pakowaniu użyj sumy SHA-256 obliczonej dla **tej konkretnej paczki**.

Po budowaniu do publikacji pracuj znów na przygotowanej gałęzi: `git switch release/xentp2604` (w pierwotnej lokalnej kopii).

## 2. Utwórz własny fork i wyślij przygotowany commit

[Utwórz fork MGC-CrDB/CrDB](https://github.com/MGC-CrDB/CrDB/fork) na swoje konto. W istniejącej kopii zostaw `origin` wskazujący upstream; dodaj **oddzielny** remote. Podmień TWOJ_LOGIN (i nazwę repozytorium, jeśli zmieniłeś ją przy forkowaniu):

~~~powershell
git remote add moj-fork https://github.com/TWOJ_LOGIN/CrDB.git
git remote get-url --push moj-fork
git push --atomic moj-fork refs/heads/release/xentp2604:refs/heads/master refs/tags/v26.4.0
~~~

Polecenie aktualizuje `master` **Twojego forka** do przygotowanego commitu i wysyła tag. Dzięki temu na stronie głównej forka będzie instrukcja 2604. Jeżeli zdalny master ma już własne zmiany lub jest chroniony, nie używaj force-push. Wyślij osobną gałąź i tag (`git push --atomic moj-fork refs/heads/release/xentp2604 refs/tags/v26.4.0`), a następnie otwórz PR z tej gałęzi do master **we własnym forku**. Nie przenoś tagu opublikowanego już dla innego commitu.

## 3. Utwórz release na GitHubie

Na stronie **swojego forka** wybierz **Releases → Draft a new release**. Wybierz istniejący tag **v26.4.0**, wpisz tytuł podany wyżej i wklej [gotowe release notes](RELEASE-NOTES.md). Załącz:
- `dist/CreateDxDatabook-Xpedition2604-v26.4.0-win-x64.zip`
- `dist/SHA256SUMS.txt`

Najpierw zapisz draft; po sprawdzeniu tagu i załączników wybierz **Publish release**. ZIP przygotowany tutaj zawiera program. Automatyczne „Source code (zip)” GitHuba to osobne archiwum źródeł.

Alternatywnie, po zalogowaniu w GitHub CLI:

~~~powershell
gh release create v26.4.0 .\dist\CreateDxDatabook-Xpedition2604-v26.4.0-win-x64.zip .\dist\SHA256SUMS.txt --repo TWOJ_LOGIN/CrDB --verify-tag --draft --title "CreateDxDatabook v26.4.0 for Xpedition 2604 (Windows x64, unofficial)" --notes-file .\release-docs\RELEASE-NOTES.md
~~~

Dokumentacja: [fork repozytorium](https://docs.github.com/en/get-started/exploring-projects-on-github/contributing-to-a-project), [gh release create](https://cli.github.com/manual/gh_release_create).

**Przed publicznym pushem zmienionego kodu lub udostępnieniem binariów:** upstream nie zawiera pliku licencji projektu, a aplikacja zachowuje noty praw autorskich. Samo utworzenie forka na GitHubie nie rozstrzyga prawa do redystrybucji modyfikacji i plików binarnych. Potwierdź uprawnienia u właściciela praw, jeżeli jeszcze ich nie masz. [Licencjonowanie repozytoriów na GitHubie](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository).
