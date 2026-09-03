using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class CategoriaPatente
{
    public TipoCategoriaPatente Tipo { get; }

    public string ClaveRecurso { get; }

    public string[] NombresPatente { get; }

    public CategoriaPatente(TipoCategoriaPatente tipo, string claveRecurso, string[] nombresPatente)
    {
        Tipo = tipo;
        ClaveRecurso = claveRecurso;
        NombresPatente = nombresPatente;
    }

    public static readonly CategoriaPatente Usuarios =
        new(TipoCategoriaPatente.Usuarios, "CategoriaUsuarios", new[]
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
        new(TipoCategoriaPatente.Familias, "CategoriaFamilias", new[] { NombrePatente.GestionarFamilias });
    public static readonly CategoriaPatente Clientes =
        new(TipoCategoriaPatente.Clientes, "CategoriaClientes", new[] { NombrePatente.GestionarClientes });
    public static readonly CategoriaPatente Catalogo =
        new(TipoCategoriaPatente.Catalogo, "CategoriaCatalogo", new[] { NombrePatente.GestionarCatalogo });
    public static readonly CategoriaPatente Suscripciones =
        new(TipoCategoriaPatente.Suscripciones, "CategoriaSuscripciones", new[] { NombrePatente.GestionarSuscripciones });
    public static readonly CategoriaPatente Facturacion =
        new(TipoCategoriaPatente.Facturacion, "CategoriaFacturacion", new[] { NombrePatente.ConsultarFacturas });
    public static readonly CategoriaPatente Incidentes =
        new(TipoCategoriaPatente.Incidentes, "CategoriaIncidentes", new[] { NombrePatente.ReportarIncidentes });
    public static readonly CategoriaPatente Sistema =
        new(TipoCategoriaPatente.Sistema, "CategoriaSistema", new[] { NombrePatente.RealizarBackup, NombrePatente.RepararBaseDatos });

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

    public static CategoriaPatente ObtenerPorTipo(TipoCategoriaPatente tipo)
    {
        foreach (CategoriaPatente categoria in ObtenerTodas())
        {
            if (categoria.Tipo == tipo)
            {
                return categoria;
            }
        }

        return null;
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
