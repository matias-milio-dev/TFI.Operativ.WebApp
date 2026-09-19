using Operativ.BE.Modelos;

namespace Operativ.Web.Paginas;
public static class NavegacionHelper
{
    public const string PerfilWebMaster = NombreFamilia.WebMaster;
    public const string PerfilAdministrador = NombreFamilia.Administrador;
    public const string PerfilComercial = NombreFamilia.Comercial;
    public const string PerfilCliente = NombreFamilia.Cliente;

    public static string ObtenerUrlHome(string nombrePerfil)
    {
        if (nombrePerfil == null)
        {
            return "~/Paginas/Comun/SinFamilia.aspx";
        }

        return nombrePerfil switch
        {
            PerfilWebMaster => "~/Paginas/Home/HomeWebMaster.aspx",
            PerfilAdministrador => "~/Paginas/Home/HomeAdministrador.aspx",
            PerfilComercial => "~/Paginas/Home/HomeComercial.aspx",
            PerfilCliente => "~/Paginas/Home/HomeCliente.aspx",
            _ => "~/Paginas/Usuarios/Login.aspx",
        };
    }
}
