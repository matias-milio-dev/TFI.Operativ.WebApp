using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Entidades;
public class Paquete
{
    public int IdPaquete { get; set; }

    public string Nombre { get; set; }

    public string Descripcion { get; set; }

    public TipoPermiso TipoPermiso { get; set; }

    public bool Activo { get; set; }

    public List<Programa> Programas { get; set; }

    public int CantidadProgramas
    {
        get { return Programas.Count; }
    }

    public Paquete()
    {
        Programas = new List<Programa>();
    }
}