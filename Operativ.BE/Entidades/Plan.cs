namespace Operativ.BE.Entidades;
public class Plan
{
    public int IdPlan { get; set; }

    public string Nombre { get; set; }

    public string Descripcion { get; set; }

    public decimal PrecioAnual { get; set; }

    public bool Activo { get; set; }
}
