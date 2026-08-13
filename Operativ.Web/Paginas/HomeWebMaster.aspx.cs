namespace Operativ.Web.Paginas
{
    public partial class HomeWebMaster : PaginaSeguraBase
    {
        protected override string PerfilRequerido
        {
            get { return NavegacionHelper.PerfilWebMaster; }
        }
    }
}
