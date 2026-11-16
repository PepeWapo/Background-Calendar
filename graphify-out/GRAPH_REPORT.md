# Graph Report - AgendaDinamica  (2026-11-15)

## Corpus Check
- 78 files · ~33,558 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 924 nodes · 1830 edges · 39 communities (35 shown, 4 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 44 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `6826ddc6`
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
- EspejoWallpaperWindow
- VideoWallpaperWindow
- Window
- MiniCalendario
- Correr Agenda Dinámica
- Window
- BandejaDelSistema
- ProgramasAbiertos
- MenuInicioPopover
- PepeWapOSx10.Dominio.csproj
- Background Calendar (PepeWapOSx10)
- Esqueleto
- MainWindow
- OrdenDeRieles
- ResourceDictionary
- PepeWapOSx10.Shell.Taskbar/Class1.cs
- PepeWapOSx10.Shell.Iconos/Class1.cs
- CLAUDE.md
- .TabVista_Click
- DockPanel
- .AbrirConexion
- VistaSemana
- ScheduledBlock
- DetectorDobleClick
- .Posicionar
- PopoverBase
- TaskbarWindow
- AccionesDeSistema
- RoutedEventArgs

## God Nodes (most connected - your core abstractions)
1. `AnclajeEscritorio` - 49 edges
2. `Window` - 36 edges
3. `ProgramasAbiertos` - 36 edges
4. `PepeWapOSx10.Shell.Widgets` - 35 edges
5. `MainWindow` - 30 edges
6. `BandejaDelSistema` - 27 edges
7. `VistaMes` - 25 edges
8. `TaskbarWindow` - 25 edges
9. `VideoWallpaperWindow` - 24 edges
10. `ScheduledBlock` - 23 edges

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

## Communities (39 total, 4 thin omitted)

### Community 0 - "Window"
Cohesion: 0.11
Nodes (28): AgendaCanvas, AgendaHeaderGenerico, AgendaNavRow, AgendaSubtitleText, CalendarDaysGrid, CalendarMonthText, CalendarYearText, DiaFooterText (+20 more)

### Community 1 - "VistaDia"
Cohesion: 0.12
Nodes (18): MouseWheelEventArgs, DateOnly, IReadOnlyList, Task, IAgendaService, Action, bool, Canvas (+10 more)

### Community 2 - "IconRailWindow"
Cohesion: 0.05
Nodes (34): Icono, Label, BitmapSource, DragEventArgs, HashSet, MouseEventArgs, ObservableCollection, Point (+26 more)

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
Cohesion: 0.12
Nodes (10): PepeWapOSx10.Dominio, PepeWapOSx10.Shell.Widgets, PepeWapOSx10.Calendario, PepeWapOSx10.Datos, PepeWapOSx10.Dominio.Interfaces, PepeWapOSx10.Dominio.Modelos, Action, double (+2 more)

### Community 7 - "AnclajeEscritorio"
Cohesion: 0.10
Nodes (15): IDisposable, bool, DispatcherTimer, DllImport, EnumWindowsProc, int, IntPtr, List (+7 more)

### Community 8 - "EspejoWallpaperWindow"
Cohesion: 0.11
Nodes (19): DWM_THUMBNAIL_PROPERTIES, MonitorEnumProc, MONITORINFO, SIZE, bool, byte, DispatcherTimer, DllImport (+11 more)

### Community 9 - "VideoWallpaperWindow"
Cohesion: 0.08
Nodes (18): LibVLC, MediaPlayer, Random, Reproductor, Velo, Window, bool, DispatcherTimer (+10 more)

### Community 10 - "Window"
Cohesion: 0.05
Nodes (35): Application, ActualWidth, IsDropDownOpen, PepeWapOSx10.App, Application, App, Window, MainWindow (+27 more)

### Community 11 - "MiniCalendario"
Cohesion: 0.09
Nodes (16): CultureInfo, DateOnly, SemanaIso, Action, bool, Border, DateOnly, double (+8 more)

### Community 12 - "Correr Agenda Dinámica"
Cohesion: 0.25
Nodes (7): 1. Matar cualquier instancia previa, 2. Build, 3. Lanzar, 4. Verla / interactuar con ella (importante, no es un top-level normal), 5. Cerrar, Correr Agenda Dinámica, Notas / trampas conocidas

### Community 13 - "Window"
Cohesion: 0.13
Nodes (15): Stroke, AppsEnEjecucion, Bg, BotonBandeja, BotonExplorador, BotonMenuWindows, BotonModoJuego, IconoModoJuego (+7 more)

### Community 14 - "BandejaDelSistema"
Cohesion: 0.10
Nodes (24): Bottom, EnLaBarra, Left, Punto, Right, byte, DllImport, IEnumerable (+16 more)

### Community 15 - "ProgramasAbiertos"
Cohesion: 0.12
Nodes (18): Icono, long, Nombre, byte, Dictionary, DllImport, EnumWindowsProc, ImageSource (+10 more)

### Community 16 - "MenuInicioPopover"
Cohesion: 0.08
Nodes (19): DateTime, ImageSource, IReadOnlyList, string, Task, TimeSpan, AppDeInicio, AppsDeInicio (+11 more)

### Community 17 - "PepeWapOSx10.Dominio.csproj"
Cohesion: 0.10
Nodes (21): Ical.Net (5.2.3), LibVLCSharp (3.9.1), LibVLCSharp.WPF (3.9.1), Microsoft.Data.Sqlite (10.0.11), VideoLAN.LibVLC.Windows (3.0.21), net10.0-windows, Microsoft.NET.Sdk, net10.0 (+13 more)

### Community 18 - "Background Calendar (PepeWapOSx10)"
Cohesion: 0.29
Nodes (6): Background Calendar (PepeWapOSx10), Configuración, Correrlo, Estado actual, Estructura de la solución, Requisitos

### Community 19 - "Esqueleto"
Cohesion: 0.23
Nodes (8): Border, Canvas, double, Grid, Panel, TimeSpan, UIElement, Esqueleto

### Community 20 - "MainWindow"
Cohesion: 0.12
Nodes (12): bool, DateOnly, DispatcherTimer, EventArgs, MouseButtonEventArgs, RoutedEventArgs, Task, TimeSpan (+4 more)

### Community 21 - "OrdenDeRieles"
Cohesion: 0.23
Nodes (8): JsonDocument, JsonSerializerOptions, Reparto, IReadOnlySet, List, string, OrdenDeRieles, Reparto

### Community 22 - "ResourceDictionary"
Cohesion: 0.33
Nodes (5): Bg, PART_Track, ResourceDictionary, Border, Track

### Community 28 - ".TabVista_Click"
Cohesion: 0.60
Nodes (4): TabDia, TabMes, TabSemana, ToggleButton

### Community 29 - "DockPanel"
Cohesion: 0.50
Nodes (4): DiaPanel, MesPanel, SemanaPanel, DockPanel

### Community 30 - ".AbrirConexion"
Cohesion: 0.10
Nodes (15): SqliteCommand, SqliteConnection, string, AgendaDbContext, DateOnly, DayOfWeek, IReadOnlyList, IReadOnlySet (+7 more)

### Community 31 - "VistaSemana"
Cohesion: 0.19
Nodes (11): Action, Border, Canvas, DateOnly, double, Grid, IReadOnlyList, ScrollViewer (+3 more)

### Community 32 - "ScheduledBlock"
Cohesion: 0.20
Nodes (9): Ellipse, Rectangle, DateTime, BlockKind, ScheduledBlock, StackPanel, double, Grid (+1 more)

### Community 33 - "DetectorDobleClick"
Cohesion: 0.13
Nodes (12): Action, Button, DateTime, DllImport, FrameworkElement, Func, MouseButtonEventArgs, string (+4 more)

### Community 34 - ".Posicionar"
Cohesion: 0.22
Nodes (7): DiaCargado, Border, Brush, Canvas, DateTime, UIElement, Thickness

### Community 35 - "PopoverBase"
Cohesion: 0.15
Nodes (9): object, StackPanel, ListaVentanasPopover, bool, Border, double, FrameworkElement, Func (+1 more)

### Community 36 - "TaskbarWindow"
Cohesion: 0.18
Nodes (8): bool, Dictionary, DispatcherTimer, FrameworkElement, string, TimeSpan, UIElement, TaskbarWindow

### Community 37 - "AccionesDeSistema"
Cohesion: 0.21
Nodes (5): byte, DllImport, uint, UIntPtr, AccionesDeSistema

## Knowledge Gaps
- **57 isolated node(s):** `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0`, `Ical.Net (5.2.3)`, `Microsoft.NET.Sdk` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PepeWapOSx10.Shell.Widgets` connect `PepeWapOSx10.Shell.Widgets` to `DetectorDobleClick`, `IconRailWindow`, `PopoverBase`, `TaskbarWindow`, `AccionesDeSistema`, `AnclajeEscritorio`, `EspejoWallpaperWindow`, `VideoWallpaperWindow`, `Window`, `MiniCalendario`, `BandejaDelSistema`, `ProgramasAbiertos`, `MenuInicioPopover`, `Esqueleto`, `OrdenDeRieles`?**
  _High betweenness centrality (0.333) - this node is a cross-community bridge._
- **Why does `MainWindow` connect `MainWindow` to `Window`, `VistaDia`, `VistaMes`, `PanelGuia`, `PepeWapOSx10.Shell.Widgets`, `AnclajeEscritorio`, `Window`, `MiniCalendario`, `.TabVista_Click`, `VistaSemana`?**
  _High betweenness centrality (0.250) - this node is a cross-community bridge._
- **Why does `TaskbarWindow` connect `TaskbarWindow` to `PopoverBase`, `AccionesDeSistema`, `RoutedEventArgs`, `AnclajeEscritorio`, `Window`, `Window`, `ProgramasAbiertos`?**
  _High betweenness centrality (0.121) - this node is a cross-community bridge._
- **What connects `net10.0-windows`, `Microsoft.NET.Sdk`, `net10.0` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Window` be split into smaller, more focused modules?**
  _Cohesion score 0.11083743842364532 - nodes in this community are weakly interconnected._
- **Should `VistaDia` be split into smaller, more focused modules?**
  _Cohesion score 0.12 - nodes in this community are weakly interconnected._
- **Should `IconRailWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.054274084124830396 - nodes in this community are weakly interconnected._