using System;
using Operativ.BE.Enums;

namespace Operativ.BE.Errores;
public class OperativException : Exception
{
    public TipoError TipoError { get; private set; }

    public string[] Parametros { get; private set; }

    public OperativException(TipoError tipoError)
        : this(tipoError, null)
    {
    }

    public OperativException(TipoError tipoError, string[] parametros)
        : base(tipoError.ToString())
    {
        TipoError = tipoError;
        Parametros = parametros;
    }
}
