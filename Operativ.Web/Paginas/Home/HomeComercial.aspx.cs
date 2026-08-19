namespace Operativ.Web.Paginas;
public partial class HomeComercial : PaginaSeguraBase
{
    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilComercial }; }
    }
}
