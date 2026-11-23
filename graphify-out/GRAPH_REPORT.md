# Graph Report - AgendaDinamica  (2026-11-23)

## Corpus Check
- 80 files · ~34,903 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 979 nodes · 1954 edges · 32 communities (29 shown, 3 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 46 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `78fc0d88`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Window
- MiniCalendario
- IconRailWindow
- VistaMes
- PanelGuia
- .Empaquetar
- PepeWapOSx10.Shell.Widgets
- AnclajeEscritorio
- EspejoWallpaperWindow
- VideoWallpaperWindow
- Window
- VistaSemana
- Correr Agenda Dinámica
- BarraDeWindows
- BandejaDelSistema
- ProgramasAbiertos
- MenuInicioPopover
- PepeWapOSx10.Dominio.csproj
- Background Calendar (PepeWapOSx10)
- Esqueleto
- ScheduledBlock
- OrdenDeRieles
- ResourceDictionary
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md
- .Posicionar
- VistaDia
- DetectorDobleClick
- TaskbarWindow

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `PepeWapOSx10.Shell.Widgets` - 37 edges
3. `Window` - 36 edges
4. `ProgramasAbiertos` - 36 edges
5. `MainWindow` - 30 edges
6. `BandejaDelSistema` - 27 edges
7. `TaskbarWindow` - 26 edges
8. `VistaMes` - 25 edges
9. `VideoWallpaperWindow` - 24 edges
10. `ScheduledBlock` - 23 edges

## Surprising Connections (you probably didn't know these)
- `SqliteFlexibleTaskRepository` --implements--> `IFlexibleTaskRepository`  [EXTRACTED]
  src/PepeWapOSx10.Datos/SqliteFlexibleTaskRepository.cs → src/PepeWapOSx10.Dominio/Interfaces/IFlexibleTaskRepository.cs
- `DiaCargado` --references--> `ScheduledBlock`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/Agenda/VistaDia.cs → src/PepeWapOSx10.Dominio/Modelos/ScheduledBlock.cs
- `EditarTareaGuiaWindow` --references--> `Repeticion`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs
- `EditarTareaGuiaWindow` --references--> `TareaGuia`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs
- `AgendaService` --implements--> `IAgendaService`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/Agenda/AgendaService.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/IAgendaService.cs

## Import Cycles
- None detected.

## Communities (32 total, 3 thin omitted)

### Community 0 - "Window"
Cohesion: 0.05
Nodes (48): AgendaCanvas, AgendaHeaderGenerico, AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText (+40 more)

### Community 1 - "MiniCalendario"
Cohesion: 0.09
Nodes (16): CultureInfo, DateOnly, SemanaIso, Action, bool, Border, DateOnly, double (+8 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.05
Nodes (34): Icono, Label, BitmapSource, DragEventArgs, HashSet, MouseEventArgs, ObservableCollection, Point (+26 more)

### Community 3 - "VistaMes"
Cohesion: 0.18
Nodes (12): Action, Border, DateOnly, double, FrameworkElement, Grid, int, IReadOnlyList (+4 more)

### Community 4 - "PanelGuia"
Cohesion: 0.06
Nodes (34): SqliteCommand, SqliteConnection, string, AgendaDbContext, DateOnly, DayOfWeek, IReadOnlyList, IReadOnlySet (+26 more)

### Community 5 - ".Empaquetar"
Cohesion: 0.07
Nodes (40): End, Fin, HttpClient, Inicio, DateOnly, IReadOnlyList, string, Task (+32 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.07
Nodes (19): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, object, Action (+11 more)

### Community 7 - "AnclajeEscritorio"
Cohesion: 0.11
Nodes (13): bool, DispatcherTimer, DllImport, EnumWindowsProc, int, IntPtr, List, StringBuilder (+5 more)

### Community 8 - "EspejoWallpaperWindow"
Cohesion: 0.11
Nodes (19): DWM_THUMBNAIL_PROPERTIES, MonitorEnumProc, MONITORINFO, SIZE, bool, byte, DispatcherTimer, DllImport (+11 more)

### Community 9 - "VideoWallpaperWindow"
Cohesion: 0.08
Nodes (18): LibVLC, MediaPlayer, Random, Reproductor, Velo, Window, bool, DispatcherTimer (+10 more)

### Community 10 - "Window"
Cohesion: 0.06
Nodes (32): Application, ActualWidth, IsDropDownOpen, PepeWapOSx10.App, Application, App, Window, MainWindow (+24 more)

### Community 11 - "VistaSemana"
Cohesion: 0.19
Nodes (11): Action, Border, Canvas, DateOnly, double, Grid, IReadOnlyList, ScrollViewer (+3 more)

### Community 12 - "Correr Agenda Dinámica"
Cohesion: 0.25
Nodes (7): 1. Matar cualquier instancia previa, 2. Build, 3. Lanzar, 4. Verla / interactuar con ella (importante, no es un top-level normal), 5. Cerrar, Correr Agenda Dinámica, Notas / trampas conocidas

### Community 13 - "BarraDeWindows"
Cohesion: 0.08
Nodes (25): APPBARDATA, Dispatcher, IDisposable, LowLevelKeyboardProc, bool, DispatcherTimer, DllImport, EventArgs (+17 more)

### Community 14 - "BandejaDelSistema"
Cohesion: 0.11
Nodes (22): Bottom, EnLaBarra, Left, Punto, Right, byte, DllImport, IEnumerable (+14 more)

### Community 15 - "ProgramasAbiertos"
Cohesion: 0.11
Nodes (19): Icono, long, Nombre, StackPanel, byte, Dictionary, DllImport, EnumWindowsProc (+11 more)

### Community 16 - "MenuInicioPopover"
Cohesion: 0.07
Nodes (25): Boton, Application, App, DateTime, ImageSource, IReadOnlyList, string, Task (+17 more)

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "Esqueleto"
Cohesion: 0.23
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

### Community 20 - "ScheduledBlock"
Cohesion: 0.20
Nodes (9): Ellipse, Rectangle, DateTime, BlockKind, ScheduledBlock, StackPanel, double, Grid (+1 more)

### Community 21 - "OrdenDeRieles"
Cohesion: 0.23
Nodes (8): JsonDocument, JsonSerializerOptions, Reparto, IReadOnlySet, List, string, OrdenDeRieles, Reparto

### Community 22 - "ResourceDictionary"
Cohesion: 0.33
Nodes (5): Bg, PART_Track, ResourceDictionary, Border, Track

### Community 28 - ".Posicionar"
Cohesion: 0.22
Nodes (7): DiaCargado, Border, Brush, Canvas, DateTime, UIElement, Thickness

### Community 29 - "VistaDia"
Cohesion: 0.12
Nodes (18): MouseWheelEventArgs, DateOnly, IReadOnlyList, Task, IAgendaService, Action, bool, Canvas (+10 more)

### Community 33 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 38 - "TaskbarWindow"
Cohesion: 0.05
Nodes (32): Stroke, byte, DllImport, int, IntPtr, uint, UIntPtr, AccionesDeSistema (+24 more)

## Knowledge Gaps
- **57 isolated node(s):** `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0`, `Ical.Net (5.2.3)`, `Microsoft.NET.Sdk` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `MiniCalendario`, `IconRailWindow`, `DetectorDobleClick`, `TaskbarWindow`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `BarraDeWindows`, `BandejaDelSistema`, `ProgramasAbiertos`, `MenuInicioPopover`, `Esqueleto`, `OrdenDeRieles`?**
  _High betweenness centrality (0.336) - this node is a cross-community bridge._
- **Why does `MainWindow` connect `Window` to `MiniCalendario`, `VistaMes`, `PanelGuia`, `PepeWapOSx10.Shell.Widgets`, `Window`, `VistaSemana`, `BarraDeWindows`, `VistaDia`?**
  _High betweenness centrality (0.249) - this node is a cross-community bridge._
- **Why does `EscritorioShell` connect `BarraDeWindows` to `Window`, `IconRailWindow`, `TaskbarWindow`, `AnclajeEscritorio`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`?**
  _High betweenness centrality (0.156) - this node is a cross-community bridge._
- **What connects `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Window` be split into smaller, more focused modules?**
  _Cohesion score 0.053613053613053616 - nodes in this community are weakly interconnected._
- **Should `MiniCalendario` be split into smaller, more focused modules?**
  _Cohesion score 0.09475806451612903 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.054274084124830396 - nodes in this community are weakly interconnected._