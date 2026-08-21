namespace tmr_mobile.Resources.Styles;

/// <summary>
/// Resuelve las variantes claras y oscuras de los iconos entregados por los menús.
/// </summary>
public static class ThemeImages
{
    private static readonly HashSet<string> IconsWithDarkVariant = new(StringComparer.OrdinalIgnoreCase)
    {
        "catalogo.png", "configura.png", "direccion.png", "feriado.png", "homes.png",
        "icon_carga.png", "icon_clientes.png", "icon_colaboradores.png", "icon_home.png", "icon_lideres.png",
        "icon_proyectos.png", "icon_pulenes.png", "icon_reportes.png", "icon_seguimiento.png",
        "icon_timereport.png", "key_icon.png", "logout_icon.png",
        "notification_bell_alarm.png", "rol.png", "user_profile.png"
    };

    public static string DarkVariant(string icon)
    {
        if (string.IsNullOrWhiteSpace(icon) || !IconsWithDarkVariant.Contains(icon))
            return icon;

        var extension = Path.GetExtension(icon);
        return $"{Path.GetFileNameWithoutExtension(icon)}_dark{extension}";
    }
}
