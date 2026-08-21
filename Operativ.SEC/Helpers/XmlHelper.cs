using System;
using System.Web;
using System.Xml;
using Operativ.BE.Enums;
using Operativ.BE.Errores;

namespace Operativ.SEC.Helpers;
public static class XmlHelper
{
    public static XmlDocument CargarDocumento(string rutaVirtual)
    {
        try
        {
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

    public static string LeerNodo(XmlDocument documento, string nombreNodo)
    {
        XmlNode nodo = documento.SelectSingleNode("//" + nombreNodo)
            ?? throw new OperativException(TipoError.ErrorArchivoEmergenciaNoDisponible);
        return nodo.InnerText.Trim();
    }
}
