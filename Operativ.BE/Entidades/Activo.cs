using Operativ.BE.Enums;

namespace Operativ.BE.Entidades;
public class Activo
{
    public int IdActivo { get; set; }

    public string Nombre { get; set; }

    public string Modelo { get; set; }

    public string NumeroSerie { get; set; }

    public string Especificaciones { get; set; }

    public int IdPaquete { get; set; }

    public string NombrePaquete { get; set; }

    public EstadoActivo Estado { get; set; }

    public bool Habilitado { get; set; }
}
