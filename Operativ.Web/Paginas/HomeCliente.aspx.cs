namespace Operativ.Web.Paginas
{
    public partial class HomeCliente : PaginaSeguraBase
    {
        protected override string PerfilRequerido
        {
            get { return NavegacionHelper.PerfilCliente; }
        }
    }
}
