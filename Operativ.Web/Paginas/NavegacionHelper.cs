namespace Operativ.Web.Paginas
{
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
                return "~/Paginas/HomeWebMaster.aspx";
            }

            if (nombrePerfil == PerfilAdministrador)
            {
                return "~/Paginas/HomeAdministrador.aspx";
            }

            if (nombrePerfil == PerfilComercial)
            {
                return "~/Paginas/HomeComercial.aspx";
            }

            if (nombrePerfil == PerfilCliente)
            {
                return "~/Paginas/HomeCliente.aspx";
            }

            return "~/Login.aspx";
        }
    }
}
