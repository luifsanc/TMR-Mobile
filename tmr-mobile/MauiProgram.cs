using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

using tmr_mobile.Services;
using tmr_mobile.ViewModels;
using tmr_mobile.Views.Auth;
using tmr_mobile.Views.Dashboard;
using tmr_mobile.Views.TimeReport;
using tmr_mobile.Views.Operaciones;
using tmr_mobile.Views.Reportes;
using tmr_mobile.Views.Configuracion;

#if ANDROID
using Android.Content.Res;
#endif

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
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular"
                );

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold"
                );
            });

        // ─────────────────────────────────────────────────────────────
        // SERVICIOS
        // ─────────────────────────────────────────────────────────────

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();


        // ─────────────────────────────────────────────────────────────
        // VIEWMODELS
        // ─────────────────────────────────────────────────────────────

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
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


        // ─────────────────────────────────────────────────────────────
        // VISTAS
        // ─────────────────────────────────────────────────────────────

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
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


        // ─────────────────────────────────────────────────────────────
        // ENTRY SIN BORDE NATIVO
        // ─────────────────────────────────────────────────────────────
        //
        // El borde visible se controla desde LoginPage.xaml
        // mediante <Border>.
        //
        // Aquí quitamos el estilo nativo del Entry para evitar
        // doble borde y línea inferior.
        //
        // ─────────────────────────────────────────────────────────────

        EntryHandler.Mapper.AppendToMapping(
            "BorderlessEntry",
            (handler, view) =>
            {
#if ANDROID

                handler.PlatformView.Background = null;

                handler.PlatformView.SetBackgroundColor(
                    Android.Graphics.Color.Transparent
                );

                handler.PlatformView.BackgroundTintList =
                    ColorStateList.ValueOf(
                        Android.Graphics.Color.Transparent
                    );

                handler.PlatformView.SetPadding(
                    0,
                    0,
                    0,
                    0
                );

#elif WINDOWS

                handler.PlatformView.BorderThickness =
                    new Microsoft.UI.Xaml.Thickness(0);

                handler.PlatformView.Padding =
                    new Microsoft.UI.Xaml.Thickness(0);

                handler.PlatformView.BorderBrush =
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Transparent
                    );

                handler.PlatformView.Background =
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Transparent
                    );

#endif
            });


        // ─────────────────────────────────────────────────────────────
        // DEBUG
        // ─────────────────────────────────────────────────────────────

#if DEBUG
        builder.Logging.AddDebug();
#endif


        return builder.Build();
    }
}