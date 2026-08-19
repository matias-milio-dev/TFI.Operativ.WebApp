using System.Collections.Generic;

namespace Operativ.BE.Composite;
public class UsuarioPatenteHoja : ComponentePermiso
{
    public override List<string> ObtenerNombresPatentes()
    {
        return new List<string> { Nombre };
    }
}
