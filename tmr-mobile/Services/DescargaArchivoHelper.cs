#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace tmr_mobile.Services;

/// <summary>
/// Guarda el archivo en Descargas cuando la plataforma lo permite y, después,
/// abre siempre el menú nativo para compartirlo.
/// </summary>
public static class DescargaArchivoHelper
{
    public const string MimeTypePdf = "application/pdf";
    public const string MimeTypeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static async Task<string?> GuardarYCompartirAsync(
        byte[] contenido,
        string nombreArchivo,
        string mimeType,
        string tituloCompartir)
    {
        if (contenido.Length == 0)
            throw new ArgumentException("El archivo generado está vacío.", nameof(contenido));

        var rutaTemporal = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);
        await File.WriteAllBytesAsync(rutaTemporal, contenido);

        string? ubicacionGuardada = null;
        var archivoParaCompartir = rutaTemporal;

#if WINDOWS
        var carpetaDescargas = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(carpetaDescargas);

        var rutaFinal = ObtenerRutaDisponible(carpetaDescargas, nombreArchivo);
        await File.WriteAllBytesAsync(rutaFinal, contenido);
        ubicacionGuardada = rutaFinal;
        archivoParaCompartir = rutaFinal;

#elif ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            var resolver = Android.App.Application.Context.ContentResolver
                ?? throw new InvalidOperationException("No se pudo acceder al almacenamiento de Android.");

            var valores = new ContentValues();
            valores.Put(MediaStore.IMediaColumns.DisplayName, nombreArchivo);
            valores.Put(MediaStore.IMediaColumns.MimeType, mimeType);
            valores.Put(MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

            var uriDestino = resolver.Insert(MediaStore.Downloads.ExternalContentUri!, valores)
                ?? throw new InvalidOperationException("No se pudo crear el archivo en Descargas.");

            try
            {
                await using var outputStream = resolver.OpenOutputStream(uriDestino)
                    ?? throw new InvalidOperationException("No se pudo escribir el archivo descargado.");
                await outputStream.WriteAsync(contenido);
                ubicacionGuardada = uriDestino.ToString();
            }
            catch
            {
                resolver.Delete(uriDestino, null, null);
                throw;
            }
        }
        else
        {
            // Android 9 o anterior no ofrece MediaStore.Downloads. Se usa la carpeta
            // Download propia de la app (sin permisos invasivos de almacenamiento).
            var carpetaAndroid = Android.App.Application.Context
                .GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath
                ?? FileSystem.AppDataDirectory;
            Directory.CreateDirectory(carpetaAndroid);
            var rutaAndroid = ObtenerRutaDisponible(carpetaAndroid, nombreArchivo);
            await File.WriteAllBytesAsync(rutaAndroid, contenido);
            ubicacionGuardada = rutaAndroid;
            archivoParaCompartir = rutaAndroid;
        }
#endif

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = tituloCompartir,
            File = new ShareFile(archivoParaCompartir, mimeType)
        });

        return ubicacionGuardada;
    }

    public static Task<string?> GuardarExcelAsync(byte[] contenido, string nombreArchivo) =>
        GuardarYCompartirAsync(contenido, nombreArchivo, MimeTypeXlsx, "Compartir reporte Excel");

    private static string ObtenerRutaDisponible(string carpeta, string nombreArchivo)
    {
        var ruta = Path.Combine(carpeta, nombreArchivo);
        if (!File.Exists(ruta))
            return ruta;

        var nombre = Path.GetFileNameWithoutExtension(nombreArchivo);
        var extension = Path.GetExtension(nombreArchivo);
        for (var numero = 2; ; numero++)
        {
            ruta = Path.Combine(carpeta, $"{nombre} ({numero}){extension}");
            if (!File.Exists(ruta))
                return ruta;
        }
    }
}
