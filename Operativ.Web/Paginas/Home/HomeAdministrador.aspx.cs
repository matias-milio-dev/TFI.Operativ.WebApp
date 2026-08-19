namespace Operativ.Web.Paginas;
public partial class HomeAdministrador : PaginaSeguraBase
{
    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilAdministrador }; }
    }
}
