# Lucide.NET

Lucide.NET provides fast, strongly-typed Lucide icons for the C# ecosystem with first-class support for WPF, WinForms, and Blazor.

**Packages**
- `Lucide.NET.Core` (shared icon catalog + SVG retrieval)
- `Lucide.NET.Wpf` (WPF controls and markup extension)
- `Lucide.NET.WinForms` (WinForms bitmap/icon helpers)
- `Lucide.NET.Blazor` (Blazor component)

**Install**
```bash
dotnet add package Lucide.NET.Core
dotnet add package Lucide.NET.Wpf
dotnet add package Lucide.NET.WinForms
dotnet add package Lucide.NET.Blazor
```

**WPF**
```xml
<Window
    xmlns:lucide="http://lucide.net/wpf">
    <StackPanel>
        <lucide:LucideIcon Kind="Home" Size="20" StrokeThickness="2" Stroke="DodgerBlue" />
        <Image Source="{lucide:LucideImage Home, Size=18, Brush=Black}" />
    </StackPanel>
</Window>
```

Lucide.NET.Wpf uses the SharpVectors.Wpf package (v1.8.5) to convert SVG markup into frozen `DrawingImage` instances.

**WinForms**
```csharp
using System.Drawing;
using Lucide.NET;
using Lucide.NET.WinForms;

var bitmap = LucideWinForms.ToBitmap(LucideIconKind.Home, 24, Color.Black);
var icon = LucideWinForms.ToIcon(LucideIconKind.AlertCircle, 16, Color.DarkRed);
```

Lucide.NET.WinForms uses the Svg package (v3.4.7) to rasterize SVG markup into cached `Bitmap` instances.

**Blazor**
```razor
@using Lucide.NET
@using Lucide.NET.Blazor

<LucideIcon Kind="LucideIconKind.ShoppingCart" Size="24" Stroke="currentColor" StrokeWidth="2" />
```

**Performance and Caching**
- Core caches decompressed SVG strings by `LucideIconKind`.
- WPF caches frozen `DrawingImage` instances by kind, size, stroke thickness, and stroke color.
- WinForms caches rasterized `Bitmap` instances by kind, size, stroke width, color, and DPI. `ToBitmap` returns the cached instance; clone it if you need to dispose:
```csharp
using var bitmap = new Bitmap(LucideWinForms.ToBitmap(LucideIconKind.Home, 24, Color.Black));
```

**Updating Icons**
1. Download or copy Lucide SVGs:
```powershell
.\eng\icons\update-lucide.ps1 -Version main
.\eng\icons\update-lucide.ps1 -SourcePath C:\path\to\lucide
```
2. Run the generator:
```powershell
.\eng\icons\generate.ps1 -InputPath .\eng\icons\lucide
```
3. Commit generated files in `src/Lucide.NET.Core/Generated` and `src/Lucide.NET.Core/Resources`.

**License**
MIT. See `LICENSE`.
