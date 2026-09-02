using System.IO;
using System.Text.Json;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Lee la carpeta de loops de video para el wallpaper animado desde
/// <c>config.json</c> (clave <c>carpeta_wallpapers</c>, opcional).
/// </summary>
internal static class WallpaperConfig
{
    /// <returns>
    /// La carpeta configurada, o <c>null</c> si la clave no está, está vacía,
    /// o la carpeta no existe (wallpaper animado deshabilitado).
    /// </returns>
    public static string? CarpetaConfigurada()
    {
        var rutaConfig = Path.Combine(AppContext.BaseDirectory, "config.json");
        if (!File.Exists(rutaConfig))
            return null;

        using var documento = JsonDocument.Parse(File.ReadAllText(rutaConfig));
        if (!documento.RootElement.TryGetProperty("carpeta_wallpapers", out var valor))
            return null;

        var carpeta = valor.GetString();
        return string.IsNullOrWhiteSpace(carpeta) || !Directory.Exists(carpeta) ? null : carpeta;
    }
}
