namespace Operativ.Web.Paginas;
public static class NavegacionHelper
{
    public const string PerfilWebMaster = "WebMaster";
    public const string PerfilAdministrador = "Administrador";
    public const string PerfilComercial = "Comercial";
    public const string PerfilCliente = "Cliente";

    public static string ObtenerUrlHome(string nombrePerfil)
    {
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
