namespace tmr_mobile.Models.Operaciones;

public class ProyectoItem
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Cliente { get; set; }
    public string Estado { get; set; }
    public decimal Presupuesto { get; set; }
    public decimal Horas { get; set; }
    public string Lider { get; set; }
    public int NumeroRecursos { get; set; }
    public string Tipo { get; set; }
    public string FechaInicio { get; set; }
    public string FechaFin { get; set; }

    public string FechaRango =>
        string.IsNullOrWhiteSpace(FechaInicio) && string.IsNullOrWhiteSpace(FechaFin)
            ? string.Empty
            : $"{FechaInicio} - {FechaFin}";

    public ProyectoItem(
        int id,
        string codigo,
        string nombre,
        string cliente,
        string estado,
        decimal presupuesto,
        decimal horas,
        string lider,
        int numeroRecursos,
        string tipo,
        string fechaInicio,
        string fechaFin)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Cliente = cliente;
        Estado = estado;
        Presupuesto = presupuesto;
        Horas = horas;
        Lider = lider;
        NumeroRecursos = numeroRecursos;
        Tipo = tipo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }
}