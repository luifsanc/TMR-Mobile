using Microsoft.Extensions.Logging;
using tmr_mobile.Services;
using tmr_mobile.ViewModels;
using tmr_mobile.Views.Auth;
using tmr_mobile.Views.Dashboard;
using tmr_mobile.Views.TimeReport;
using tmr_mobile.Views.Operaciones;
using tmr_mobile.Views.Reportes;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Servicios ──
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // ── ViewModels ──
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TimeReportViewModel>();
        builder.Services.AddTransient<CargaActividadesViewModel>();
        builder.Services.AddTransient<ProyectosViewModel>();
        builder.Services.AddTransient<ColaboradoresViewModel>();
        builder.Services.AddTransient<ClientesViewModel>();
        builder.Services.AddTransient<LideresViewModel>();
        builder.Services.AddTransient<ReporteHorasViewModel>();
        builder.Services.AddTransient<ReporteFechasViewModel>();
        builder.Services.AddTransient<UsuariosConfigViewModel>();
        builder.Services.AddTransient<RolesConfigViewModel>();
        builder.Services.AddTransient<FeriadosConfigViewModel>();
        builder.Services.AddTransient<CatalogosConfigViewModel>();

        // ── Vistas ──
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TimeReportPage>();
        builder.Services.AddTransient<CargaActividadesPage>();
        builder.Services.AddTransient<ProyectosPage>();
        builder.Services.AddTransient<ColaboradoresPage>();
        builder.Services.AddTransient<ClientesPage>();
        builder.Services.AddTransient<LideresPage>();
        builder.Services.AddTransient<ReporteHorasPage>();
        builder.Services.AddTransient<ReporteFechasPage>();
        builder.Services.AddTransient<UsuariosPage>();
        builder.Services.AddTransient<RolesPage>();
        builder.Services.AddTransient<FeriadosPage>();
        builder.Services.AddTransient<CatalogosPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
