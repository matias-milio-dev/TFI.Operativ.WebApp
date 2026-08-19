namespace Operativ.Web.Paginas;
public partial class HomeCliente : PaginaSeguraBase
{
    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilCliente }; }
    }
}
