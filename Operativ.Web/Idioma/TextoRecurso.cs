using System.Web;

namespace Operativ.Web.Idioma;
public static class TextoRecurso
{
    private const string ArchivoRecursos = "Textos";

    public static string Obtener(string clave)
    {
        return (string)HttpContext.GetGlobalResourceObject(ArchivoRecursos, clave);
    }

    public static string Formato(string clave, params object[] valores)
    {
        return string.Format(Obtener(clave), valores);
    }
}
