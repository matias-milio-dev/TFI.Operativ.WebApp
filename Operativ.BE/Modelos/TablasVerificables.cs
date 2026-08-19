using System.Collections.Generic;

namespace Operativ.BE.Modelos;

public class TablasVerificables
{
    public string Nombre { get; }

    public string[] ColumnasClave { get; }

    public TablasVerificables(string nombre, string[] columnasClave)
    {
        Nombre = nombre;
        ColumnasClave = columnasClave;
    }

    public static readonly TablasVerificables Usuario = new("Usuario", new[] { "IdUsuario" });
    public static readonly TablasVerificables Bitacora = new("Bitacora", new[] { "IdBitacora" });
    public static readonly TablasVerificables Familia = new("Familia", new[] { "IdFamilia" });
    public static readonly TablasVerificables Patente = new("Patente", new[] { "IdPatente" });
    public static readonly TablasVerificables UsuarioFamilia = new("UsuarioFamilia", new[] { "IdUsuario", "IdFamilia" });
    public static readonly TablasVerificables UsuarioPatente = new("UsuarioPatente", new[] { "IdUsuario", "IdPatente" });
    public static readonly TablasVerificables FamiliaPatente = new("FamiliaPatente", new[] { "IdFamilia", "IdPatente" });
    public static readonly TablasVerificables FamiliaFamilia = new("FamiliaFamilia", new[] { "IdFamiliaPadre", "IdFamiliaHija" });

    public static List<TablasVerificables> ObtenerTodas()
    {
        return new List<TablasVerificables>
        {
            Usuario,
            Bitacora,
            Familia,
            Patente,
            UsuarioFamilia,
            UsuarioPatente,
            FamiliaPatente,
            FamiliaFamilia
        };
    }
}
