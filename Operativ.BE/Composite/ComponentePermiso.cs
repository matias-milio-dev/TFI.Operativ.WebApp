using System.Collections.Generic;

namespace Operativ.BE.Composite;
public abstract class ComponentePermiso
{
    public int Id { get; set; }

    public string Nombre { get; set; }

    public abstract List<string> ObtenerNombresPatentes();
}
