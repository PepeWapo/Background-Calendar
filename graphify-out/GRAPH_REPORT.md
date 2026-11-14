# Graph Report - AgendaDinamica  (2026-11-14)

## Corpus Check
- 70 files · ~28,363 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 756 nodes · 1448 edges · 28 communities (25 shown, 3 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 39 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5d0dcfe0`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Window
- VistaDia
- IconRailWindow
- VistaMes
- PanelGuia
- AgendaService
- PepeWapOSx10.Shell.Widgets
- AnclajeEscritorio
- EspejoWallpaperWindow
- VideoWallpaperWindow
- Window
- MiniCalendario
- Correr Agenda Dinámica
- TaskbarWindow
- .Empaquetar
- DetectorDobleClick
- AccesosDirectos
- PepeWapOSx10.Dominio.csproj
- Background Calendar (PepeWapOSx10)
- Esqueleto
- .AbrirConexion
- Window
- Bg
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `Window` - 36 edges
3. `MainWindow` - 30 edges
4. `PepeWapOSx10.Shell.Widgets` - 27 edges
5. `VistaMes` - 25 edges
6. `VideoWallpaperWindow` - 24 edges
7. `ScheduledBlock` - 23 edges
8. `IconRailWindow` - 23 edges
9. `PanelGuia` - 21 edges
10. `VistaDia` - 21 edges

## Surprising Connections (you probably didn't know these)
- `SqliteFlexibleTaskRepository` --implements--> `IFlexibleTaskRepository`  [EXTRACTED]
  src/PepeWapOSx10.Datos/SqliteFlexibleTaskRepository.cs → src/PepeWapOSx10.Dominio/Interfaces/IFlexibleTaskRepository.cs
- `SqliteTareaGuiaRepository` --implements--> `ITareaGuiaRepository`  [EXTRACTED]
  src/PepeWapOSx10.Datos/SqliteTareaGuiaRepository.cs → src/PepeWapOSx10.Dominio/Interfaces/ITareaGuiaRepository.cs
- `DiaCargado` --references--> `ScheduledBlock`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/Agenda/VistaDia.cs → src/PepeWapOSx10.Dominio/Modelos/ScheduledBlock.cs
- `EditarTareaGuiaWindow` --references--> `Repeticion`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs
- `EditarTareaGuiaWindow` --references--> `TareaGuia`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs

## Import Cycles
- None detected.

## Communities (28 total, 3 thin omitted)

### Community 0 - "Window"
Cohesion: 0.05
Nodes (48): AgendaCanvas, AgendaHeaderGenerico, AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText (+40 more)

### Community 1 - "VistaDia"
Cohesion: 0.08
Nodes (27): DiaCargado, Ellipse, MouseWheelEventArgs, Rectangle, BlockKind, Action, bool, Canvas (+19 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.07
Nodes (30): Icono, Label, DragEventArgs, ImageSource, JsonDocument, JsonSerializerOptions, MouseEventArgs, ObservableCollection (+22 more)

### Community 3 - "VistaMes"
Cohesion: 0.07
Nodes (32): DateTime, ScheduledBlock, DateOnly, SemanaIso, DateOnly, IReadOnlyList, Task, IAgendaService (+24 more)

### Community 4 - "PanelGuia"
Cohesion: 0.11
Nodes (19): DateOnly, IReadOnlyList, Task, ITareaGuiaRepository, DateOnly, TareaGuia, Border, Button (+11 more)

### Community 5 - "AgendaService"
Cohesion: 0.10
Nodes (24): Dictionary, HttpClient, DateOnly, IReadOnlyList, string, Task, GoogleCalendarSource, DateOnly (+16 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.09
Nodes (13): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, Action, double (+5 more)

### Community 7 - "AnclajeEscritorio"
Cohesion: 0.10
Nodes (15): EnumWindowsProc, IDisposable, bool, DispatcherTimer, DllImport, int, IntPtr, List (+7 more)

### Community 8 - "EspejoWallpaperWindow"
Cohesion: 0.11
Nodes (19): byte, DWM_THUMBNAIL_PROPERTIES, MonitorEnumProc, MONITORINFO, SIZE, bool, DispatcherTimer, DllImport (+11 more)

### Community 9 - "VideoWallpaperWindow"
Cohesion: 0.08
Nodes (18): LibVLC, MediaPlayer, Random, Reproductor, Velo, Window, bool, DispatcherTimer (+10 more)

### Community 10 - "Window"
Cohesion: 0.08
Nodes (25): ActualWidth, IsDropDownOpen, Bg, CategoriaBox, ContentSite, ErrorText, PART_ContentHost, PART_EditableTextBox (+17 more)

### Community 11 - "MiniCalendario"
Cohesion: 0.12
Nodes (14): CultureInfo, Action, bool, Border, DateOnly, double, FrameworkElement, Grid (+6 more)

### Community 12 - "Correr Agenda Dinámica"
Cohesion: 0.25
Nodes (7): 1. Matar cualquier instancia previa, 2. Build, 3. Lanzar, 4. Verla / interactuar con ella (importante, no es un top-level normal), 5. Cerrar, Correr Agenda Dinámica, Notas / trampas conocidas

### Community 13 - "TaskbarWindow"
Cohesion: 0.10
Nodes (22): Stroke, AppsEnEjecucion, Bg, BotonBandeja, BotonExplorador, BotonMenuWindows, BotonModoJuego, IconoModoJuego (+14 more)

### Community 14 - ".Empaquetar"
Cohesion: 0.17
Nodes (16): End, Fin, Inicio, DayOfWeek, IReadOnlyList, TimeOnly, FlexibleTask, DateOnly (+8 more)

### Community 15 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 16 - "AccesosDirectos"
Cohesion: 0.14
Nodes (12): BitmapSource, HashSet, ShFileInfo, SpecialFolder, DllImport, IEnumerable, int, IntPtr (+4 more)

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "Esqueleto"
Cohesion: 0.23
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

### Community 20 - ".AbrirConexion"
Cohesion: 0.10
Nodes (15): SqliteCommand, SqliteConnection, string, AgendaDbContext, DateOnly, DayOfWeek, IReadOnlyList, IReadOnlySet (+7 more)

### Community 21 - "Window"
Cohesion: 0.12
Nodes (10): Application, PepeWapOSx10.App, Application, App, Window, MainWindow, Application, App (+2 more)

### Community 22 - "Bg"
Cohesion: 0.50
Nodes (3): Bg, ResourceDictionary, Border

## Knowledge Gaps
- **55 isolated node(s):** `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0`, `Ical.Net (5.2.3)`, `Microsoft.NET.Sdk` (+50 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MainWindow` connect `Window` to `VistaDia`, `VistaMes`, `PanelGuia`, `PepeWapOSx10.Shell.Widgets`, `AnclajeEscritorio`, `MiniCalendario`, `.AbrirConexion`, `Window`?**
  _High betweenness centrality (0.306) - this node is a cross-community bridge._
- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `VistaDia`, `IconRailWindow`, `VistaMes`, `AnclajeEscritorio`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `Window`, `MiniCalendario`, `TaskbarWindow`, `DetectorDobleClick`, `AccesosDirectos`, `Esqueleto`, `Window`?**
  _High betweenness centrality (0.235) - this node is a cross-community bridge._
- **Why does `EscritorioShell` connect `AnclajeEscritorio` to `Window`, `IconRailWindow`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `TaskbarWindow`?**
  _High betweenness centrality (0.139) - this node is a cross-community bridge._
- **What connects `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0` to the rest of the system?**
  _55 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Window` be split into smaller, more focused modules?**
  _Cohesion score 0.05336538461538461 - nodes in this community are weakly interconnected._
- **Should `VistaDia` be split into smaller, more focused modules?**
  _Cohesion score 0.07617051013277429 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.06558441558441558 - nodes in this community are weakly interconnected._