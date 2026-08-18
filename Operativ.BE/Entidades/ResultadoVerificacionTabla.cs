using System.Collections.Generic;

namespace Operativ.BE.Entidades
{
    public class ResultadoVerificacionTabla
    {
        public string NombreTabla { get; set; }

        public bool Integra { get; set; }

        public long ValorDvvAlmacenado { get; set; }

        public long ValorDvvCalculado { get; set; }

        public List<string> ClavesFilasInvalidas { get; set; }

        public ResultadoVerificacionTabla()
        {
            ClavesFilasInvalidas = new List<string>();
        }
    }
}
