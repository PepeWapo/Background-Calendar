# Graph Report - AgendaDinamica  (2026-11-25)

## Corpus Check
- 81 files · ~35,868 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1043 nodes · 2033 edges · 66 communities (42 shown, 24 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 47 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d5311032`
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
- TeclaWindows
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
- BarraDeWindows
- LineaDeTiempo
- PopoverBase
- Esqueleto
- ScheduledBlock
- DetectorDobleClick
- IntPtr
- .TabVista_Click
- TaskbarWindow
- DockPanel
- AccionesDeSistema
- VistaSemana
- MenuInicioPopover
- List
- RoutedEventArgs
- Paleta
- Window
- .Inicio
- .GetImage
- .BotonVistaDeTareas_Click
- DiaScroll
- BitmapSource
- DateTime
- DllImport
- Func
- ImageSource
- IntPtr
- IReadOnlyList
- string
- Task
- TimeSpan
- uint
- Action
- Button
- ScrollViewer
- StackPanel
- TextBlock
- UIElement

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `PepeWapOSx10.Shell.Widgets` - 38 edges
3. `Window` - 37 edges
4. `ProgramasAbiertos` - 36 edges
5. `MainWindow` - 31 edges
6. `BandejaDelSistema` - 27 edges
7. `TaskbarWindow` - 26 edges
8. `AppsDeInicio` - 25 edges
9. `VistaMes` - 25 edges
10. `VideoWallpaperWindow` - 24 edges

## Surprising Connections (you probably didn't know these)
- `Window` --references--> `MainWindow`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml → src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs
- `MainWindow` --references--> `MiniCalendario`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/MiniCalendario.cs
- `MainWindow` --references--> `PanelGuia`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/PanelGuia.cs
- `MainWindow` --references--> `RelojDigital`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/RelojDigital.cs
- `MainWindow` --references--> `VistaDia`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/VistaDia.cs

## Import Cycles
- None detected.

## Communities (66 total, 24 thin omitted)

### Community 0 - "MainWindow"
Cohesion: 0.14
Nodes (11): bool, DateOnly, DispatcherTimer, MouseButtonEventArgs, RoutedEventArgs, Task, TimeSpan, MainWindow (+3 more)

### Community 1 - "MiniCalendario"
Cohesion: 0.11
Nodes (14): CultureInfo, Action, bool, Border, DateOnly, double, FrameworkElement, Grid (+6 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.06
Nodes (34): Icono, Label, DragEventArgs, HashSet, MouseEventArgs, ObservableCollection, Point, ShFileInfo (+26 more)

### Community 3 - "VistaMes"
Cohesion: 0.18
Nodes (12): Action, Border, DateOnly, double, FrameworkElement, Grid, int, IReadOnlyList (+4 more)

### Community 4 - "PanelGuia"
Cohesion: 0.11
Nodes (19): DateOnly, IReadOnlyList, Task, ITareaGuiaRepository, DateOnly, TareaGuia, Border, Button (+11 more)

### Community 5 - ".Empaquetar"
Cohesion: 0.07
Nodes (40): End, Fin, HttpClient, Inicio, DateOnly, IReadOnlyList, string, Task (+32 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.05
Nodes (25): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, SqliteCommand, SqliteConnection (+17 more)

### Community 7 - "AnclajeEscritorio"
Cohesion: 0.11
Nodes (13): bool, DispatcherTimer, DllImport, EnumWindowsProc, int, IntPtr, List, StringBuilder (+5 more)

### Community 8 - "EspejoWallpaperWindow"
Cohesion: 0.11
Nodes (19): DWM_THUMBNAIL_PROPERTIES, MonitorEnumProc, MONITORINFO, SIZE, bool, byte, DispatcherTimer, DllImport (+11 more)

### Community 9 - "VideoWallpaperWindow"
Cohesion: 0.05
Nodes (28): Application, PepeWapOSx10.App, LibVLC, MediaPlayer, Random, Application, App, Window (+20 more)

### Community 10 - "Window"
Cohesion: 0.08
Nodes (25): ActualWidth, IsDropDownOpen, Bg, CategoriaBox, ContentSite, ErrorText, PART_ContentHost, PART_EditableTextBox (+17 more)

### Community 11 - "Window"
Cohesion: 0.12
Nodes (26): AgendaCanvas, AgendaHeaderGenerico, AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText (+18 more)

### Community 12 - "Correr Agenda Dinámica"
Cohesion: 0.25
Nodes (7): 1. Matar cualquier instancia previa, 2. Build, 3. Lanzar, 4. Verla / interactuar con ella (importante, no es un top-level normal), 5. Cerrar, Correr Agenda Dinámica, Notas / trampas conocidas

### Community 13 - "TeclaWindows"
Cohesion: 0.08
Nodes (21): Dispatcher, IDisposable, LowLevelKeyboardProc, PowerModeChangedEventArgs, DispatcherTimer, EventArgs, TextBlock, TimeSpan (+13 more)

### Community 14 - "BandejaDelSistema"
Cohesion: 0.10
Nodes (24): Bottom, EnLaBarra, Left, Punto, Right, byte, DllImport, IEnumerable (+16 more)

### Community 15 - "ProgramasAbiertos"
Cohesion: 0.11
Nodes (19): Icono, long, Nombre, StackPanel, byte, Dictionary, DllImport, EnumWindowsProc (+11 more)

### Community 16 - "AppDeInicio"
Cohesion: 0.21
Nodes (5): Action, Button, ImageSource, AppDeInicio, Style

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "VistaDia"
Cohesion: 0.11
Nodes (18): MouseWheelEventArgs, DateOnly, IReadOnlyList, Task, IAgendaService, Action, bool, Canvas (+10 more)

### Community 20 - "AppsDeInicio"
Cohesion: 0.15
Nodes (15): DateTime, Func, GeneratedRegex, IReadOnlyList, Regex, int, AppsDeInicio, CabeceraDeMapa (+7 more)

### Community 21 - "OrdenDeRieles"
Cohesion: 0.23
Nodes (8): JsonDocument, JsonSerializerOptions, Reparto, IReadOnlySet, List, string, OrdenDeRieles, Reparto

### Community 22 - "ResourceDictionary"
Cohesion: 0.33
Nodes (5): Bg, PART_Track, ResourceDictionary, Border, Track

### Community 28 - "BarraDeWindows"
Cohesion: 0.19
Nodes (11): APPBARDATA, bool, DispatcherTimer, DllImport, EventArgs, int, IntPtr, string (+3 more)

### Community 29 - "LineaDeTiempo"
Cohesion: 0.18
Nodes (10): DiaCargado, Rectangle, Border, Brush, Canvas, DateTime, double, UIElement (+2 more)

### Community 30 - "PopoverBase"
Cohesion: 0.22
Nodes (7): object, ListaVentanasPopover, bool, Border, double, FrameworkElement, PopoverBase

### Community 31 - "Esqueleto"
Cohesion: 0.25
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

### Community 32 - "ScheduledBlock"
Cohesion: 0.24
Nodes (6): Ellipse, DateTime, BlockKind, ScheduledBlock, StackPanel, Grid

### Community 33 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 34 - "IntPtr"
Cohesion: 0.25
Nodes (7): BitmapSource, CabeceraDeMapa, DllImport, Guid, IntPtr, IShellItemImageFactory, MapaDeBits

### Community 35 - ".TabVista_Click"
Cohesion: 0.60
Nodes (4): TabDia, TabMes, TabSemana, ToggleButton

### Community 36 - "TaskbarWindow"
Cohesion: 0.18
Nodes (8): bool, Dictionary, DispatcherTimer, FrameworkElement, string, TimeSpan, UIElement, TaskbarWindow

### Community 37 - "DockPanel"
Cohesion: 0.50
Nodes (4): DiaPanel, MesPanel, SemanaPanel, DockPanel

### Community 38 - "AccionesDeSistema"
Cohesion: 0.16
Nodes (6): byte, DllImport, int, IntPtr, uint, AccionesDeSistema

### Community 39 - "VistaSemana"
Cohesion: 0.20
Nodes (11): Action, Border, Canvas, DateOnly, double, Grid, IReadOnlyList, ScrollViewer (+3 more)

### Community 40 - "MenuInicioPopover"
Cohesion: 0.17
Nodes (11): App, Boton, List, PopoverBase, ScrollViewer, int, MenuInicioPopover, StackPanel (+3 more)

### Community 42 - "RoutedEventArgs"
Cohesion: 0.21
Nodes (7): Func, BotonBandeja, BotonExplorador, BotonMenuWindows, BotonModoJuego, RoutedEventArgs, Button

### Community 44 - "Window"
Cohesion: 0.18
Nodes (10): Stroke, AppsEnEjecucion, Bg, IconoModoJuego, SeparadorDeApps, Window, Border, Rectangle (+2 more)

### Community 46 - ".GetImage"
Cohesion: 0.50
Nodes (3): PreserveSig, IShellItemImageFactory, Tamano

### Community 48 - "DiaScroll"
Cohesion: 0.67
Nodes (3): DiaScroll, SemanaScroll, ScrollViewer

## Knowledge Gaps
- **57 isolated node(s):** `Vista`, `Class1`, `Class1`, `ActualWidth`, `Border` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `MiniCalendario`, `IconRailWindow`, `DetectorDobleClick`, `TaskbarWindow`, `AccionesDeSistema`, `MenuInicioPopover`, `VideoWallpaperWindow`, `Window`, `Paleta`, `EspejoWallpaperWindow`, `TeclaWindows`, `BandejaDelSistema`, `ProgramasAbiertos`, `AppDeInicio`, `VistaDia`, `OrdenDeRieles`, `PopoverBase`?**
  _High betweenness centrality (0.383) - this node is a cross-community bridge._
- **Why does `MainWindow` connect `MainWindow` to `MiniCalendario`, `VistaMes`, `PanelGuia`, `.TabVista_Click`, `PepeWapOSx10.Shell.Widgets`, `VistaSemana`, `VideoWallpaperWindow`, `Window`, `TeclaWindows`, `VistaDia`?**
  _High betweenness centrality (0.204) - this node is a cross-community bridge._
- **Why does `EscritorioShell` connect `TeclaWindows` to `MainWindow`, `IconRailWindow`, `TaskbarWindow`, `AnclajeEscritorio`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `BarraDeWindows`?**
  _High betweenness centrality (0.160) - this node is a cross-community bridge._
- **What connects `Vista`, `Class1`, `Class1` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MainWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.13666666666666666 - nodes in this community are weakly interconnected._
- **Should `MiniCalendario` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.05547785547785548 - nodes in this community are weakly interconnected._