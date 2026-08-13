namespace Operativ.Web.Paginas
{
    public partial class HomeAdministrador : PaginaSeguraBase
    {
        protected override string PerfilRequerido
        {
            get { return NavegacionHelper.PerfilAdministrador; }
        }
    }
}
