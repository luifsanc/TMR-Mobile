using System.Globalization;
using System.Text;

namespace tmr_mobile.Services;

public static class ReportService
{
    private const string OpcionPdf = "📄 Descargar PDF";
    private const string OpcionExcel = "📊 Descargar Excel";

    /// <summary>
    /// Flujo común para todos los reportes: elegir formato, generar un archivo real,
    /// guardarlo en Descargas y mostrar el menú nativo de compartir.
    /// </summary>
    public static async Task SeleccionarYExportarAsync(
        string titulo,
        string[] encabezados,
        List<string[]> filas,
        string nombreArchivo)
    {
        if (filas.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Exportar",
                "No hay datos para exportar.",
                "OK");

            return;
        }

        var opcion = await Shell.Current.DisplayActionSheetAsync(
            "Seleccione el formato de descarga:",
            "Cancelar",
            null,
            OpcionPdf,
            OpcionExcel);

        try
        {
            if (opcion == OpcionPdf)
                await ExportarPdfAsync(titulo, encabezados, filas, nombreArchivo);
            else if (opcion == OpcionExcel)
                await ExportarExcelAsync(titulo, encabezados, filas, nombreArchivo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Exportar] {ex}");

            await Shell.Current.DisplayAlertAsync(
                "Error",
                $"No se pudo generar el reporte: {ex.Message}",
                "OK");
        }
    }

    public static async Task ExportarExcelAsync(
        string titulo,
        string[] encabezados,
        List<string[]> filas,
        string nombreArchivo)
    {
        var columnas = encabezados.Select(e => new ReporteColumna
        {
            Encabezado = e,
            AnchoExcel = Math.Clamp(e.Length + 12, 16, 36)
        }).ToList();

        var servicioExcel = new ExcelExportService();

        var columnaEstado = Array.FindLastIndex(
            encabezados,
            e => e.Contains("Estado", StringComparison.OrdinalIgnoreCase));

        var bytes = servicioExcel.GenerarReporteExcel(
            titulo,
            LimpiarNombreHoja(titulo),
            columnas,
            filas.Select(f => f.Cast<object>().ToArray()).ToList(),
            columnaEstado >= 0 ? columnaEstado : null);

        var archivo = $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        await DescargaArchivoHelper.GuardarYCompartirAsync(
            bytes,
            archivo,
            DescargaArchivoHelper.MimeTypeXlsx,
            $"Compartir {titulo}");
    }

    public static async Task ExportarPdfAsync(
        string titulo,
        string[] encabezados,
        List<string[]> filas,
        string nombreArchivo)
    {
        var bytes = GenerarPdf(titulo, encabezados, filas);

        var archivo = $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        await DescargaArchivoHelper.GuardarYCompartirAsync(
            bytes,
            archivo,
            DescargaArchivoHelper.MimeTypePdf,
            $"Compartir {titulo}");
    }

    // Compatibilidad con llamadas antiguas mientras otras ramas migran al flujo común.
    public static Task ExportarCsvAsync(
        string titulo,
        string[] encabezados,
        List<string[]> filas,
        string nombreArchivo) =>
        ExportarExcelAsync(titulo, encabezados, filas, nombreArchivo);

    public static Task ExportarHtmlPdfAsync(
        string titulo,
        string[] encabezados,
        List<string[]> filas,
        string nombreArchivo) =>
        ExportarPdfAsync(titulo, encabezados, filas, nombreArchivo);

    private static string LimpiarNombreHoja(string titulo)
    {
        var nombre = string.Concat(
            titulo.Select(c => "[]:*?/\\".Contains(c) ? ' ' : c)).Trim();

        return string.IsNullOrWhiteSpace(nombre)
            ? "Reporte"
            : nombre[..Math.Min(nombre.Length, 31)];
    }

    private static byte[] GenerarPdf(
        string titulo,
        string[] encabezados,
        List<string[]> filas)
    {
        const int filasPorPagina = 25;

        var paginas = filas.Chunk(filasPorPagina).ToList();

        var objetos = new List<string>();
        var idsPagina = new List<int>();
        var idsContenido = new List<int>();

        // 1 = catálogo, 2 = árbol de páginas, 3 = fuente.
        for (var i = 0; i < paginas.Count; i++)
        {
            idsPagina.Add(4 + i * 2);
            idsContenido.Add(5 + i * 2);
        }

        objetos.Add("<< /Type /Catalog /Pages 2 0 R >>");

        objetos.Add(
            $"<< /Type /Pages /Count {paginas.Count} /Kids [{string.Join(" ", idsPagina.Select(id => $"{id} 0 R"))}] >>");

        objetos.Add(
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");

        for (var i = 0; i < paginas.Count; i++)
        {
            var contenido = CrearContenidoPagina(
                titulo,
                encabezados,
                paginas[i],
                i + 1,
                paginas.Count);

            objetos.Add(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] /Resources << /Font << /F1 3 0 R >> >> /Contents {idsContenido[i]} 0 R >>");

            objetos.Add(
                $"<< /Length {Encoding.ASCII.GetByteCount(contenido)} >>\nstream\n{contenido}\nendstream");
        }

        using var stream = new MemoryStream();

        void Escribir(string texto)
        {
            var datos = Encoding.ASCII.GetBytes(texto);
            stream.Write(datos, 0, datos.Length);
        }

        Escribir("%PDF-1.4\n%TMR\n");

        var offsets = new List<long> { 0 };

        for (var i = 0; i < objetos.Count; i++)
        {
            offsets.Add(stream.Position);

            Escribir(
                $"{i + 1} 0 obj\n{objetos[i]}\nendobj\n");
        }

        var inicioXref = stream.Position;

        Escribir(
            $"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
            Escribir($"{offset:0000000000} 00000 n \n");

        Escribir(
            $"trailer\n<< /Size {objetos.Count + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF");

        return stream.ToArray();
    }

    private static string CrearContenidoPagina(
        string titulo,
        string[] encabezados,
        string[][] filas,
        int pagina,
        int totalPaginas)
    {
        const double margen = 32;
        const double anchoUtil = 778;
        const double altoFila = 18;

        var anchoColumna =
            anchoUtil / Math.Max(encabezados.Length, 1);

        var sb = new StringBuilder();

        AgregarTexto(
            sb,
            titulo,
            margen,
            558,
            18,
            negritaSimulada: true);

        AgregarTexto(
            sb,
            $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}   Página {pagina} de {totalPaginas}",
            margen,
            539,
            8);

        var y = 510d;

        sb.AppendLine(
            $"0.086 0.208 0.447 rg " +
            $"{margen.ToString(CultureInfo.InvariantCulture)} " +
            $"{y.ToString(CultureInfo.InvariantCulture)} " +
            $"{anchoUtil.ToString(CultureInfo.InvariantCulture)} 22 re f");

        for (var c = 0; c < encabezados.Length; c++)
        {
            AgregarTexto(
                sb,
                Recortar(encabezados[c], anchoColumna, 8),
                margen + c * anchoColumna + 4,
                y + 7,
                8);
        }

        y -= altoFila;

        for (var f = 0; f < filas.Length; f++)
        {
            if (f % 2 == 0)
            {
                sb.AppendLine(
                    $"0.973 0.980 0.988 rg " +
                    $"{margen.ToString(CultureInfo.InvariantCulture)} " +
                    $"{y.ToString(CultureInfo.InvariantCulture)} " +
                    $"{anchoUtil.ToString(CultureInfo.InvariantCulture)} " +
                    $"{altoFila.ToString(CultureInfo.InvariantCulture)} re f");
            }

            for (var c = 0; c < encabezados.Length; c++)
            {
                var valor =
                    c < filas[f].Length
                        ? filas[f][c]
                        : string.Empty;

                AgregarTexto(
                    sb,
                    Recortar(valor, anchoColumna, 7),
                    margen + c * anchoColumna + 4,
                    y + 6,
                    7,
                    colorOscuro: true);
            }

            sb.AppendLine(
                $"0.886 0.910 0.941 RG 0.4 w " +
                $"{margen.ToString(CultureInfo.InvariantCulture)} " +
                $"{y.ToString(CultureInfo.InvariantCulture)} m " +
                $"{(margen + anchoUtil).ToString(CultureInfo.InvariantCulture)} " +
                $"{y.ToString(CultureInfo.InvariantCulture)} l S");

            y -= altoFila;
        }

        return sb.ToString();
    }

    private static void AgregarTexto(
        StringBuilder sb,
        string texto,
        double x,
        double y,
        int tamano,
        bool negritaSimulada = false,
        bool colorOscuro = false)
    {
        var color =
            colorOscuro
                ? "0.20 0.25 0.33"
                : (y < 532 && y > 500
                    ? "1 1 1"
                    : "0.086 0.208 0.447");

        sb.AppendLine(
            $"BT /F1 {tamano} Tf {color} rg " +
            $"{x.ToString(CultureInfo.InvariantCulture)} " +
            $"{y.ToString(CultureInfo.InvariantCulture)} Td " +
            $"({EscaparPdf(texto)}) Tj ET");

        if (negritaSimulada)
        {
            sb.AppendLine(
                $"BT /F1 {tamano} Tf {color} rg " +
                $"{(x + 0.35).ToString(CultureInfo.InvariantCulture)} " +
                $"{y.ToString(CultureInfo.InvariantCulture)} Td " +
                $"({EscaparPdf(texto)}) Tj ET");
        }
    }

    private static string Recortar(
        string? valor,
        double ancho,
        int tamano)
    {
        valor = string.IsNullOrWhiteSpace(valor)
            ? "-"
            : valor
                .Replace('\n', ' ')
                .Replace('\r', ' ');

        var maximo =
            Math.Max(4, (int)(ancho / (tamano * 0.55)));

        return valor.Length <= maximo
            ? valor
            : valor[..Math.Max(1, maximo - 1)] + "…";
    }

    private static string EscaparPdf(string texto)
    {
        var sb = new StringBuilder();

        foreach (var caracter in texto)
        {
            if (caracter is '\\' or '(' or ')')
            {
                sb.Append('\\').Append(caracter);
            }
            else if (caracter is >= ' ' and <= '~')
            {
                sb.Append(caracter);
            }
            else
            {
                sb.Append('\\')
                    .Append(
                        Convert
                            .ToString(ObtenerWinAnsi(caracter), 8)
                            .PadLeft(3, '0'));
            }
        }

        return sb.ToString();
    }

    private static byte ObtenerWinAnsi(char c) => c switch
    {
        'á' => 225,
        'é' => 233,
        'í' => 237,
        'ó' => 243,
        'ú' => 250,

        'Á' => 193,
        'É' => 201,
        'Í' => 205,
        'Ó' => 211,
        'Ú' => 218,

        'ñ' => 241,
        'Ñ' => 209,
        'ü' => 252,
        'Ü' => 220,

        '¿' => 191,
        '¡' => 161,
        '–' => 150,
        '—' => 151,
        '…' => 133,

        _ => (byte)'?'
    };
}