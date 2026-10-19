using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers;
public static class TarjetasXmlHelper
{
    public static List<DatosTarjeta> CargarTarjetasValidas()
    {
        try
        {
            string rutaFisica = HttpContext.Current.Server.MapPath(ConfiguracionAplicacion.RutaXmlTarjetas);

            XmlDocument documento = new XmlDocument();
            documento.Load(rutaFisica);

            List<DatosTarjeta> tarjetas = new List<DatosTarjeta>();

            foreach (XmlNode nodo in documento.SelectNodes("//Tarjeta"))
            {
                tarjetas.Add(new DatosTarjeta
                {
                    NumeroTarjeta = AesHelper.Desencriptar(nodo.SelectSingleNode("Numero").InnerText.Trim()),
                    NombreTitular = AesHelper.Desencriptar(nodo.SelectSingleNode("Titular").InnerText.Trim()),
                    Vencimiento = AesHelper.Desencriptar(nodo.SelectSingleNode("Vencimiento").InnerText.Trim()),
                    CodigoSeguridad = AesHelper.Desencriptar(nodo.SelectSingleNode("CodigoSeguridad").InnerText.Trim())
                });
            }

            return tarjetas;
        }
        catch (Exception)
        {
            throw new OperativException(TipoError.ErrorArchivoTarjetasNoDisponible);
        }
    }
}
