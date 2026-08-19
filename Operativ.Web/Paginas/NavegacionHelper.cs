namespace Operativ.Web.Paginas;
public static class NavegacionHelper
{
    public const string PerfilWebMaster = "WebMaster";
    public const string PerfilAdministrador = "Administrador";
    public const string PerfilComercial = "Comercial";
    public const string PerfilCliente = "Cliente";

    public static string ObtenerUrlHome(string nombrePerfil)
    {
        if (nombrePerfil == PerfilWebMaster)
        {
            return "~/Paginas/Home/HomeWebMaster.aspx";
        }

        if (nombrePerfil == PerfilAdministrador)
        {
            return "~/Paginas/Home/HomeAdministrador.aspx";
        }

        if (nombrePerfil == PerfilComercial)
        {
            return "~/Paginas/Home/HomeComercial.aspx";
        }

        if (nombrePerfil == PerfilCliente)
        {
            return "~/Paginas/Home/HomeCliente.aspx";
        }

        return "~/Paginas/Usuarios/Login.aspx";
    }
}
