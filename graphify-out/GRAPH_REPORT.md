# Graph Report - AgendaDinamica  (2026-11-25)

## Corpus Check
- 81 files · ~35,193 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1003 nodes · 1968 edges · 52 communities (42 shown, 10 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 47 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c9dfaa7a`
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
- MenuInicioPopover
- PepeWapOSx10.Dominio.csproj
- Background Calendar (PepeWapOSx10)
- VistaDia
- TaskbarWindow
- OrdenDeRieles
- ResourceDictionary
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md
- RelojDigital
- LineaDeTiempo
- PopoverBase
- Window
- RoutedEventArgs
- DetectorDobleClick
- .Armar
- .TabVista_Click
- StackPanel
- DockPanel
- AccionesDeSistema
- VistaSemana
- DiaScroll
- AccesosDirectos
- Window
- IconRailItem
- .MigrarYCargar
- .Inicio
- TextBlock
- bool
- DateOnly
- MouseButtonEventArgs
- RoutedEventArgs
- Task

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `PepeWapOSx10.Shell.Widgets` - 38 edges
3. `Window` - 37 edges
4. `ProgramasAbiertos` - 36 edges
5. `MainWindow` - 30 edges
6. `BandejaDelSistema` - 27 edges
7. `TaskbarWindow` - 26 edges
8. `VistaMes` - 24 edges
9. `VideoWallpaperWindow` - 24 edges
10. `IconRailWindow` - 23 edges

## Surprising Connections (you probably didn't know these)
- `MainWindow` --references--> `RelojDigital`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/RelojDigital.cs
- `MainWindow` --inherits--> `Window`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml.cs → src/PepeWapOSx10.Shell.Widgets/MainWindow.xaml
- `MiniCalendario` --references--> `DetectorDobleClick`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/Agenda/MiniCalendario.cs → src/PepeWapOSx10.Shell.Widgets/Ui/Interaccion.cs
- `EditarTareaGuiaWindow` --references--> `Repeticion`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs
- `EditarTareaGuiaWindow` --references--> `TareaGuia`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs

## Import Cycles
- None detected.

## Communities (52 total, 10 thin omitted)

### Community 0 - "MainWindow"
Cohesion: 0.10
Nodes (17): bool, DateOnly, EscritorioShell, MiniCalendario, MouseButtonEventArgs, PanelGuia, RoutedEventArgs, DispatcherTimer (+9 more)

### Community 1 - "MiniCalendario"
Cohesion: 0.11
Nodes (14): CultureInfo, Action, bool, Border, DateOnly, double, FrameworkElement, Grid (+6 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.18
Nodes (9): DragEventArgs, MouseEventArgs, Point, IconosControl, bool, double, MouseButtonEventArgs, IconRailWindow (+1 more)

### Community 3 - "VistaMes"
Cohesion: 0.15
Nodes (15): DateTime, ScheduledBlock, Action, Border, DateOnly, double, FrameworkElement, Grid (+7 more)

### Community 4 - "PanelGuia"
Cohesion: 0.06
Nodes (34): SqliteCommand, SqliteConnection, string, AgendaDbContext, DateOnly, DayOfWeek, IReadOnlyList, IReadOnlySet (+26 more)

### Community 5 - ".Empaquetar"
Cohesion: 0.07
Nodes (40): End, Fin, HttpClient, Inicio, DateOnly, IReadOnlyList, string, Task (+32 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.08
Nodes (18): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, Border, Canvas (+10 more)

### Community 7 - "AnclajeEscritorio"
Cohesion: 0.11
Nodes (13): bool, DispatcherTimer, DllImport, EnumWindowsProc, int, IntPtr, List, StringBuilder (+5 more)

### Community 8 - "EspejoWallpaperWindow"
Cohesion: 0.11
Nodes (19): DWM_THUMBNAIL_PROPERTIES, MonitorEnumProc, MONITORINFO, SIZE, bool, byte, DispatcherTimer, DllImport (+11 more)

### Community 9 - "VideoWallpaperWindow"
Cohesion: 0.05
Nodes (25): Application, PepeWapOSx10.App, LibVLC, MediaPlayer, Random, Application, App, Window (+17 more)

### Community 10 - "Window"
Cohesion: 0.08
Nodes (25): ActualWidth, IsDropDownOpen, Bg, CategoriaBox, ContentSite, ErrorText, PART_ContentHost, PART_EditableTextBox (+17 more)

### Community 11 - "Window"
Cohesion: 0.15
Nodes (22): AgendaCanvas, AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText, DiaHeaderAnioText (+14 more)

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

### Community 16 - "MenuInicioPopover"
Cohesion: 0.07
Nodes (25): Boton, Application, App, DateTime, ImageSource, IReadOnlyList, string, Task (+17 more)

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "VistaDia"
Cohesion: 0.11
Nodes (18): MouseWheelEventArgs, DateOnly, IReadOnlyList, Task, IAgendaService, Action, bool, Canvas (+10 more)

### Community 20 - "TaskbarWindow"
Cohesion: 0.18
Nodes (8): bool, Dictionary, DispatcherTimer, FrameworkElement, string, TimeSpan, UIElement, TaskbarWindow

### Community 21 - "OrdenDeRieles"
Cohesion: 0.23
Nodes (8): JsonDocument, JsonSerializerOptions, Reparto, IReadOnlySet, List, string, OrdenDeRieles, Reparto

### Community 22 - "ResourceDictionary"
Cohesion: 0.33
Nodes (5): Bg, PART_Track, ResourceDictionary, Border, Track

### Community 28 - "RelojDigital"
Cohesion: 0.14
Nodes (7): PowerModeChangedEventArgs, DispatcherTimer, EventArgs, TimeSpan, RelojDigital, EventArgs, TextBlock

### Community 29 - "LineaDeTiempo"
Cohesion: 0.14
Nodes (13): DiaCargado, Ellipse, Rectangle, BlockKind, Border, Brush, Canvas, DateTime (+5 more)

### Community 30 - "PopoverBase"
Cohesion: 0.21
Nodes (7): object, bool, Border, double, FrameworkElement, Func, PopoverBase

### Community 31 - "Window"
Cohesion: 0.18
Nodes (10): Stroke, AppsEnEjecucion, Bg, IconoModoJuego, SeparadorDeApps, Window, Border, Rectangle (+2 more)

### Community 32 - "RoutedEventArgs"
Cohesion: 0.27
Nodes (6): BotonBandeja, BotonExplorador, BotonMenuWindows, BotonModoJuego, RoutedEventArgs, Button

### Community 33 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 34 - ".Armar"
Cohesion: 0.22
Nodes (3): UIntPtr, StackPanel, ListaVentanasPopover

### Community 35 - ".TabVista_Click"
Cohesion: 0.60
Nodes (4): TabDia, TabMes, TabSemana, ToggleButton

### Community 36 - "StackPanel"
Cohesion: 0.50
Nodes (4): AgendaHeaderGenerico, DiaHeaderStack, GuiaPanel, StackPanel

### Community 37 - "DockPanel"
Cohesion: 0.50
Nodes (4): DiaPanel, MesPanel, SemanaPanel, DockPanel

### Community 38 - "AccionesDeSistema"
Cohesion: 0.16
Nodes (6): byte, DllImport, int, IntPtr, uint, AccionesDeSistema

### Community 39 - "VistaSemana"
Cohesion: 0.19
Nodes (11): Action, Border, Canvas, DateOnly, double, Grid, IReadOnlyList, ScrollViewer (+3 more)

### Community 40 - "DiaScroll"
Cohesion: 0.67
Nodes (3): DiaScroll, SemanaScroll, ScrollViewer

### Community 41 - "AccesosDirectos"
Cohesion: 0.21
Nodes (10): BitmapSource, ShFileInfo, SpecialFolder, DllImport, int, IntPtr, string, uint (+2 more)

### Community 42 - "Window"
Cohesion: 0.21
Nodes (6): Icono, Label, Slot, Window, RoutedEventArgs, Border

### Community 43 - "IconRailItem"
Cohesion: 0.32
Nodes (6): ObservableCollection, ImageSource, IconRailItem, IEnumerable, LadoEscritorio, RielesDeIconos

### Community 44 - ".MigrarYCargar"
Cohesion: 0.29
Nodes (3): HashSet, IEnumerable, IEnumerable

## Knowledge Gaps
- **57 isolated node(s):** `Vista`, `Class1`, `Class1`, `ActualWidth`, `Border` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **10 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `MainWindow`, `MiniCalendario`, `IconRailWindow`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `Window`, `BarraDeWindows`, `BandejaDelSistema`, `ProgramasAbiertos`, `MenuInicioPopover`, `VistaDia`, `TaskbarWindow`, `OrdenDeRieles`, `RelojDigital`, `PopoverBase`, `DetectorDobleClick`, `.Armar`, `AccionesDeSistema`, `AccesosDirectos`, `IconRailItem`?**
  _High betweenness centrality (0.485) - this node is a cross-community bridge._
- **Why does `MainWindow` connect `MainWindow` to `.TabVista_Click`, `Window`, `RelojDigital`?**
  _High betweenness centrality (0.132) - this node is a cross-community bridge._
- **Why does `TaskbarWindow` connect `TaskbarWindow` to `RoutedEventArgs`, `.Armar`, `AccionesDeSistema`, `VideoWallpaperWindow`, `BarraDeWindows`, `ProgramasAbiertos`, `PopoverBase`, `Window`?**
  _High betweenness centrality (0.120) - this node is a cross-community bridge._
- **What connects `Vista`, `Class1`, `Class1` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MainWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.0967741935483871 - nodes in this community are weakly interconnected._
- **Should `MiniCalendario` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._
- **Should `PanelGuia` be split into smaller, more focused modules?**
  _Cohesion score 0.0554954954954955 - nodes in this community are weakly interconnected._