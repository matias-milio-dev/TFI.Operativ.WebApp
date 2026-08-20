using System;
using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.SEC.Helpers;
using Operativ.Web.Paginas;

namespace Operativ.Web;

//Login.aspx.cs hereda de pagina base la cual contiene la cultura actual de la pagina 
//Necesaria para determinar el idioma de las aplicacion.
public partial class Login : PaginaBase
{
    //Declaracion de miembros de clase:
    //Interfaces de los servicios de la capa SEC, manejador de session de usuario 
    //Y flag de modo emergencia.
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;
    private bool modoEmergencia;

    //Constructor inicializando los servicios usando la el patron Factory Method,
    //Donde se utiliza una implementacion de la interfaz de servicio    
    public Login()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }
    
    //Durante el evento Page Load, si hay una sesion activa y no es postback
    //Se redirecciona al login correspondiente al perfil.
    //Se llama al metodo privado de esta clase Verificar integridad sistema.
    //Se maneja la session expirada chequeando el querystring err.
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo.Nombre));
        }
        VerificarIntegridadSistema();
        if (!IsPostBack && Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(TipoError.ErrorSesionExpirada);
        }
    }
    
    //En el evento click del btnIngresar validamos que no este en modo emergencia.
    //En un bloque try-catch, obteneemos el usuario validado con el metodo de servicio ValidarCredenciales)
    //El cual le pasamos como parametro el texto de los controles txtNombreUsuario recortado con Trim y la contrasena.
    //Luego cargamos la familia del usuario y los permisos asociados a esa familia.
    //Finalmente guardamos estos datos en el manejador de la session y redireccionamos al home que corresponde al perfil.
    //En caso de ocurrir una excepcion como por ejemplo un login no exitoso, o una falla en la base de datos
    //El control de usuario ucNotificaciones contiene un metodo MostrarMensaje que mapea la excepcion a un mensaje localizado y lo muestra.
    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (modoEmergencia || !Page.IsValid)
        {
            return;
        }

        try
        {
            Usuario usuario = usuarioService.ValidarCredenciales(
                txtNombreUsuario.Text.Trim(),
                txtContrasena.Text);

            Familia perfil = familiaService.GetPerfilDeUsuario(usuario.IdUsuario);
            FamiliaCompuesto arbolPermisos = familiaService.ArmarArbolPermisos(usuario.IdUsuario);

            sesionHandler.IniciarSesion(usuario, perfil, arbolPermisos);

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfil.Nombre), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }

    //Aqui se vincula el evento click del btnIngresoEmergencia de estar visible por un fallo de integridad.
    //El cual invoca al LoginEmergenciaHelper donde busca en una fuente alternativa (XML) el login de emergencia para WebMaster.
    //Si las credenciales son validas, se repara la base de datos.
    //Se registra en bitacora con id de usuario null por que no existe el usuario de emergencia en la base de datos real.
    //Se inicia sesion en el manejador de la session con los datos previsionales de usuario.
    //Se pasa al home del webmaster pasando un querystring indicando que hay que reparar la base datos.
    protected void btnIngresoEmergencia_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            bool credencialesValidas = LoginEmergenciaHelper.ValidarCredenciales(
                txtUsuarioEmergencia.Text.Trim(), txtContrasenaEmergencia.Text);

            if (!credencialesValidas)
            {
                throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
            }

            integridadService.RepararBaseDatos();
            bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);

            Usuario usuarioEmergencia = new Usuario
            {
                IdUsuario = 0,
                NombreUsuario = txtUsuarioEmergencia.Text.Trim(),
                NombreCompleto = "Web Master (acceso de emergencia)",
                Activo = true,
                Bloqueado = false
            };

            Familia perfilEmergencia = new Familia
            {
                IdFamilia = 0,
                Nombre = "WebMaster",
                Descripcion = "Acceso de emergencia"
            };

            sesionHandler.IniciarSesion(usuarioEmergencia, perfilEmergencia, new FamiliaCompuesto());

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilEmergencia.Nombre) + "?reparado=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }

    //Este metodo privado verifica la integridad del sistema con el metodo VerificarIntegridad del servicio de integridad.
    //Este devuelve una lista de tipo ResultadoVerificacionTabla que contiene, de haber, las filas con inconsistencias 
    //De haber el flag modo emergencia se setea en true, se preparar de manera legible los errores con el metodo FormatearResumenFallas
    //Se muestra el mensaje en el control de usuario de notificaciones, graba en bitacora y pone en false la visibilidad del login normal.
    //De haber algun tipo de excepcion se pone en true el flag el modo emergencia tambien.
    private void VerificarIntegridadSistema()
    {
        try
        {
            List<ResultadoVerificacionTabla> resultadosInvalidos = integridadService.VerificarIntegridad();
            modoEmergencia = resultadosInvalidos.Count > 0;
            if (modoEmergencia)
            {
                string detalle = integridadService.FormatearResumenFallas(resultadosInvalidos);
                ucNotificaciones.MostrarMensaje(TipoError.ErrorIntegridadCorrupta, new string[] { detalle });
                bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, detalle);
                pnlLoginNormal.Visible = false;
                pnlAccesoEmergencia.Visible = true;
            }
        }
        catch (Exception excepcion)
        {
            bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, excepcion.Message);
            modoEmergencia = true;
            pnlLoginNormal.Visible = false;
            pnlAccesoEmergencia.Visible = true;
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }
}
