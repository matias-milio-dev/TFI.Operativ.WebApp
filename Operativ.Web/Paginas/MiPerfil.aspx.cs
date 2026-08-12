using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.SEC;

namespace Operativ.Web.Paginas
{
    public partial class MiPerfil : PaginaBase
    {
        protected Literal litUsuario;
        protected Literal litNombreCompleto;
        protected Literal litCorreo;
        protected Literal litPerfil;

        protected void Page_Load(object sender, EventArgs e)
        {
            var usuario = ContextoSesion.Actual.UsuarioActual;
            litUsuario.Text = usuario.NombreUsuario;
            litNombreCompleto.Text = usuario.NombreCompleto;
            litCorreo.Text = usuario.CorreoElectronico;
            litPerfil.Text = usuario.CodigoPerfil;
        }
    }
}
