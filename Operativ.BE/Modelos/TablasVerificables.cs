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
    public static readonly TablasVerificables Programa = new("Programa", new[] { "IdPrograma" });
    public static readonly TablasVerificables Paquete = new("Paquete", new[] { "IdPaquete" });
    public static readonly TablasVerificables PaquetePrograma = new("PaquetePrograma", new[] { "IdPaquete", "IdPrograma" });
    public static readonly TablasVerificables Activo = new("Activo", new[] { "IdActivo" });
    public static readonly TablasVerificables Cliente = new("Cliente", new[] { "IdCliente" });

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
            FamiliaFamilia,
            Programa,
            Paquete,
            PaquetePrograma,
            Activo,
            Cliente
        };
    }
}
