using Operativ.BE.Entidades;
using Operativ.BE.Modelos.Composite;

namespace Operativ.BE.Modelos;
public class ResultadoAutenticacion
{
    public Usuario Usuario { get; set; }

    public Familia Perfil { get; set; }

    public FamiliaCompuesto ArbolPermisos { get; set; }

    public string SufijoRedireccion { get; set; } = string.Empty;
}
