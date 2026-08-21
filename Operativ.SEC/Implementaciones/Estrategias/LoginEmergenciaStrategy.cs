using System;
using System.Xml;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BE.Modelos.Composite;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones.Estrategias;
public class LoginEmergenciaStrategy : ILoginStrategy
{
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;

    public LoginEmergenciaStrategy()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public ResultadoAutenticacion Autenticar(string nombreUsuario, string contrasena)
    {
        if (!ValidarCredenciales(nombreUsuario, contrasena))
        {
            throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
        }

        integridadService.RepararBaseDatos();

        bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);

        Usuario usuarioEmergencia = new Usuario
        {
            IdUsuario = 0,
            NombreUsuario = nombreUsuario,
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

        return new ResultadoAutenticacion
        {
            Usuario = usuarioEmergencia,
            Perfil = perfilEmergencia,
            ArbolPermisos = new FamiliaCompuesto(),
            SufijoRedireccion = "?reparado=1"
        };
    }

    private bool ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        XmlDocument documento = XmlHelper.CargarDocumento(ConfiguracionAplicacion.RutaXmlEmergencia);

        string nombreUsuarioEsperado = XmlHelper.LeerNodo(documento, "NombreUsuario");
        string salt = XmlHelper.LeerNodo(documento, "Salt");
        string hashAlmacenado = XmlHelper.LeerNodo(documento, "HashContrasena");

        if (!string.Equals(nombreUsuario, nombreUsuarioEsperado, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HashHelper.ValidarContrasena(contrasena, salt, hashAlmacenado);
    }
}
