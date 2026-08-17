// Colores tomados exactamente del archivo Angular (reporte-excel.utils.ts)
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;

public static class ReporteEstilos
{
    public static readonly XLColor Cabecera = XLColor.FromArgb(0x16, 0x35, 0x72); // COLOR_CABECERA
    public static readonly XLColor Texto = XLColor.FromArgb(0x33, 0x41, 0x55); // COLOR_TEXTO
    public static readonly XLColor Borde = XLColor.FromArgb(0xE2, 0xE8, 0xF0); // COLOR_BORDE
    public static readonly XLColor Alterno = XLColor.FromArgb(0xF8, 0xFA, 0xFC); // COLOR_ALTERNO
    public static readonly XLColor Blanco = XLColor.White;
    public static readonly XLColor Verde = XLColor.FromArgb(0x16, 0xA3, 0x4A); // activo/cargado
    public static readonly XLColor Gris = XLColor.FromArgb(0x6B, 0x72, 0x80); // inactivo
}

public class ReporteColumna
{
    public string Encabezado { get; set; } = "";
    public double AnchoExcel { get; set; } = 20;
    public XLAlignmentHorizontalValues Alineacion { get; set; } = XLAlignmentHorizontalValues.Left;
}

public class ExcelExportService
{
    // Tope máximo de ancho de columna (ajusta a gusto)
    private const double ANCHO_MAXIMO = 45;

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

        // --- Anchos de columna iniciales (equivalente a worksheet.columns) ---
        // Estos se usan como "piso mínimo"; el ajuste final se hace después de llenar los datos
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

                // Columna Estado con color condicional (igual que Angular y la UI)
                if (columnaEstado.HasValue && c == columnaEstado.Value)
                {
                    string valorEstado = (datos[c]?.ToString() ?? "").ToLower();

                    cell.Style.Font.Bold = true;

                    if (valorEstado == "completo")
                        cell.Style.Font.FontColor = XLColor.FromArgb(0x43, 0xA0, 0x47); // Verde
                    else if (valorEstado == "en progreso")
                        cell.Style.Font.FontColor = XLColor.FromArgb(0xF5, 0x9E, 0x0B); // Naranja
                    else if (valorEstado == "pendiente")
                        cell.Style.Font.FontColor = XLColor.FromArgb(0xE5, 0x39, 0x35); // Rojo
                    else
                    {
                        bool activo = valorEstado == "cargado" || valorEstado == "activo";
                        cell.Style.Font.FontColor = activo ? ReporteEstilos.Verde : ReporteEstilos.Gris;
                    }

                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                }
            }
            // Nota: ya NO fijamos ws.Row(filaExcel).Height aquí.
            // El alto final se calcula en AjustarAltoFilas() después del loop.
        }

        // --- Ajuste automático de ancho y alto (nuevo) ---
        AjustarAnchoColumnas(ws, columnas, ultimaColumna, filas.Count);
        AjustarAltoFilas(ws, filas.Count);

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

    /// <summary>
    /// Autoajusta el ancho de cada columna al contenido real (encabezado + datos),
    /// respetando un mínimo (AnchoExcel de cada ReporteColumna) y un máximo (ANCHO_MAXIMO)
    /// para que ninguna columna quede angostísima ni se dispare de tamaño.
    /// Debe llamarse DESPUÉS de llenar encabezados y datos.
    /// </summary>
    private void AjustarAnchoColumnas(
        IXLWorksheet ws,
        List<ReporteColumna> columnas,
        int ultimaColumna,
        int cantidadFilas)
    {
        // Autoajuste base sobre encabezados (fila 3) + filas de datos
        // Nota: se usa ws.Columns(...).AdjustToContents(filaInicio, filaFin) en vez de
        // range.Columns().AdjustToContents() porque IXLRangeColumns no expone ese método
        // en algunas versiones de ClosedXML; IXLColumns (a nivel de hoja) sí lo tiene.
        int filaFinAutoajuste = cantidadFilas + 3;
        ws.Columns(1, ultimaColumna).AdjustToContents(3, filaFinAutoajuste);

        // Aplicar piso mínimo y techo máximo por columna
        for (int i = 0; i < columnas.Count; i++)
        {
            var columna = ws.Column(i + 1);

            if (columna.Width < columnas[i].AnchoExcel)
                columna.Width = columnas[i].AnchoExcel;

            if (columna.Width > ANCHO_MAXIMO)
                columna.Width = ANCHO_MAXIMO;
        }
    }

    /// <summary>
    /// Autoajusta el alto de las filas de datos (desde la fila 4) según el contenido
    /// envuelto (WrapText), para que el texto largo no se monte una línea encima de otra.
    /// Debe llamarse DESPUÉS de fijar los anchos finales de columna.
    /// </summary>
    private void AjustarAltoFilas(IXLWorksheet ws, int cantidadFilas)
    {
        if (cantidadFilas <= 0) return;

        ws.Rows(4, cantidadFilas + 3).AdjustToContents();
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

    private void InsertarLogo(IXLWorksheet ws)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("logo_reporte_integrity.png").Result;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            var picture = ws.AddPicture(ms, XLPictureFormat.Png);

            // Ajustar logo según referencia (aprox 172x55, pero en MAUI puede necesitar multiplicador de escala)
            // Utilizando un tamaño mayor para asegurar visibilidad en la cabecera alta.
            picture.Width = 300;
            picture.Height = 85;

            picture.MoveTo(ws.Cell(1, 1), 10, 10);
        }
        catch
        {
            // Si no encuentra el logo, continúa sin romper el reporte
        }
    }

    public byte[] GenerarReporteDetalleColaborador(
        string nombreColaborador,
        DateTime startDate,
        DateTime endDate,
        List<tmr_mobile.Models.Seguimiento.ActividadDetalleDto> actividades,
        List<string> feriados)
    {
        using var workbook = new XLWorkbook();

        var groupsByClient = actividades
            .GroupBy(a => string.IsNullOrWhiteSpace(a.ClienteProyecto) ? "Sin Cliente" : a.ClienteProyecto)
            .ToList();

        var listDates = new List<DateTime>();
        for (var cur = startDate; cur <= endDate; cur = cur.AddDays(1))
            listDates.Add(cur);

        int totalDays = listDates.Count;
        int totalCols = 6 + totalDays + 1;

        foreach (var group in groupsByClient)
        {
            var clientName = group.Key;
            var sheetName = $"Reporte_{clientName}".Replace("*", "").Replace("?", "").Replace(":", "").Replace("\\", "").Replace("/", "").Replace("[", "").Replace("]", "");
            if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);
            if (string.IsNullOrWhiteSpace(sheetName)) sheetName = "Reporte";

            var ws = workbook.Worksheets.Add(sheetName);

            ws.Column(1).Width = 5;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 25;
            ws.Column(4).Width = 25;
            ws.Column(5).Width = 60;
            ws.Column(6).Width = 15;
            for (int i = 0; i < totalDays; i++)
                ws.Column(7 + i).Width = 4.5;
            ws.Column(totalCols).Width = 15;

            // --- ESTANDARIZAR CABECERA (igual al de referencia web) ---
            ws.Row(1).Height = 79.25;
            ws.Row(2).Height = 28.25;

            ws.Range(1, 3, 1, totalCols).Merge();
            ws.Range(2, 3, 2, totalCols).Merge();

            // Fondo azul corporativo para la cabecera
            var rangeTop = ws.Range(1, 1, 2, totalCols);
            rangeTop.Style.Fill.BackgroundColor = ReporteEstilos.Cabecera;

            InsertarLogo(ws);

            var mesAnio = startDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-EC")).ToUpper();
            var titleCell = ws.Cell(1, 3);
            titleCell.Value = $"TIME REPORT - {clientName.ToUpper()}";
            titleCell.Style.Font.FontName = "Calibri";
            titleCell.Style.Font.FontSize = 20;
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontColor = XLColor.White;
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

            var subtitleCell = ws.Cell(2, 3);
            subtitleCell.Value = $"{mesAnio} | Generado: {DateTime.Now.ToString("dd/MM/yyyy")}";
            subtitleCell.Style.Font.FontName = "Calibri";
            subtitleCell.Style.Font.FontSize = 10;
            subtitleCell.Style.Font.FontColor = XLColor.White;
            subtitleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            subtitleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            // ------------------------------------------------------------

            ws.Cell(4, 1).Value = "Cliente:";
            ws.Cell(4, 1).Style.Font.Bold = true; ws.Cell(4, 1).Style.Font.FontColor = ReporteEstilos.Cabecera;
            ws.Cell(4, 3).Value = clientName;
            ws.Cell(4, 3).Style.Font.Bold = true;

            ws.Cell(5, 1).Value = "Nombre del consultor:";
            ws.Cell(5, 1).Style.Font.Bold = true; ws.Cell(5, 1).Style.Font.FontColor = ReporteEstilos.Cabecera;
            ws.Cell(5, 3).Value = nombreColaborador;
            ws.Cell(5, 3).Style.Font.Bold = true;

            ws.Cell(6, 1).Value = "N°";
            ws.Cell(6, 2).Value = "TIPO DE ACTIVIDAD";
            ws.Cell(6, 3).Value = "LIDER DE PROYECTO";
            ws.Cell(6, 4).Value = "CODIGO REQUERIMIENTO / INCIDENTE";
            ws.Cell(6, 5).Value = "DESCRIPCION DE TRABAJOS REALIZADOS";
            ws.Cell(6, 6).Value = "TOTAL HORAS POR ACTIVIDAD";
            ws.Cell(6, 7).Value = "DISTRIBUCION DE TIEMPO DEL DIA";
            ws.Cell(6, totalCols).Value = "TOTAL HORAS POR ACT.";

            ws.Range(6, 1, 8, 1).Merge();
            ws.Range(6, 2, 8, 2).Merge();
            ws.Range(6, 3, 8, 3).Merge();
            ws.Range(6, 4, 8, 4).Merge();
            ws.Range(6, 5, 8, 5).Merge();
            ws.Range(6, 6, 8, 6).Merge();
            ws.Range(6, 7, 6, 6 + totalDays - 1).Merge();
            ws.Range(6, totalCols, 8, totalCols).Merge();

            var weekdays = new[] { "D", "L", "M", "M", "J", "V", "S" };
            for (int i = 0; i < totalDays; i++)
            {
                ws.Cell(7, 7 + i).Value = listDates[i].ToString("dd");
                ws.Cell(8, 7 + i).Value = weekdays[(int)listDates[i].DayOfWeek];
            }

            var rngHeader = ws.Range(6, 1, 8, totalCols);
            rngHeader.Style.Fill.BackgroundColor = ReporteEstilos.Cabecera;
            rngHeader.Style.Font.FontColor = ReporteEstilos.Blanco;
            rngHeader.Style.Font.Bold = true;
            rngHeader.Style.Font.FontSize = 9;
            rngHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rngHeader.Style.Alignment.WrapText = true;
            AplicarBorde(ws.Cell(6, 1), ReporteEstilos.Blanco); // Just to not duplicate range styling logic manually for now. We will loop.

            foreach (var cell in rngHeader.Cells())
            {
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = ReporteEstilos.Blanco;
            }

            var actividadesAgrupadas = group.GroupBy(a => new { a.TipoActividad, a.LiderProyecto, a.CodigoRequerimiento, a.Descripcion, Rec = a.EsRecurrente || a.Recurrente }).ToList();

            int currentRow = 9;
            int seqNum = 1;

            foreach (var actGrp in actividadesAgrupadas)
            {
                ws.Cell(currentRow, 1).Value = seqNum++;
                ws.Cell(currentRow, 2).Value = actGrp.Key.TipoActividad;
                ws.Cell(currentRow, 3).Value = actGrp.Key.LiderProyecto;
                ws.Cell(currentRow, 4).Value = actGrp.Key.CodigoRequerimiento;
                ws.Cell(currentRow, 5).Value = actGrp.Key.Descripcion;

                var horasPorDia = actGrp.GroupBy(x => x.Fecha).ToDictionary(g => g.Key, g => g.Sum(x => x.Horas));

                for (int i = 0; i < totalDays; i++)
                {
                    var dateStr = listDates[i].ToString("yyyy-MM-dd");
                    if (horasPorDia.TryGetValue(dateStr, out var hrs) && hrs > 0)
                    {
                        ws.Cell(currentRow, 7 + i).Value = hrs;
                    }
                }

                ws.Cell(currentRow, 6).FormulaA1 = $"SUM({ws.Cell(currentRow, 7).Address.ColumnLetter}{currentRow}:{ws.Cell(currentRow, 6 + totalDays).Address.ColumnLetter}{currentRow})";
                ws.Cell(currentRow, totalCols).FormulaA1 = $"SUM({ws.Cell(currentRow, 7).Address.ColumnLetter}{currentRow}:{ws.Cell(currentRow, 6 + totalDays).Address.ColumnLetter}{currentRow})";

                var rowRange = ws.Range(currentRow, 1, currentRow, totalCols);
                rowRange.Style.Fill.BackgroundColor = XLColor.White;
                rowRange.Style.Font.FontSize = 10;
                rowRange.Style.Font.FontColor = ReporteEstilos.Texto;

                foreach (var cell in rowRange.Cells())
                {
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = ReporteEstilos.Borde;
                }

                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currentRow, totalCols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int c = 7; c <= 6 + totalDays; c++)
                {
                    var cell = ws.Cell(currentRow, c);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var dateIdx = c - 7;
                    var date = listDates[dateIdx];
                    var dateStr = date.ToString("yyyy-MM-dd");
                    var isWeekend = date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
                    var isFeriado = feriados.Contains(dateStr);
                    var hasVal = cell.Value.ToString() != "";

                    if (hasVal && actGrp.Key.TipoActividad == "Vacaciones")
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0xFF, 0xC0, 0x00);
                    else if (hasVal && actGrp.Key.TipoActividad == "Permiso")
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0x76, 0x93, 0x3C);
                    else if (hasVal && actGrp.Key.Rec)
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0xCC, 0xC0, 0xDA);
                    else if (isFeriado)
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
                    else if (isWeekend)
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0x8D, 0xB4, 0xE2);
                }

                currentRow++;
            }

            int totalRow = currentRow;
            ws.Cell(totalRow, 1).Value = "TOTAL";
            ws.Cell(totalRow, 6).FormulaA1 = $"SUM(F9:F{totalRow - 1})";
            ws.Cell(totalRow, totalCols).FormulaA1 = $"SUM({ws.Cell(totalRow, totalCols).Address.ColumnLetter}9:{ws.Cell(totalRow, totalCols).Address.ColumnLetter}{totalRow - 1})";

            for (int i = 0; i < totalDays; i++)
            {
                var colLetter = ws.Cell(totalRow, 7 + i).Address.ColumnLetter;
                ws.Cell(totalRow, 7 + i).FormulaA1 = $"SUM({colLetter}9:{colLetter}{totalRow - 1})";
            }

            var totRange = ws.Range(totalRow, 1, totalRow, totalCols);
            totRange.Style.Font.Bold = true;
            totRange.Style.Font.FontColor = ReporteEstilos.Cabecera;
            totRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            totRange.Style.Border.TopBorderColor = ReporteEstilos.Cabecera;
            totRange.Style.Border.BottomBorder = XLBorderStyleValues.Double;
            totRange.Style.Border.BottomBorderColor = ReporteEstilos.Cabecera;

            for (int i = 0; i < totalDays; i++)
            {
                var date = listDates[i];
                var dateStr = date.ToString("yyyy-MM-dd");
                var isWeekend = date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
                var isFeriado = feriados.Contains(dateStr);

                if (isFeriado)
                    ws.Cell(totalRow, 7 + i).Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
                else if (isWeekend)
                    ws.Cell(totalRow, 7 + i).Style.Fill.BackgroundColor = XLColor.FromArgb(0xFF, 0x8D, 0xB4, 0xE2);
            }

            int sigRow1 = totalRow + 5;
            int sigRow2 = totalRow + 6;
            ws.Cell(sigRow1, 2).Value = $"Elaborado por: {nombreColaborador}";
            ws.Cell(sigRow1, 2).Style.Font.Italic = true;
            ws.Cell(sigRow2, 2).Value = "ISC INTEGRITY SOLUTIONS & CONSULTING CIA. LTDA.";
            ws.Cell(sigRow2, 2).Style.Font.Bold = true;

            var lideres = group.Select(a => a.LiderProyecto).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().ToList();
            var leaderName = lideres.Any() ? string.Join(", ", lideres) : "Sin Líder";
            ws.Cell(sigRow1, 8).Value = $"Revisado y Aprobado por: {leaderName}";
            ws.Cell(sigRow1, 8).Style.Font.Italic = true;
            ws.Cell(sigRow2, 8).Value = $"Empresa: {clientName}";
            ws.Cell(sigRow2, 8).Style.Font.Bold = true;

            int nomRow = totalRow + 9;
            ws.Cell(nomRow, 2).Value = "Nomenclatura";
            ws.Cell(nomRow, 2).Style.Font.Bold = true; ws.Cell(nomRow, 2).Style.Font.FontColor = ReporteEstilos.Cabecera;

            var leyendas = new[] {
                ("Vacaciones", XLColor.FromArgb(0xFF, 0xFF, 0xC0, 0x00)),
                ("Feriado", XLColor.FromArgb(0xFF, 0xFF, 0xFF, 0x00)),
                ("Permiso", XLColor.FromArgb(0xFF, 0x76, 0x93, 0x3C)),
                ("Fines de Semana", XLColor.FromArgb(0xFF, 0x8D, 0xB4, 0xE2)),
                ("Actividad Recurrente", XLColor.FromArgb(0xFF, 0xCC, 0xC0, 0xDA))
            };

            for (int i = 0; i < leyendas.Length; i++)
            {
                var c = ws.Cell(nomRow + 1 + i, 3);
                c.Value = leyendas[i].Item1;
                c.Style.Fill.BackgroundColor = leyendas[i].Item2;
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Font.FontSize = 9;
                AplicarBorde(c, ReporteEstilos.Borde);
            }

            ws.SheetView.FreezeRows(8);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}