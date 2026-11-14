# Graph Report - AgendaDinamica  (2026-11-14)

## Corpus Check
- 69 files · ~26,896 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 712 nodes · 1356 edges · 27 communities (24 shown, 3 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 36 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e9d03bd1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Window
- VistaDia
- IconRailWindow
- VistaMes
- PanelGuia
- .Empaquetar
- PepeWapOSx10.Shell.Widgets
- AnclajeEscritorio
- Paleta
- VideoWallpaperWindow
- Window
- MiniCalendario
- Correr Agenda Dinámica
- TaskbarWindow
- Window
- DetectorDobleClick
- AccesosDirectos
- PepeWapOSx10.Dominio.csproj
- Background Calendar (PepeWapOSx10)
- Esqueleto
- Bg
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md
- FormatoEspanol

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 47 edges
2. `Window` - 36 edges
3. `MainWindow` - 30 edges
4. `PepeWapOSx10.Shell.Widgets` - 26 edges
5. `VistaMes` - 25 edges
6. `ScheduledBlock` - 23 edges
7. `IconRailWindow` - 23 edges
8. `VideoWallpaperWindow` - 22 edges
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

## Communities (27 total, 3 thin omitted)

### Community 0 - "Window"
Cohesion: 0.05
Nodes (48): AgendaCanvas, AgendaHeaderGenerico, AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText (+40 more)

### Community 1 - "VistaDia"
Cohesion: 0.05
Nodes (42): DiaCargado, Ellipse, MouseWheelEventArgs, Rectangle, BlockKind, DateOnly, IReadOnlyList, Task (+34 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.09
Nodes (21): Icono, Label, DragEventArgs, ImageSource, MouseEventArgs, ObservableCollection, Point, IconRailItem (+13 more)

### Community 3 - "VistaMes"
Cohesion: 0.15
Nodes (15): DateTime, ScheduledBlock, Action, Border, DateOnly, double, FrameworkElement, Grid (+7 more)

### Community 4 - "PanelGuia"
Cohesion: 0.11
Nodes (19): DateOnly, IReadOnlyList, Task, ITareaGuiaRepository, DateOnly, TareaGuia, Border, Button (+11 more)

### Community 5 - ".Empaquetar"
Cohesion: 0.07
Nodes (40): Dictionary, End, Fin, HttpClient, Inicio, DateOnly, IReadOnlyList, string (+32 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.07
Nodes (21): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, SqliteCommand, SqliteConnection (+13 more)

### Community 7 - "AnclajeEscritorio"
Cohesion: 0.10
Nodes (15): EnumWindowsProc, IDisposable, bool, DispatcherTimer, DllImport, int, IntPtr, List (+7 more)

### Community 8 - "Paleta"
Cohesion: 0.17
Nodes (7): Action, double, Window, EspacioEscritorio, Brush, Paleta, Style

### Community 9 - "VideoWallpaperWindow"
Cohesion: 0.08
Nodes (18): LibVLC, MediaPlayer, Random, Reproductor, Velo, Window, bool, DispatcherTimer (+10 more)

### Community 10 - "Window"
Cohesion: 0.08
Nodes (25): ActualWidth, IsDropDownOpen, Bg, CategoriaBox, ContentSite, ErrorText, PART_ContentHost, PART_EditableTextBox (+17 more)

### Community 11 - "MiniCalendario"
Cohesion: 0.15
Nodes (11): DateOnly, SemanaIso, Action, bool, Border, DateOnly, double, FrameworkElement (+3 more)

### Community 12 - "Correr Agenda Dinámica"
Cohesion: 0.25
Nodes (7): 1. Matar cualquier instancia previa, 2. Build, 3. Lanzar, 4. Verla / interactuar con ella (importante, no es un top-level normal), 5. Cerrar, Correr Agenda Dinámica, Notas / trampas conocidas

### Community 13 - "TaskbarWindow"
Cohesion: 0.12
Nodes (17): AppsEnEjecucion, Bg, BotonBandeja, BotonExplorador, BotonMenuWindows, Window, DllImport, double (+9 more)

### Community 14 - "Window"
Cohesion: 0.12
Nodes (10): Application, PepeWapOSx10.App, Application, App, Window, MainWindow, Application, App (+2 more)

### Community 15 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 16 - "AccesosDirectos"
Cohesion: 0.08
Nodes (21): BitmapSource, HashSet, JsonDocument, JsonSerializerOptions, Reparto, ShFileInfo, SpecialFolder, DllImport (+13 more)

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "Esqueleto"
Cohesion: 0.23
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

### Community 22 - "Bg"
Cohesion: 0.50
Nodes (3): Bg, ResourceDictionary, Border

### Community 28 - "FormatoEspanol"
Cohesion: 0.26
Nodes (5): CultureInfo, DateOnly, DayOfWeek, string, FormatoEspanol

## Knowledge Gaps
- **53 isolated node(s):** `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0`, `Ical.Net (5.2.3)`, `Microsoft.NET.Sdk` (+48 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MainWindow` connect `Window` to `VistaDia`, `VistaMes`, `PanelGuia`, `PepeWapOSx10.Shell.Widgets`, `AnclajeEscritorio`, `MiniCalendario`, `Window`?**
  _High betweenness centrality (0.305) - this node is a cross-community bridge._
- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `VistaDia`, `IconRailWindow`, `AnclajeEscritorio`, `Paleta`, `VideoWallpaperWindow`, `Window`, `TaskbarWindow`, `Window`, `DetectorDobleClick`, `AccesosDirectos`, `Esqueleto`, `FormatoEspanol`?**
  _High betweenness centrality (0.232) - this node is a cross-community bridge._
- **Why does `EscritorioShell` connect `AnclajeEscritorio` to `Window`, `VideoWallpaperWindow`, `IconRailWindow`, `TaskbarWindow`?**
  _High betweenness centrality (0.133) - this node is a cross-community bridge._
- **What connects `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0` to the rest of the system?**
  _53 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Window` be split into smaller, more focused modules?**
  _Cohesion score 0.05336538461538461 - nodes in this community are weakly interconnected._
- **Should `VistaDia` be split into smaller, more focused modules?**
  _Cohesion score 0.05126582278481013 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.08668076109936575 - nodes in this community are weakly interconnected._