using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web.Paginas
{
    public partial class Administracion : PaginaBase
    {
        protected override string PatenteRequerida => "BASEDATOS_REPARAR";

        private readonly ISistemaBLL _sistemaBLL = FabricaBLL.Instancia.CrearSistemaBLL();

        protected Literal litTitulo;
        protected GridView gvIntegridad;
        protected TextBox txtRutaBackup;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuAdministracion");
            }
        }

        protected void btnVerificar_Click(object sender, EventArgs e)
        {
            EjecutarConManejoDeErrores(() =>
            {
                gvIntegridad.DataSource = _sistemaBLL.VerificarIntegridad();
                gvIntegridad.DataBind();
            });
        }

        protected void btnReparar_Click(object sender, EventArgs e)
        {
            EjecutarConManejoDeErrores(() =>
            {
                gvIntegridad.DataSource = _sistemaBLL.RepararBaseDatos();
                gvIntegridad.DataBind();
                ((Master.SiteMaster)Master).MostrarExito("Reparación de integridad ejecutada correctamente.");
            });
        }

        protected void btnBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRutaBackup.Text))
            {
                ((Master.SiteMaster)Master).MostrarMensaje(CodigosError.ErrorRutaBackupInvalida);
                return;
            }

            EjecutarConManejoDeErrores(() =>
            {
                GestorAutorizacion.RequerirPatente("BASEDATOS_BACKUP");
                _sistemaBLL.RealizarBackup(txtRutaBackup.Text.Trim());
                ((Master.SiteMaster)Master).MostrarExito("Backup generado correctamente.");
            });
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRutaBackup.Text))
            {
                ((Master.SiteMaster)Master).MostrarMensaje(CodigosError.ErrorRutaRestoreInvalida);
                return;
            }

            EjecutarConManejoDeErrores(() =>
            {
                GestorAutorizacion.RequerirPatente("BASEDATOS_BACKUP");
                _sistemaBLL.RealizarRestore(txtRutaBackup.Text.Trim());
                ((Master.SiteMaster)Master).MostrarExito("Restore ejecutado correctamente.");
            });
        }

        private void EjecutarConManejoDeErrores(Action accion)
        {
            try
            {
                accion();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}
