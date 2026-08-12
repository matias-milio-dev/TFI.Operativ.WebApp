using System.Linq;
using Operativ.Comun;

namespace Operativ.SEC
{
    public static class ValidadorReglasClave
    {
        private const int LongitudMinima = 8;

        public static void Validar(string clave)
        {
            if (string.IsNullOrEmpty(clave)
                || clave.Length < LongitudMinima
                || !clave.Any(char.IsUpper)
                || !clave.Any(char.IsLower)
                || !clave.Any(char.IsDigit))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorClaveNoCumpleRequisitosMinimos);
            }
        }
    }
}
