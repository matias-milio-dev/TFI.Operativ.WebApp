using System.Text.RegularExpressions;

namespace Operativ.SEC.Helpers
{
    public static class ClaveHelper
    {
        public static bool EsCompleja(string clave)
        {
            string patron = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$";
            return Regex.IsMatch(clave, patron);
        }
    }
}
