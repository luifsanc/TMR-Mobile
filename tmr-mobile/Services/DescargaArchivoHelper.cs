#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace tmr_mobile.Services;

/// <summary>
/// Guarda un archivo generado en memoria en la ubicación "natural" de cada
/// plataforma (carpeta de Descargas en Windows/Android; Share sheet en iOS/MacCatalyst,
/// donde no existe una carpeta pública accesible para apps de terceros).
/// </summary>
public static class DescargaArchivoHelper
{
    private const string MimeTypeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Devuelve la ruta/URI final donde quedó guardado el archivo (Windows/Android),
    /// o null en iOS/MacCatalyst, donde el guardado se resuelve a través del Share sheet
    /// nativo y no hay una ruta directa que mostrarle al usuario.
    /// </summary>
    public static async Task<string?> GuardarExcelAsync(byte[] contenido, string nombreArchivo)
    {
#if WINDOWS
        var carpetaDescargas = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(carpetaDescargas);

        var rutaFinal = Path.Combine(carpetaDescargas, nombreArchivo);
        await File.WriteAllBytesAsync(rutaFinal, contenido);
        return rutaFinal;

#elif ANDROID
        var resolver = Android.App.Application.Context.ContentResolver
            ?? throw new InvalidOperationException("No se pudo acceder al ContentResolver de Android.");

        var valores = new ContentValues();
        valores.Put(MediaStore.MediaColumns.DisplayName, nombreArchivo);
        valores.Put(MediaStore.MediaColumns.MimeType, MimeTypeXlsx);
        valores.Put(MediaStore.MediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

        var uriDestino = resolver.Insert(MediaStore.Downloads.ExternalContentUri!, valores)
            ?? throw new InvalidOperationException("No se pudo crear el archivo en la carpeta de Descargas.");

        var outputStream = resolver.OpenOutputStream(uriDestino)
            ?? throw new InvalidOperationException("No se pudo abrir el archivo destino para escritura.");

        await using (outputStream)
            await outputStream.WriteAsync(contenido);

        return uriDestino.ToString();

#else
        // iOS / MacCatalyst: no existe una carpeta de Descargas pública para apps de
        // terceros. Se guarda en el caché de la app y se ofrece vía el Share sheet nativo
        // para que el usuario elija dónde ponerlo (AirDrop, Archivos, Guardar en Drive, etc.).
        var rutaTemporal = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);
        await File.WriteAllBytesAsync(rutaTemporal, contenido);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Guardar actividades",
            File = new ShareFile(rutaTemporal)
        });

        return null;
#endif
    }
}
