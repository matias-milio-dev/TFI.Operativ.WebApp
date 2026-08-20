using System;
using System.Web;
using System.Xml;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers;
public static class LoginEmergenciaHelper
{
    public static bool ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        XmlDocument documento = CargarDocumento();

        string nombreUsuarioEsperado = LeerNodo(documento, "NombreUsuario");
        string salt = LeerNodo(documento, "Salt");
        string hashAlmacenado = LeerNodo(documento, "HashContrasena");

        if (!string.Equals(nombreUsuario, nombreUsuarioEsperado, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HashHelper.ValidarContrasena(contrasena, salt, hashAlmacenado);
    }

    private static XmlDocument CargarDocumento()
    {
        try
        {
            string rutaVirtual = ConfiguracionAplicacion.RutaXmlEmergencia;
            string rutaFisica = HttpContext.Current.Server.MapPath(rutaVirtual);

            XmlDocument documento = new XmlDocument();
            documento.Load(rutaFisica);
            return documento;
        }
        catch (Exception)
        {
            throw new OperativException(TipoError.ErrorArchivoEmergenciaNoDisponible);
        }
    }

    private static string LeerNodo(XmlDocument documento, string nombreNodo)
    {
        XmlNode nodo = documento.SelectSingleNode("//" + nombreNodo) 
            ?? throw new OperativException(TipoError.ErrorArchivoEmergenciaNoDisponible);
        return nodo.InnerText.Trim();
    }
}
