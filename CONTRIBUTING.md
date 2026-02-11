# Contributing

Thanks for helping improve Lucide.NET!

**Build**
1. Restore and build:
```bash
dotnet build
```
2. Run tests:
```bash
dotnet test -c Release
```
3. Create packages:
```bash
dotnet pack -c Release
```

**Updating Icons**
1. Fetch Lucide SVGs:
```powershell
.\eng\icons\update-lucide.ps1 -Version main
```
Or copy from a local checkout:
```powershell
.\eng\icons\update-lucide.ps1 -SourcePath C:\path\to\lucide
```
2. Generate code and resources:
```powershell
.\eng\icons\generate.ps1 -InputPath .\eng\icons\lucide
```
3. Verify output:
- `src/Lucide.NET.Core/Generated/*.g.cs`
- `src/Lucide.NET.Core/Resources/lucide_icons.bin`

**Release**
1. Update `CHANGELOG.md`.
2. Set the version:
```powershell
.\eng\versioning\set-version.ps1 -Version 0.1.0
```
3. Validate:
```bash
dotnet test -c Release
dotnet pack -c Release
```
4. Commit, tag, and push:
```bash
git tag v0.1.0
git push --tags
```
5. The release workflow publishes packages from tags that start with `v`.

**Pull requests**
- Keep changes focused.
- Include tests when behavior changes.
- Update docs when adding or changing public API.
