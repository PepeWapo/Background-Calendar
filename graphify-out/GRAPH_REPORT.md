# Graph Report - AgendaDinamica  (2026-11-25)

## Corpus Check
- 81 files · ~35,193 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 991 nodes · 1970 edges · 41 communities (38 shown, 3 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 47 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9b972e16`
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
- Esqueleto
- TaskbarWindow
- OrdenDeRieles
- ResourceDictionary
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md
- RelojDigital
- ScheduledBlock
- PopoverBase
- Window
- RoutedEventArgs
- DetectorDobleClick
- .Armar
- .TabVista_Click
- StackPanel
- DockPanel
- AccionesDeSistema
- AgendaCanvas
- DiaScroll

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `PepeWapOSx10.Shell.Widgets` - 38 edges
3. `Window` - 37 edges
4. `ProgramasAbiertos` - 36 edges
5. `MainWindow` - 31 edges
6. `BandejaDelSistema` - 27 edges
7. `TaskbarWindow` - 26 edges
8. `VistaMes` - 25 edges
9. `VideoWallpaperWindow` - 24 edges
10. `ScheduledBlock` - 23 edges

## Surprising Connections (you probably didn't know these)
- `SqliteFlexibleTaskRepository` --implements--> `IFlexibleTaskRepository`  [EXTRACTED]
  src/PepeWapOSx10.Datos/SqliteFlexibleTaskRepository.cs → src/PepeWapOSx10.Dominio/Interfaces/IFlexibleTaskRepository.cs
- `SqliteTareaGuiaRepository` --implements--> `ITareaGuiaRepository`  [EXTRACTED]
  src/PepeWapOSx10.Datos/SqliteTareaGuiaRepository.cs → src/PepeWapOSx10.Dominio/Interfaces/ITareaGuiaRepository.cs
- `EditarTareaGuiaWindow` --references--> `Repeticion`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs
- `EditarTareaGuiaWindow` --references--> `TareaGuia`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/EditarTareaGuiaWindow.xaml.cs → src/PepeWapOSx10.Dominio/Modelos/TareaGuia.cs
- `AgendaService` --implements--> `IAgendaService`  [EXTRACTED]
  src/PepeWapOSx10.Shell.Widgets/Agenda/AgendaService.cs → src/PepeWapOSx10.Shell.Widgets/Agenda/IAgendaService.cs

## Import Cycles
- None detected.

## Communities (41 total, 3 thin omitted)

### Community 0 - "MainWindow"
Cohesion: 0.14
Nodes (11): bool, DateOnly, DispatcherTimer, MouseButtonEventArgs, RoutedEventArgs, Task, TimeSpan, MainWindow (+3 more)

### Community 1 - "MiniCalendario"
Cohesion: 0.12
Nodes (14): CultureInfo, Action, bool, Border, DateOnly, double, FrameworkElement, Grid (+6 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.06
Nodes (34): Icono, Label, BitmapSource, DragEventArgs, HashSet, MouseEventArgs, ObservableCollection, Point (+26 more)

### Community 3 - "VistaMes"
Cohesion: 0.14
Nodes (14): DateOnly, SemanaIso, Action, Border, DateOnly, double, FrameworkElement, Grid (+6 more)

### Community 4 - "PanelGuia"
Cohesion: 0.11
Nodes (19): DateOnly, IReadOnlyList, Task, ITareaGuiaRepository, DateOnly, TareaGuia, Border, Button (+11 more)

### Community 5 - ".Empaquetar"
Cohesion: 0.07
Nodes (40): End, Fin, HttpClient, Inicio, DateOnly, IReadOnlyList, string, Task (+32 more)

### Community 6 - "PepeWapOSx10.Shell.Widgets"
Cohesion: 0.06
Nodes (25): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, SqliteCommand, SqliteConnection (+17 more)

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
Cohesion: 0.17
Nodes (19): AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText, DiaHeaderAnioText, DiaHeaderDiaText (+11 more)

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

### Community 19 - "Esqueleto"
Cohesion: 0.23
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

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
Cohesion: 0.15
Nodes (7): PowerModeChangedEventArgs, DispatcherTimer, EventArgs, TextBlock, TimeSpan, RelojDigital, EventArgs

### Community 29 - "ScheduledBlock"
Cohesion: 0.05
Nodes (45): DiaCargado, Ellipse, MouseWheelEventArgs, Rectangle, DateTime, BlockKind, ScheduledBlock, DateOnly (+37 more)

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

### Community 39 - "AgendaCanvas"
Cohesion: 0.67
Nodes (3): AgendaCanvas, SemanaCanvas, Canvas

### Community 40 - "DiaScroll"
Cohesion: 0.67
Nodes (3): DiaScroll, SemanaScroll, ScrollViewer

## Knowledge Gaps
- **57 isolated node(s):** `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0`, `Ical.Net (5.2.3)`, `Microsoft.NET.Sdk` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `MiniCalendario`, `IconRailWindow`, `.Armar`, `DetectorDobleClick`, `AccionesDeSistema`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `Window`, `BarraDeWindows`, `BandejaDelSistema`, `ProgramasAbiertos`, `MenuInicioPopover`, `Esqueleto`, `TaskbarWindow`, `OrdenDeRieles`, `RelojDigital`, `ScheduledBlock`, `PopoverBase`?**
  _High betweenness centrality (0.334) - this node is a cross-community bridge._
- **Why does `MainWindow` connect `MainWindow` to `MiniCalendario`, `VistaMes`, `PanelGuia`, `.TabVista_Click`, `PepeWapOSx10.Shell.Widgets`, `VideoWallpaperWindow`, `Window`, `BarraDeWindows`, `RelojDigital`, `ScheduledBlock`?**
  _High betweenness centrality (0.256) - this node is a cross-community bridge._
- **Why does `EscritorioShell` connect `BarraDeWindows` to `MainWindow`, `IconRailWindow`, `AnclajeEscritorio`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `TaskbarWindow`?**
  _High betweenness centrality (0.154) - this node is a cross-community bridge._
- **What connects `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MainWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.13666666666666666 - nodes in this community are weakly interconnected._
- **Should `MiniCalendario` be split into smaller, more focused modules?**
  _Cohesion score 0.1168091168091168 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.05547785547785548 - nodes in this community are weakly interconnected._