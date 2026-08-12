using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Operativ.Comun
{
    public static class ManejadorErrores
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("Operativ.Comun.Recursos.Mensajes", Assembly.GetExecutingAssembly());

        public static MensajeError Resolver(string codigoError)
        {
            return Resolver(codigoError, Thread.CurrentThread.CurrentUICulture);
        }

        public static MensajeError Resolver(string codigoError, CultureInfo cultura)
        {
            string texto = _resourceManager.GetString(codigoError, cultura)
                            ?? _resourceManager.GetString(CodigosError.ErrorInesperadoSistema, cultura)
                            ?? "Error del sistema.";

            return new MensajeError
            {
                CodigoError = codigoError,
                Texto = texto,
                Tipo = CodigosError.ObtenerCriticidad(codigoError)
            };
        }

        public static MensajeError ResolverExcepcion(Exception excepcion)
        {
            if (excepcion is ExcepcionNegocio excepcionNegocio)
            {
                return Resolver(excepcionNegocio.CodigoError);
            }
            return Resolver(CodigosError.ErrorInesperadoSistema);
        }
    }

    public class MensajeError
    {
        public string CodigoError { get; set; }
        public string Texto { get; set; }
        public TipoCriticidadError Tipo { get; set; }
    }
}
