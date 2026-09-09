using System;
using System.IO;
using System.Web.UI.WebControls;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class BackupRestore : PaginaSeguraBase
{
    private readonly IBackupService backupService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup }; }
    }

    public BackupRestore()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        backupService = fabricaSeguridad.CrearBackupService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarGrilla();
        }
    }

    protected override void AplicarVisibilidadPorPatentes()
    {
        btnCrearBackup.Visible = AutorizacionHandler.TienePatente(NombrePatente.RealizarBackup);
        btnRestaurarDesdeArchivo.Visible = AutorizacionHandler.TienePatente(NombrePatente.RestaurarBackup);
    }

    protected void btnCrearBackup_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.RealizarBackup))
        {
            return;
        }

        try
        {
            string nombreArchivo = backupService.CrearBackup();
            string formato = (string)GetGlobalResourceObject("Textos", "MensajeExitoCrearBackup");
            ControlNotificaciones.MostrarMensaje(string.Format(formato, nombreArchivo), true);
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnRestaurarDesdeArchivo_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.RestaurarBackup))
        {
            return;
        }

        if (!fileuploadRestaurar.HasFile)
        {
            string mensaje = (string)GetGlobalResourceObject("Textos", "MensajeArchivoRestaurarObligatorio");
            ControlNotificaciones.MostrarMensaje(mensaje, false);
            return;
        }

        string rutaTemporal = backupService.PrepararRutaParaSubida();

        try
        {
            fileuploadRestaurar.SaveAs(rutaTemporal);
            backupService.RestaurarBackupDesdeRuta(rutaTemporal);

            SesionHandler.CerrarSesion();
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?restaurado=1");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }
        }
    }

    protected void gvBackups_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string nombreArchivo = e.CommandArgument.ToString();

        if (e.CommandName == "Descargar")
        {
            DescargarArchivo(nombreArchivo);
            return;
        }

        if (e.CommandName != "Restaurar")
        {
            return;
        }

        if (!ValidarPatente(NombrePatente.RestaurarBackup))
        {
            return;
        }

        try
        {
            backupService.RestaurarBackup(nombreArchivo);

            SesionHandler.CerrarSesion();
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?restaurado=1");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void gvBackups_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
        {
            return;
        }

        LinkButton lnkRestaurar = (LinkButton)e.Row.FindControl("lnkRestaurar");
        lnkRestaurar.Visible = AutorizacionHandler.TienePatente(NombrePatente.RestaurarBackup);
    }

    private void DescargarArchivo(string nombreArchivo)
    {
        string rutaCompleta = backupService.ObtenerRutaCompleta(nombreArchivo);

        if (!File.Exists(rutaCompleta))
        {
            ControlNotificaciones.MostrarMensaje(TipoError.ErrorArchivoBackupNoExiste, new string[] { nombreArchivo });
            return;
        }

        Response.Clear();
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);
        Response.TransmitFile(rutaCompleta);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }

    private bool ValidarPatente(string nombrePatente)
    {
        if (AutorizacionHandler.TienePatente(nombrePatente))
        {
            return true;
        }

        ControlNotificaciones.MostrarMensaje(TipoError.ErrorSinPermiso, new string[] { nombrePatente });
        return false;
    }

    private void CargarGrilla()
    {
        gvBackups.DataSource = backupService.ListarBackups();
        gvBackups.DataBind();
    }
}
