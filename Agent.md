# Lucide C# (Lucide.NET) — Codex Agent Instructions

You are Codex working inside a repository named **lucide-csharp**. Your goal is to build an end-to-end, production-ready, open-source Lucide icon library for the C# ecosystem, supporting **.NET Framework 4.8** through **.NET 10**, with first-class packages for **WPF**, **WinForms**, and **Blazor**.

## Core principles
- **Performance first**: no repeated SVG parsing; use caching and frozen/freezable objects where applicable.
- **Developer-friendly**: strongly typed `LucideIconKind`, simple controls/components, intuitive APIs.
- **Renderer-agnostic core**: Core package contains icon catalog and SVG retrieval; UI packages contain rendering.
- **Easy updates**: a generator can sync from upstream Lucide and regenerate code + resources.
- **NuGet-ready**: multi-targeting, SourceLink, symbols, deterministic builds, CI release pipeline.
- **Open source**: clean docs, contribution guide, code of conduct, license.

## Non-goals
- Do not lock the entire solution to a single SVG renderer (SharpVectors is allowed for WPF only).
- Do not parse SVG on every render (must cache).
- Do not store icons as loose files loaded from disk at runtime (use embedded binary blob + generated index).

## Repository targets and packages
Create these NuGet packages:
1. `Lucide.NET.Core` (netstandard2.0;net48)
2. `Lucide.NET.Wpf` (net48;net6.0-windows;net8.0-windows;net10.0-windows)
3. `Lucide.NET.WinForms` (net48;net6.0-windows;net8.0-windows;net10.0-windows)
4. `Lucide.NET.Blazor` (net8.0;net9.0;net10.0)

## Required outputs
- Full repo structure under `src/`, `tests/`, `samples/`, `eng/`, `.github/workflows/`
- Generator tool under `eng/icons/generator/` that:
  - downloads/reads Lucide SVGs (from a provided folder path)
  - normalizes names
  - produces:
    - `LucideIconKind.g.cs` enum
    - `LucideAliases.g.cs` (string->kind map)
    - `LucideIconIndex.g.cs` (offset/length table)
    - `lucide_icons.bin` (concatenated compressed SVG payloads)
- Core library that:
  - exposes `Lucide.GetSvg(kind, options)`
  - exposes `TryResolve(name, out kind)` (aliases + canonical)
  - reads embedded blob with offsets and decompresses on demand with caching
- WPF library that:
  - provides `<lucide:LucideIcon Kind="Home" />`
  - provides MarkupExtension to produce `ImageSource`
  - caches `DrawingImage` and freezes it
- WinForms library that:
  - provides `LucideWinForms.ToBitmap(...)` and `ToIcon(...)`
  - caches bitmaps per key (kind/size/color/strokeWidth/dpi)
- Blazor library that:
  - provides `<LucideIcon Kind="Home" Size="18" Stroke="currentColor" />`
  - outputs inline `<svg>` efficiently (no JS dependency)
- Docs:
  - README with quick-start for all frameworks
  - CONTRIBUTING.md (including how to update icon set)
  - CODE_OF_CONDUCT.md
  - SECURITY.md (basic)
  - CHANGELOG.md
- CI:
  - `ci.yml` build/test/pack + upload artifacts
  - `release.yml` publish to NuGet on tag `v*`
  - optional `icons-sync.yml` for scheduled upstream sync PR
- Versioning:
  - single source of truth in `eng/versioning/version.json`
  - build reads version to set `PackageVersion` and `AssemblyVersion`

## Quality gates
- Add unit tests verifying:
  - enum contains expected sample icons
  - `TryResolve` resolves canonical names and aliases
  - retrieving SVG returns valid `<svg ...>` text
  - WPF renderer caches and returns a frozen `ImageSource` (when possible)
- Add a simple perf sanity test (optional) that loads 200 icons and asserts completion under a generous time budget.

## Tooling constraints
- Assume a Windows dev environment.
- Avoid external runtime dependencies beyond what is necessary (SharpVectors allowed in WPF package).
- Use modern C# features where supported, but keep cross-target compatibility (net48 vs net8+).
- Keep public API stable and well documented with XML docs.

## Implementation notes (must follow)
- Store icons as a single embedded resource `lucide_icons.bin` in `Lucide.NET.Core`.
- Store a generated index table mapping `LucideIconKind` -> `(offset,length,compression)`; use compression `gzip` by default.
- Decompression cache:
  - `ConcurrentDictionary<LucideIconKind, string>` for decoded SVG.
- WPF cache key includes:
  - kind, size, stroke thickness, brush color (ARGB), and optionally `Stretch`
- WinForms cache key includes:
  - kind, size, stroke width, color ARGB, dpi
- Blazor should avoid heavy allocations:
  - precompute attribute strings where possible
  - `RenderTreeBuilder` or `.razor` markup OK

## Acceptance criteria
- `dotnet build` succeeds.
- `dotnet test` succeeds.
- `dotnet pack` produces `.nupkg` and `.snupkg` for all packages.
- Samples run showing icons.
- README documents installation + usage.
- Release workflow is ready (requires secrets only).