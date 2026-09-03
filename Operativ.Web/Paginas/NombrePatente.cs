namespace Operativ.Web.Paginas;
public static class NombrePatente
{
    public const string ConsultarUsuario = "ConsultarUsuario";
    public const string AltaUsuario = "AltaUsuario";
    public const string BajaUsuario = "BajaUsuario";
    public const string ModificacionUsuario = "ModificacionUsuario";
    public const string DesbloqueoUsuario = "DesbloqueoUsuario";
    public const string BloqueoUsuario = "BloqueoUsuario";
    public const string AsignarPatente = "AsignarPatente";
    public const string RemoverPatente = "RemoverPatente";

    public static readonly string[] ModuloUsuarios =
    {
        ConsultarUsuario,
        AltaUsuario,
        BajaUsuario,
        ModificacionUsuario,
        DesbloqueoUsuario,
        BloqueoUsuario,
        AsignarPatente,
        RemoverPatente
    };

    public static readonly string[] ModuloPermisos =
    {
        AsignarPatente,
        RemoverPatente
    };
}
