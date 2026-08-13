// Colores tomados exactamente del archivo Angular (reporte-excel.utils.ts)
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;

public static class ReporteEstilos
{
    public static readonly XLColor Cabecera = XLColor.FromArgb(0x16, 0x35, 0x72); // COLOR_CABECERA
    public static readonly XLColor Texto    = XLColor.FromArgb(0x33, 0x41, 0x55); // COLOR_TEXTO
    public static readonly XLColor Borde    = XLColor.FromArgb(0xE2, 0xE8, 0xF0); // COLOR_BORDE
    public static readonly XLColor Alterno  = XLColor.FromArgb(0xF8, 0xFA, 0xFC); // COLOR_ALTERNO
    public static readonly XLColor Blanco   = XLColor.White;
    public static readonly XLColor Verde    = XLColor.FromArgb(0x16, 0xA3, 0x4A); // activo/cargado
    public static readonly XLColor Gris     = XLColor.FromArgb(0x6B, 0x72, 0x80); // inactivo
}

public class ReporteColumna
{
    public string Encabezado { get; set; } = "";
    public double AnchoExcel { get; set; } = 20;
    public XLAlignmentHorizontalValues Alineacion { get; set; } = XLAlignmentHorizontalValues.Left;
}

public class ExcelExportService
{
    public byte[] GenerarReporteExcel(
        string titulo,
        string nombreHoja,
        List<ReporteColumna> columnas,
        List<object[]> filas,
        int? columnaEstado = null)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(nombreHoja);

        int ultimaColumna = columnas.Count;

        // --- Anchos de columna (equivalente a worksheet.columns) ---
        for (int i = 0; i < columnas.Count; i++)
            ws.Column(i + 1).Width = columnas[i].AnchoExcel;

        // --- Filas 1 y 2 en blanco + merge para título/cabecera (igual que Angular) ---
        var filaTitulo = ws.Row(1);
        filaTitulo.Height = 55; // ClosedXML usa puntos, no px; ajusta a gusto visual
        var filaSub = ws.Row(2);
        filaSub.Height = 30;

        ws.Range(1, 3, 1, ultimaColumna).Merge();
        ws.Range(2, 3, 2, ultimaColumna).Merge();

        for (int row = 1; row <= 2; row++)
        {
            for (int col = 1; col <= ultimaColumna; col++)
                ws.Cell(row, col).Style.Fill.BackgroundColor = ReporteEstilos.Cabecera;
        }

        var celdaTitulo = ws.Cell(1, 3);
        celdaTitulo.Value = titulo;
        celdaTitulo.Style.Font.FontName = "Calibri";
        celdaTitulo.Style.Font.FontSize = 20;
        celdaTitulo.Style.Font.Bold = true;
        celdaTitulo.Style.Font.FontColor = ReporteEstilos.Blanco;
        celdaTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
        celdaTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // --- Logo (igual que obtenerLogo() + addImage) ---
        InsertarLogo(ws);

        // --- Fila 3: encabezados de columna ---
        for (int i = 0; i < columnas.Count; i++)
        {
            var cell = ws.Cell(3, i + 1);
            cell.Value = columnas[i].Encabezado;
            cell.Style.Fill.BackgroundColor = ReporteEstilos.Cabecera;
            cell.Style.Font.FontName = "Segoe UI";
            cell.Style.Font.FontSize = 11;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = ReporteEstilos.Blanco;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.WrapText = true;
            AplicarBorde(cell, ReporteEstilos.Cabecera);
        }
        ws.Row(3).Height = 18;

        // --- Filas de datos (desde la fila 4) ---
        for (int f = 0; f < filas.Count; f++)
        {
            int filaExcel = f + 4;
            var datos = filas[f];
            var fillFila = filaExcel % 2 == 0 ? ReporteEstilos.Alterno : ReporteEstilos.Blanco;

            for (int c = 0; c < datos.Length; c++)
            {
                var cell = ws.Cell(filaExcel, c + 1);
                cell.Value = datos[c]?.ToString() ?? "";
                cell.Style.Fill.BackgroundColor = fillFila;
                cell.Style.Font.FontName = "Segoe UI";
                cell.Style.Font.FontSize = 10;
                cell.Style.Font.FontColor = ReporteEstilos.Texto;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.Horizontal = columnas[c].Alineacion;
                cell.Style.Alignment.WrapText = true;
                AplicarBorde(cell, ReporteEstilos.Borde);

                // Columna Estado con color condicional (igual que Angular)
                if (columnaEstado.HasValue && c == columnaEstado.Value)
                {
                    bool activo = (datos[c]?.ToString() ?? "").ToLower() == "cargado"
                                || (datos[c]?.ToString() ?? "").ToLower() == "activo";
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = activo ? ReporteEstilos.Verde : ReporteEstilos.Gris;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                }
            }
            ws.Row(filaExcel).Height = 18;
        }

        // --- Freeze panes + autofiltro (igual que views/autoFilter en Angular) ---
        ws.SheetView.FreezeRows(3);
        ws.Range(3, 1, 3, ultimaColumna).SetAutoFilter();

        // --- Orientación de página ---
        ws.PageSetup.PageOrientation = ultimaColumna > 6
            ? XLPageOrientation.Landscape
            : XLPageOrientation.Portrait;
        ws.PageSetup.FitToPages(1, 0);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void AplicarBorde(IXLCell cell, XLColor color)
    {
        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.TopBorderColor = color;
        cell.Style.Border.BottomBorderColor = color;
        cell.Style.Border.LeftBorderColor = color;
        cell.Style.Border.RightBorderColor = color;
    }

    // En InsertarLogo():
    private void InsertarLogo(IXLWorksheet ws)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("logo_reporte_integrity.png").Result;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            var picture = ws.AddPicture(ms, XLPictureFormat.Png);

            const double anchoDeseado = 230; // antes 172 — logo más grande
            double escala = anchoDeseado / picture.Width;

            picture.Width = (int)anchoDeseado;
            picture.Height = (int)(picture.Height * escala);

            picture.MoveTo(ws.Cell(1, 1), 4, 3);
        }
        catch
        {
            // Si no encuentra el logo, continúa sin romper el reporte
        }
    }
}