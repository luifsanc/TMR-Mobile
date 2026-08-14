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
using tmr_mobile.Views.Home;
using tmr_mobile.Views;
using tmr_mobile.Views.Seguimiento;


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
                    "Inter-Regular.ttf",
                    "InterRegular"
                );

                fonts.AddFont(
                    "Inter-SemiBold.ttf",
                    "InterSemiBold"
                );
            });

        // ─────────────────────────────────────────────────────────────
        // SERVICIOS
        // ─────────────────────────────────────────────────────────────

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IConfirmDialogService, ConfirmDialogService>();
        builder.Services.AddSingleton<ICatalogosService, CatalogosService>();
        builder.Services.AddSingleton<IClientesService, ClientesService>();
        builder.Services.AddSingleton<IColaboradoresService, ColaboradoresService>();
        builder.Services.AddSingleton<ICargaActividadesService, CargaActividadesService>();
        builder.Services.AddSingleton<IUsuariosService, UsuariosService>();
        builder.Services.AddSingleton<IRolesService, RolesService>();
        builder.Services.AddSingleton<IFeriadosService, FeriadosService>();
        builder.Services.AddSingleton<ISeguimientoService, SeguimientoService>();
        builder.Services.AddSingleton<ExcelExportService>();


        // ─────────────────────────────────────────────────────────────
        // VIEWMODELS
        // ─────────────────────────────────────────────────────────────

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TimeReportViewModel>();
        builder.Services.AddTransient<CrearActividadViewModel>();
        builder.Services.AddTransient<CargaActividadesViewModel>();
        builder.Services.AddTransient<NotificacionesViewModel>();
        builder.Services.AddTransient<ProyectosViewModel>();
        builder.Services.AddTransient<ProyectosFormViewModel>();
        builder.Services.AddTransient<ColaboradoresViewModel>();
        builder.Services.AddTransient<ClientesViewModel>();
        builder.Services.AddTransient<LideresViewModel>();
        builder.Services.AddTransient<ReporteHorasViewModel>();
        builder.Services.AddTransient<ReporteFechasViewModel>();
        builder.Services.AddTransient<UsuariosConfigViewModel>();
        builder.Services.AddTransient<UsuarioDetalleViewModel>();
        builder.Services.AddTransient<UsuarioFormViewModel>();
        builder.Services.AddTransient<RolesConfigViewModel>();
        builder.Services.AddTransient<RolDetalleViewModel>();
        builder.Services.AddTransient<RolFormViewModel>();
        builder.Services.AddTransient<FeriadosConfigViewModel>();
        builder.Services.AddTransient<FeriadoDetalleViewModel>();
        builder.Services.AddTransient<FeriadoFormViewModel>();
        builder.Services.AddTransient<CatalogosConfigViewModel>();
        builder.Services.AddTransient<ConfiguracionViewModel>();
        builder.Services.AddTransient<CatalogoDetalleViewModel>();
        builder.Services.AddTransient<CatalogoFormViewModel>();
        builder.Services.AddTransient<ClienteDetalleViewModel>();
        builder.Services.AddTransient<ClienteFormViewModel>();
        builder.Services.AddTransient<ColaboradorDetalleViewModel>();
        builder.Services.AddTransient<ColaboradorSalidaViewModel>();
        builder.Services.AddTransient<ColaboradorFormViewModel>();
        builder.Services.AddTransient<EventosViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<NoticiasViewModel>();
        builder.Services.AddTransient<ReportesViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ChangePasswordViewModel>();
        builder.Services.AddTransient<SeguimientoViewModel>();
        builder.Services.AddTransient<SeguimientoDetalleViewModel>();


        // ─────────────────────────────────────────────────────────────
        // VISTAS
        // ─────────────────────────────────────────────────────────────

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TimeReportPage>();
        builder.Services.AddTransient<CrearActividadPage>();
        builder.Services.AddTransient<CargaActividadesPage>();
        builder.Services.AddTransient<NotificacionesPage>();
        builder.Services.AddTransient<ProyectosPage>();
        builder.Services.AddTransient<ProyectosFormPage>();
        builder.Services.AddTransient<ColaboradoresPage>();
        builder.Services.AddTransient<ClientesPage>();
        builder.Services.AddTransient<LideresPage>();
        builder.Services.AddTransient<ReporteHorasPage>();
        builder.Services.AddTransient<ReporteFechasPage>();
        builder.Services.AddTransient<UsuariosPage>();
        builder.Services.AddTransient<UsuarioDetallePage>();
        builder.Services.AddTransient<UsuarioFormPage>();
        builder.Services.AddTransient<RolesPage>();
        builder.Services.AddTransient<RolDetallePage>();
        builder.Services.AddTransient<RolFormPage>();
        builder.Services.AddTransient<FeriadosPage>();
        builder.Services.AddTransient<FeriadoDetallePage>();
        builder.Services.AddTransient<FeriadoFormPage>();
        builder.Services.AddTransient<ConfiguracionPage>();
        builder.Services.AddTransient<CatalogosPage>();
        builder.Services.AddTransient<CatalogoDetallePage>();
        builder.Services.AddTransient<CatalogoFormPage>();
        builder.Services.AddTransient<ClienteDetallePage>();
        builder.Services.AddTransient<ClienteFormPage>();
        builder.Services.AddTransient<ColaboradorDetallePage>();
        builder.Services.AddTransient<ColaboradorSalidaPage>();
        builder.Services.AddTransient<ColaboradorFormPage>();
        builder.Services.AddTransient<EventosPage>();
        builder.Services.AddTransient<NoticiasPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ReportesPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ChangePasswordPage>();
        builder.Services.AddTransient<SeguimientoPage>();
        builder.Services.AddTransient<SeguimientoDetallePage>();


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

        DatePickerHandler.Mapper.AppendToMapping(
            "CapitalizedPlaceholderDatePicker",
            (handler, view) =>
            {
#if WINDOWS
                if (handler.PlatformView is Microsoft.UI.Xaml.Controls.CalendarDatePicker datePicker)
                {
                    datePicker.PlaceholderText = "Seleccionar una fecha";
                }
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
