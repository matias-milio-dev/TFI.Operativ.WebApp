namespace Operativ.Web.Paginas;
public static class CategoriaPatente
{
    public const string Usuarios = "Usuarios";
    public const string Familias = "Familias";
    public const string Clientes = "Clientes";
    public const string Catalogo = "Catalogo";
    public const string Suscripciones = "Suscripciones";
    public const string Facturacion = "Facturacion";
    public const string Incidentes = "Incidentes";
    public const string Sistema = "Sistema";

    public static readonly string[] Orden =
    {
        Usuarios,
        Familias,
        Clientes,
        Catalogo,
        Suscripciones,
        Facturacion,
        Incidentes,
        Sistema
    };

    public static string Obtener(string nombrePatente)
    {
        switch (nombrePatente)
        {
            case NombrePatente.ConsultarUsuario:
            case NombrePatente.AltaUsuario:
            case NombrePatente.BajaUsuario:
            case NombrePatente.ModificacionUsuario:
            case NombrePatente.DesbloqueoUsuario:
            case NombrePatente.BloqueoUsuario:
            case NombrePatente.AsignarPatente:
            case NombrePatente.RemoverPatente:
                return Usuarios;
            case "GestionarFamilias":
                return Familias;
            case "GestionarClientes":
                return Clientes;
            case "GestionarCatalogo":
                return Catalogo;
            case "GestionarSuscripciones":
                return Suscripciones;
            case "ConsultarFacturas":
                return Facturacion;
            case "ReportarIncidentes":
                return Incidentes;
            default:
                return Sistema;
        }
    }
}
