# Graph Report - AgendaDinamica  (2026-11-25)

## Corpus Check
- 81 files · ~35,868 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1023 nodes · 2033 edges · 45 communities (40 shown, 5 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 47 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `cec038a2`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MainWindow
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
- Window
- Correr Agenda Dinámica
- BarraDeWindows
- BandejaDelSistema
- ProgramasAbiertos
- AppDeInicio
- PepeWapOSx10.Dominio.csproj
- Background Calendar (PepeWapOSx10)
- VistaDia
- AppsDeInicio
- OrdenDeRieles
- ResourceDictionary
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md
- RelojDigital
- LineaDeTiempo
- PopoverBase
- Esqueleto
- ScheduledBlock
- DetectorDobleClick
- Window
- .TabVista_Click
- StackPanel
- DockPanel
- TaskbarWindow
- VistaSemana
- MenuInicioPopover
- .Visibles
- App
- Paleta
- AgendaCanvas

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `PepeWapOSx10.Shell.Widgets` - 38 edges
3. `Window` - 37 edges
4. `ProgramasAbiertos` - 36 edges
5. `MainWindow` - 31 edges
6. `BandejaDelSistema` - 27 edges
7. `TaskbarWindow` - 26 edges
8. `VistaMes` - 25 edges
9. `AppsDeInicio` - 25 edges
10. `VideoWallpaperWindow` - 24 edges

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

## Communities (45 total, 5 thin omitted)

### Community 0 - "MainWindow"
Cohesion: 0.14
Nodes (11): bool, DateOnly, DispatcherTimer, MouseButtonEventArgs, RoutedEventArgs, Task, TimeSpan, MainWindow (+3 more)

### Community 1 - "MiniCalendario"
Cohesion: 0.10
Nodes (16): CultureInfo, DateOnly, SemanaIso, Action, bool, Border, DateOnly, double (+8 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.05
Nodes (34): Icono, Label, DragEventArgs, HashSet, MouseEventArgs, ObservableCollection, Point, ShFileInfo (+26 more)

### Community 3 - "VistaMes"
Cohesion: 0.18
Nodes (12): Action, Border, DateOnly, double, FrameworkElement, Grid, int, IReadOnlyList (+4 more)

### Community 4 - "PanelGuia"
Cohesion: 0.05
Nodes (34): SqliteCommand, SqliteConnection, string, AgendaDbContext, DateOnly, DayOfWeek, IReadOnlyList, IReadOnlySet (+26 more)

### Community 5 - ".Empaquetar"
Cohesion: 0.07
Nodes (40): End, Fin, HttpClient, Inicio, DateOnly, IReadOnlyList, string, Task (+32 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.12
Nodes (10): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, Action, double (+2 more)

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
Cohesion: 0.09
Nodes (25): ActualWidth, IsDropDownOpen, Bg, CategoriaBox, ContentSite, ErrorText, PART_ContentHost, PART_EditableTextBox (+17 more)

### Community 11 - "Window"
Cohesion: 0.15
Nodes (22): AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText, DiaHeaderAnioText, DiaHeaderDiaText (+14 more)

### Community 12 - "Correr Agenda Dinámica"
Cohesion: 0.25
Nodes (7): 1. Matar cualquier instancia previa, 2. Build, 3. Lanzar, 4. Verla / interactuar con ella (importante, no es un top-level normal), 5. Cerrar, Correr Agenda Dinámica, Notas / trampas conocidas

### Community 13 - "BarraDeWindows"
Cohesion: 0.08
Nodes (25): APPBARDATA, Dispatcher, IDisposable, LowLevelKeyboardProc, bool, DispatcherTimer, DllImport, EventArgs (+17 more)

### Community 14 - "BandejaDelSistema"
Cohesion: 0.10
Nodes (24): Bottom, EnLaBarra, Left, Punto, Right, byte, DllImport, IEnumerable (+16 more)

### Community 15 - "ProgramasAbiertos"
Cohesion: 0.12
Nodes (18): Icono, long, Nombre, byte, Dictionary, DllImport, EnumWindowsProc, ImageSource (+10 more)

### Community 16 - "AppDeInicio"
Cohesion: 0.21
Nodes (5): ImageSource, AppDeInicio, Action, Button, Style

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "VistaDia"
Cohesion: 0.12
Nodes (18): MouseWheelEventArgs, DateOnly, IReadOnlyList, Task, IAgendaService, Action, bool, Canvas (+10 more)

### Community 20 - "AppsDeInicio"
Cohesion: 0.09
Nodes (25): CabeceraDeMapa, GeneratedRegex, Guid, IShellItemImageFactory, MapaDeBits, PreserveSig, Regex, BitmapSource (+17 more)

### Community 21 - "OrdenDeRieles"
Cohesion: 0.23
Nodes (8): JsonDocument, JsonSerializerOptions, Reparto, IReadOnlySet, List, string, OrdenDeRieles, Reparto

### Community 22 - "ResourceDictionary"
Cohesion: 0.33
Nodes (5): Bg, PART_Track, ResourceDictionary, Border, Track

### Community 28 - "RelojDigital"
Cohesion: 0.15
Nodes (7): PowerModeChangedEventArgs, DispatcherTimer, EventArgs, TextBlock, TimeSpan, RelojDigital, EventArgs

### Community 29 - "LineaDeTiempo"
Cohesion: 0.20
Nodes (10): DiaCargado, Rectangle, Border, Brush, Canvas, DateTime, double, UIElement (+2 more)

### Community 30 - "PopoverBase"
Cohesion: 0.18
Nodes (8): object, StackPanel, ListaVentanasPopover, bool, Border, double, FrameworkElement, PopoverBase

### Community 31 - "Esqueleto"
Cohesion: 0.21
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

### Community 32 - "ScheduledBlock"
Cohesion: 0.22
Nodes (6): Ellipse, DateTime, BlockKind, ScheduledBlock, StackPanel, Grid

### Community 33 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 34 - "Window"
Cohesion: 0.18
Nodes (7): Application, PepeWapOSx10.App, Application, App, Window, MainWindow, Window

### Community 35 - ".TabVista_Click"
Cohesion: 0.60
Nodes (4): TabDia, TabMes, TabSemana, ToggleButton

### Community 36 - "StackPanel"
Cohesion: 0.50
Nodes (4): AgendaHeaderGenerico, DiaHeaderStack, GuiaPanel, StackPanel

### Community 37 - "DockPanel"
Cohesion: 0.50
Nodes (4): DiaPanel, MesPanel, SemanaPanel, DockPanel

### Community 38 - "TaskbarWindow"
Cohesion: 0.06
Nodes (32): Stroke, byte, DllImport, int, IntPtr, uint, UIntPtr, AccionesDeSistema (+24 more)

### Community 39 - "VistaSemana"
Cohesion: 0.20
Nodes (11): Action, Border, Canvas, DateOnly, double, Grid, IReadOnlyList, ScrollViewer (+3 more)

### Community 40 - "MenuInicioPopover"
Cohesion: 0.22
Nodes (7): int, ScrollViewer, StackPanel, TextBlock, UIElement, MenuInicioPopover, TextBox

### Community 42 - "App"
Cohesion: 0.33
Nodes (3): Application, App, StartupEventArgs

### Community 44 - "AgendaCanvas"
Cohesion: 0.67
Nodes (3): AgendaCanvas, SemanaCanvas, Canvas

## Knowledge Gaps
- **57 isolated node(s):** `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0`, `Ical.Net (5.2.3)`, `Microsoft.NET.Sdk` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **5 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `MiniCalendario`, `IconRailWindow`, `DetectorDobleClick`, `TaskbarWindow`, `MenuInicioPopover`, `EspejoWallpaperWindow`, `App`, `Paleta`, `VideoWallpaperWindow`, `BarraDeWindows`, `BandejaDelSistema`, `ProgramasAbiertos`, `AppDeInicio`, `OrdenDeRieles`, `RelojDigital`, `PopoverBase`, `Esqueleto`?**
  _High betweenness centrality (0.369) - this node is a cross-community bridge._
- **Why does `MainWindow` connect `MainWindow` to `MiniCalendario`, `Window`, `VistaMes`, `PanelGuia`, `.TabVista_Click`, `PepeWapOSx10.Shell.Widgets`, `VistaSemana`, `Window`, `BarraDeWindows`, `VistaDia`, `RelojDigital`?**
  _High betweenness centrality (0.217) - this node is a cross-community bridge._
- **Why does `EscritorioShell` connect `BarraDeWindows` to `MainWindow`, `IconRailWindow`, `TaskbarWindow`, `AnclajeEscritorio`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`?**
  _High betweenness centrality (0.142) - this node is a cross-community bridge._
- **What connects `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MainWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.14492753623188406 - nodes in this community are weakly interconnected._
- **Should `MiniCalendario` be split into smaller, more focused modules?**
  _Cohesion score 0.0967741935483871 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.054274084124830396 - nodes in this community are weakly interconnected._