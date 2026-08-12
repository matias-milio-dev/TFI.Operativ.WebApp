using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Operativ.Comun
{
    public static class TextoHelper
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("Operativ.Comun.Recursos.Mensajes", Assembly.GetExecutingAssembly());

        public static string Resolver(string clave, string idioma)
        {
            CultureInfo cultura = string.IsNullOrEmpty(idioma) ? CultureInfo.CurrentUICulture : new CultureInfo(idioma);
            return _resourceManager.GetString(clave, cultura) ?? clave;
        }
    }
}
