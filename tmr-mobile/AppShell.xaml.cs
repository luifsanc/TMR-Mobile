using tmr_mobile.Views.Dashboard;
using tmr_mobile.Views.Operaciones;
using tmr_mobile.Views.TimeReport;
using tmr_mobile.Views.Reportes;

namespace tmr_mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		Routing.RegisterRoute("proyectos", typeof(ProyectosPage));
		Routing.RegisterRoute("timereport", typeof(TimeReportPage));
		Routing.RegisterRoute("carga", typeof(CargaActividadesPage));
		Routing.RegisterRoute("reportes", typeof(ReportesPage));
		Routing.RegisterRoute("lideres", typeof(LideresPage));
		Routing.RegisterRoute("colaboradores", typeof(ColaboradoresPage));
		Routing.RegisterRoute("clientes", typeof(ClientesPage));
	}
}
