# Background Calendar (PepeWapOSx10)

Skin personalizado de Windows 10: widgets propios de escritorio para
optimizar la gestión del espacio en pantalla (agenda, iconos, taskbar), con
una **agenda dinámica, interactiva y modular** en el centro — incluyendo una
todolist integrada — corriendo en background sobre el desktop.

Todo el stack es **C# / .NET 10**.

## Estado actual

- **Motor de agenda** (dominio, sin UI): lee eventos fijos desde Google
  Calendar y arma la agenda del día combinando fijos + tareas flexibles +
  huecos libres.
- **Widget de escritorio** (WPF): ventana transparente, sin bordes,
  always-on-top, con tres vistas de agenda —
  - **Día**: timeline vertical con eje de horas, scroll continuo entre días.
  - **Semana**: grilla de 24 horas × 7 días, con los bloques del mismo
    horario alineados en la misma fila entre columnas.
  - **Mes**: grilla de calendario con número de semana ISO, chips de
    eventos por día, y navegación (`‹ › Hoy`).
  - Selección con click y navegación con doble click entre vistas (día ↔
    semana ↔ mes).
- **Guía (checklist)**: tareas recurrentes (limpieza, higiene,
  entrenamiento, pagos, etc.) con alta, edición y repetición configurable
  (única / semanal / mensual) — el estado de "hecha" se deriva de la última
  vez que se marcó y la repetición, sin necesidad de ningún reset manual.
- **Contenedores de iconos** y **mejoras de taskbar**: todavía sin empezar
  (proyectos scaffoldeados, sin lógica).
- **Escritura a Google Calendar**: pendiente — hoy la lectura es de solo
  lectura vía ICS; migrar a la Calendar API con OAuth para poder editar
  eventos desde el widget está planeado pero no implementado.

## Estructura de la solución

```
src/
  PepeWapOSx10.Dominio/         Modelos y Scheduler — sin dependencias de UI/infra
  PepeWapOSx10.Datos/           Persistencia SQLite (tareas flexibles, guía)
  PepeWapOSx10.Calendario/      Lectura de Google Calendar (ICS)
  PepeWapOSx10.Shell.Widgets/   Widget de escritorio WPF (agenda + guía)
  PepeWapOSx10.Shell.Iconos/    Contenedores de iconos de escritorio (sin implementar)
  PepeWapOSx10.Shell.Taskbar/   Mejoras de taskbar (sin implementar)
  PepeWapOSx10.App/             Orquestador de bandeja del sistema (sin implementar)
  PepeWapOSx10.SmokeTest/       Arnés de consola para probar el motor end-to-end
```

Los proyectos `Shell.*` dependen de `Dominio`/`Datos`/`Calendario`, nunca al
revés, para poder seguir probando el motor por consola aunque el shell no
exista o esté roto.

## Requisitos

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Configuración

El widget necesita la URL del ICS privado de tu Google Calendar. Creá
`src/PepeWapOSx10.Calendario/config.json` (gitignoreado, nunca se commitea):

```json
{
  "ics_url": "https://calendar.google.com/calendar/ical/.../private-.../basic.ics"
}
```

## Correrlo

```powershell
dotnet build src/PepeWapOSx10.Shell.Widgets/PepeWapOSx10.Shell.Widgets.csproj
dotnet run --project src/PepeWapOSx10.Shell.Widgets
```

Para probar el motor de agenda solo, sin UI:

```powershell
dotnet run --project src/PepeWapOSx10.SmokeTest
```
