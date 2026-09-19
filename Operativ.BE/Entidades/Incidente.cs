using System;
using Operativ.BE.Enums;

namespace Operativ.BE.Entidades;
public class Incidente
{
    public int IdIncidente { get; set; }

    public string NumeroIncidente { get; set; }

    public int IdActivo { get; set; }

    public string NombreActivo { get; set; }

    public string NumeroSerieActivo { get; set; }

    public string RazonSocialCliente { get; set; }

    public string Descripcion { get; set; }

    public CategoriaIncidente Categoria { get; set; }

    public PrioridadIncidente Prioridad { get; set; }

    public EstadoIncidente Estado { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime? FechaCierre { get; set; }

    public string ComentarioResolucion { get; set; }

    public bool EstaCerrado
    {
        get { return Estado == EstadoIncidente.Cerrado; }
    }
}
