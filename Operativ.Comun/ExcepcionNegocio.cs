using System;

namespace Operativ.Comun
{
    public class ExcepcionNegocio : Exception
    {
        public string CodigoError { get; }

        public ExcepcionNegocio(string codigoError) : base(codigoError)
        {
            CodigoError = codigoError;
        }

        public ExcepcionNegocio(string codigoError, Exception innerException) : base(codigoError, innerException)
        {
            CodigoError = codigoError;
        }
    }
}
