using System.Collections.Generic;

namespace Operativ.BE.Modelos;

public class CategoriaPatente
{
    public string Clave { get; }

    public string ClaveRecurso { get; }

    public string[] NombresPatente { get; }

    public CategoriaPatente(string clave, string claveRecurso, string[] nombresPatente)
    {
        Clave = clave;
        ClaveRecurso = claveRecurso;
        NombresPatente = nombresPatente;
    }

    public static readonly CategoriaPatente Usuarios =
        new("Usuarios", "CategoriaUsuarios", new[]
        {
            NombrePatente.ConsultarUsuario,
            NombrePatente.AltaUsuario,
            NombrePatente.BajaUsuario,
            NombrePatente.ModificacionUsuario,
            NombrePatente.DesbloqueoUsuario,
            NombrePatente.BloqueoUsuario,
            NombrePatente.AsignarPatente,
            NombrePatente.RemoverPatente
        });
    public static readonly CategoriaPatente Familias =
        new("Familias", "CategoriaFamilias", new[] { NombrePatente.GestionarFamilias });
    public static readonly CategoriaPatente Clientes =
        new("Clientes", "CategoriaClientes", new[] { NombrePatente.GestionarClientes });
    public static readonly CategoriaPatente Catalogo =
        new("Catalogo", "CategoriaCatalogo", new[] { NombrePatente.GestionarCatalogo });
    public static readonly CategoriaPatente Suscripciones =
        new("Suscripciones", "CategoriaSuscripciones", new[] { NombrePatente.GestionarSuscripciones });
    public static readonly CategoriaPatente Facturacion =
        new("Facturacion", "CategoriaFacturacion", new[] { NombrePatente.ConsultarFacturas });
    public static readonly CategoriaPatente Incidentes =
        new("Incidentes", "CategoriaIncidentes", new[] { NombrePatente.ReportarIncidentes });
    public static readonly CategoriaPatente Sistema =
        new("Sistema", "CategoriaSistema", new[] { NombrePatente.RealizarBackup, NombrePatente.RepararBaseDatos });

    public static List<CategoriaPatente> ObtenerTodas()
    {
        return new List<CategoriaPatente>
        {
            Usuarios,
            Familias,
            Clientes,
            Catalogo,
            Suscripciones,
            Facturacion,
            Incidentes,
            Sistema
        };
    }

    public static CategoriaPatente ObtenerPorPatente(string nombrePatente)
    {
        foreach (CategoriaPatente categoria in ObtenerTodas())
        {
            foreach (string nombre in categoria.NombresPatente)
            {
                if (nombre == nombrePatente)
                {
                    return categoria;
                }
            }
        }

        return Sistema;
    }
}
