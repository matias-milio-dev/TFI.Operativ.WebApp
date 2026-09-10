using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public static class Paginado
{
    public static string FormatearResumen(string claveRecurso, int numeroPagina, int tamanioPagina, int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((numeroPagina - 1) * tamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        return TextoRecurso.Formato(claveRecurso, desde, hasta, total);
    }
}
