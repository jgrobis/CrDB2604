# Package a Windows release

Use a checkout of the source commit you intend to release. Building requires Windows x64, .NET Framework 4.8, and Xpedition 2604. If the installation uses a non-default location, pass `-SddHome` to the packaging script.

1. From the repository root, run:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\PackRelease.ps1
   Get-Content .\dist\SHA256SUMS.txt
   ```

2. Extract the ZIP from `dist\` into a separate folder and run the extracted executable against a copy of a complete central library. Check its exit code, the generated DB3 and DBC, and the generation log. To verify the ZIP itself, compare `Get-FileHash <path-to-zip> -Algorithm SHA256` with the digest in `SHA256SUMS.txt`.

3. Push the tested source commit and its tag to your fork. Attach **the Windows ZIP** and **SHA256SUMS.txt** to a release for that tag. Keep `dist\` out of the source commit. For example, after pushing tag `v26.4.2` on this fork:

   ```powershell
   gh release create v26.4.2 .\dist\CreateDxDatabook-Xpedition2604-v26.4.2-win-x64.zip .\dist\SHA256SUMS.txt --repo jgrobis/CrDB2604 --verify-tag --draft --title "CreateDxDatabook v26.4.2 for Xpedition 2604 (Windows x64, unofficial)" --notes-file .\release-docs\RELEASE-NOTES.md
   ```

4. Review the assets and [release notes](RELEASE-NOTES.md) in the draft, then publish it. GitHub provides separate source archives automatically. If publishing from another fork, replace the repository name in the example with your own.