using System.Text;

namespace tmr_mobile.Services;

public static class ReportService
{
    public static async Task ExportarCsvAsync(string titulo, string[] encabezados, List<string[]> filas, string nombreArchivo)
    {
        var sb = new StringBuilder();
        
        // UTF-8 BOM para que Excel abra correctamente tildes y caracteres especiales
        sb.Append('\uFEFF');
        
        // Título del Reporte
        sb.AppendLine($"\"{titulo}\"");
        sb.AppendLine($"\"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}\"");
        sb.AppendLine();

        // Encabezados
        sb.AppendLine(string.Join(";", encabezados.Select(h => $"\"{h}\"")));

        // Filas
        foreach (var fila in filas)
        {
            sb.AppendLine(string.Join(";", fila.Select(cell => $"\"{cell?.Replace("\"", "\"\"")}\"")));
        }

        var fileName = $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = $"Exportar {titulo}",
            File = new ShareFile(filePath, "text/csv")
        });
    }

    public static async Task ExportarHtmlPdfAsync(string titulo, string[] encabezados, List<string[]> filas, string nombreArchivo)
    {
        var htmlBuilder = new StringBuilder();

        htmlBuilder.AppendLine("<!DOCTYPE html>");
        htmlBuilder.AppendLine("<html><head><meta charset='utf-8'/>");
        htmlBuilder.AppendLine("<style>");
        htmlBuilder.AppendLine("  body { font-family: 'Segoe UI', Helvetica, Arial, sans-serif; margin: 0; padding: 20px; color: #334155; }");
        htmlBuilder.AppendLine("  .header { background-color: #163572; color: #ffffff; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }");
        htmlBuilder.AppendLine("  .header h1 { margin: 0; font-size: 22px; font-weight: bold; }");
        htmlBuilder.AppendLine("  .header p { margin: 4px 0 0 0; font-size: 12px; opacity: 0.8; }");
        htmlBuilder.AppendLine("  table { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 12px; }");
        htmlBuilder.AppendLine("  th { background-color: #163572; color: #ffffff; padding: 10px 8px; text-align: left; font-weight: bold; font-size: 11px; }");
        htmlBuilder.AppendLine("  td { padding: 8px; border-bottom: 1px solid #E2E8F0; }");
        htmlBuilder.AppendLine("  tr:nth-child(even) { background-color: #F8FAFC; }");
        htmlBuilder.AppendLine("  .badge-activo { color: #16A34A; font-weight: bold; }");
        htmlBuilder.AppendLine("  .badge-inactivo { color: #6B7280; font-weight: bold; }");
        htmlBuilder.AppendLine("  .footer { margin-top: 20px; text-align: center; font-size: 10px; color: #94A3B8; border-top: 1px solid #E2E8F0; padding-top: 10px; }");
        htmlBuilder.AppendLine("</style></head><body>");

        // Banner de Cabecera Corporativa (#163572)
        htmlBuilder.AppendLine("<div class='header'>");
        htmlBuilder.AppendLine($"  <h1>TMR — {titulo}</h1>");
        htmlBuilder.AppendLine($"  <p>Generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
        htmlBuilder.AppendLine("</div>");

        // Tabla de datos
        htmlBuilder.AppendLine("<table><thead><tr>");
        foreach (var col in encabezados)
        {
            htmlBuilder.AppendLine($"  <th>{col}</th>");
        }
        htmlBuilder.AppendLine("</tr></thead><tbody>");

        foreach (var fila in filas)
        {
            htmlBuilder.AppendLine("<tr>");
            for (int i = 0; i < fila.Length; i++)
            {
                var val = fila[i] ?? "-";
                if (i == fila.Length - 1) // Última columna (Estado)
                {
                    var esActivo = val.Equals("Activo", StringComparison.OrdinalIgnoreCase);
                    var cssClass = esActivo ? "badge-activo" : "badge-inactivo";
                    htmlBuilder.AppendLine($"  <td class='{cssClass}'>{val}</td>");
                }
                else
                {
                    htmlBuilder.AppendLine($"  <td>{val}</td>");
                }
            }
            htmlBuilder.AppendLine("</tr>");
        }

        htmlBuilder.AppendLine("</tbody></table>");
        htmlBuilder.AppendLine("<div class='footer'>Integrity Solutions — TMR Móvil</div>");
        htmlBuilder.AppendLine("</body></html>");

        var fileName = $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmm}.html";
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

        await File.WriteAllTextAsync(filePath, htmlBuilder.ToString(), Encoding.UTF8);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = $"Exportar {titulo}",
            File = new ShareFile(filePath, "text/html")
        });
    }
}
