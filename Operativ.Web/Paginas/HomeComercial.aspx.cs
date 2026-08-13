namespace Operativ.Web.Paginas
{
    public partial class HomeComercial : PaginaSeguraBase
    {
        protected override string PerfilRequerido
        {
            get { return NavegacionHelper.PerfilComercial; }
        }
    }
}
