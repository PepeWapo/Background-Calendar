---
name: run
description: Build and launch the Agenda Dinámica WPF widget (PepeWapOSx10.Shell.Widgets), verify it's actually running, and see/interact with it. Use whenever asked to run, launch, test, or screenshot this app.
---

# Correr Agenda Dinámica

Es una app WPF (.NET, Windows) que corre como un widget "on desktop": sin
bordes, sin taskbar, fijada al fondo del z-order (ver
`src/PepeWapOSx10.Shell.Widgets/Escritorio/AnclajeEscritorio.cs`). No es
`Topmost` — vive *debajo* de cualquier ventana normal, así que para verla o
interactuar con ella no puede haber otra ventana tapándola en esa zona de
pantalla.

## 1. Matar cualquier instancia previa

El build falla con `MSB3021`/`MSB3027` (archivo bloqueado) si queda un
proceso viejo corriendo. Siempre limpiar primero:

```bash
taskkill //F //IM PepeWapOSx10.Shell.Widgets.exe 2>&1; echo done
```

(no falla si no había ninguno corriendo — el `2>&1; echo done` es solo para
no cortar la cadena de comandos si `taskkill` devuelve error)

## 2. Build

```bash
cd "/c/Users/PepeWapo/Documents/programacion/AgendaDinamica"
dotnet build src/PepeWapOSx10.Shell.Widgets/PepeWapOSx10.Shell.Widgets.csproj
```

Debe dar "0 Advertencia(s) 0 Errores". Si no, parar y arreglar antes de
seguir — no tiene sentido lanzar un build roto.

## 3. Lanzar

Lanzar en background (la app no termina sola, hay que seguir usando la
terminal):

```bash
cd "/c/Users/PepeWapo/Documents/programacion/AgendaDinamica"
(nohup dotnet run --project src/PepeWapOSx10.Shell.Widgets --no-build > /tmp/widget_run.log 2>&1 &) ; sleep 8; echo launched
```

Confirmar que el proceso real (el `.exe`, no el `dotnet run` wrapper) está
vivo:

```powershell
Get-Process -Name "PepeWapOSx10.Shell.Widgets" -ErrorAction SilentlyContinue | Select-Object Id
```

## 4. Verla / interactuar con ella (importante, no es un top-level normal)

Como está fijada al fondo del z-order, **hay que minimizar todo lo demás
primero** para que quede a la vista:

```powershell
$shell = New-Object -ComObject Shell.Application
$shell.MinimizeAll()
Start-Sleep -Seconds 1
```

Después, una captura normal de pantalla completa (`CopyFromScreen`) la va a
mostrar. El widget principal se llama "Agenda Dinámica"; los dos
contenedores de iconos anclados a los bordes laterales se llaman "Riel de
iconos" (uno a cada lado). Ninguno aparece en Alt+Tab (a propósito,
`WS_EX_TOOLWINDOW`) ni en la barra de tareas.

Para simular clicks/scroll reales (no alcanza con `TogglePattern.Toggle()`
en botones con `ControlTemplate` custom — no dispara el evento `Click`):
usar UI Automation para ubicar el elemento por nombre y sacar su
`BoundingRectangle` (coordenadas físicas, confiables), y despachar el click
con `SetCursorPos` + `mouse_event`/`SendInput` en vez de coordinar a mano.
El hwnd de la ventana se encuentra por `EnumWindows` + `GetWindowText`
(título exacto "Agenda Dinámica"), no hace falta buscar un `WorkerW` — ya
no se reparenta a la jerarquía de Explorer.

## 5. Cerrar

```bash
taskkill //F //IM PepeWapOSx10.Shell.Widgets.exe 2>&1; echo done
```

## Notas / trampas conocidas

- **DPI**: para clicks/capturas por coordenadas, usar siempre
  `AutomationElement.BoundingRectangle` (píxeles físicos, confiable) y NO
  `GetWindowRect` desde un proceso PowerShell no DPI-aware (devuelve
  coordenadas "virtualizadas" que no coinciden con la pantalla real).
- Si algún día se vuelve a intentar anclar detrás de los íconos reales del
  escritorio (`WorkerW`, estilo Rainmeter clásico): **no lo hagas** sin
  releer el historial — en este Windows el `SysListView32` de Explorer
  captura el mouse en toda su área y bloquea los clicks al widget. Por eso
  hoy se usa `SetWindowPos(HWND_BOTTOM)` como ventana normal en vez de
  reparentar.
