using System.Collections.Generic;

namespace Operativ.BE.Modelos.Composite;
public class UsuarioPatenteHoja : ComponentePermiso
{
    public override List<string> ObtenerNombresPatentes()
    {
        return new List<string> { Nombre };
    }
}
