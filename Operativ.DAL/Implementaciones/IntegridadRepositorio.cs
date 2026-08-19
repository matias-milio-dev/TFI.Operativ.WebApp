using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class IntegridadRepositorio : IIntegridadRepositorio
{
    private class TablaVerificable
    {
        public readonly string Nombre;

        public readonly string[] ColumnasClave;

        public TablaVerificable(string nombre, string[] columnasClave)
        {
            Nombre = nombre;
            ColumnasClave = columnasClave;
        }
    }

    private static readonly List<TablaVerificable> TablasVerificables = new List<TablaVerificable>
    {
        new TablaVerificable("Usuario", new[] { "IdUsuario" }),
        new TablaVerificable("Bitacora", new[] { "IdBitacora" }),
        new TablaVerificable("Familia", new[] { "IdFamilia" }),
        new TablaVerificable("Patente", new[] { "IdPatente" }),
        new TablaVerificable("UsuarioFamilia", new[] { "IdUsuario", "IdFamilia" }),
        new TablaVerificable("UsuarioPatente", new[] { "IdUsuario", "IdPatente" }),
        new TablaVerificable("FamiliaPatente", new[] { "IdFamilia", "IdPatente" }),
        new TablaVerificable("FamiliaFamilia", new[] { "IdFamiliaPadre", "IdFamiliaHija" })
    };

    private readonly AccesoDatos accesoDatos;

    public IntegridadRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public bool ExisteLineaBase()
    {
        object resultado = accesoDatos.EjecutarEscalar("SELECT COUNT(*) FROM DigitosVerticales", null);
        return Convert.ToInt32(resultado) > 0;
    }

    public void RecalcularTodo()
    {
        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
        {
            conexion.Open();

            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                try
                {
                    foreach (TablaVerificable tabla in TablasVerificables)
                    {
                        RecalcularTabla(conexion, transaccion, tabla);
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }
    }

    private void RecalcularTabla(SqlConnection conexion, SqlTransaction transaccion, TablaVerificable tabla)
    {
        DataTable filas = new DataTable();

        using (SqlCommand comando = new SqlCommand(string.Format("SELECT * FROM {0}", tabla.Nombre), conexion, transaccion))
        {
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                filas.Load(lector);
            }
        }

        List<long> valoresDvh = new List<long>();

        foreach (DataRow fila in filas.Rows)
        {
            string cadenaBase = IntegridadHelper.ConstruirCadenaBase(fila);
            long dvh = IntegridadHelper.CalcularDVH(cadenaBase);
            valoresDvh.Add(dvh);

            ActualizarDvhFila(conexion, transaccion, tabla, fila, dvh);
        }

        long dvv = IntegridadHelper.CalcularDVV(valoresDvh);
        int filasActualizadas;

        using (SqlCommand comandoUpdate = new SqlCommand(
            "UPDATE DigitosVerticales SET ValorDVV = @ValorDVV, FechaCalculo = GETDATE() WHERE NombreTabla = @NombreTabla",
            conexion, transaccion))
        {
            comandoUpdate.Parameters.Add(new SqlParameter("@ValorDVV", dvv));
            comandoUpdate.Parameters.Add(new SqlParameter("@NombreTabla", tabla.Nombre));
            filasActualizadas = comandoUpdate.ExecuteNonQuery();
        }

        if (filasActualizadas == 0)
        {
            using (SqlCommand comandoInsert = new SqlCommand(
                "INSERT INTO DigitosVerticales (NombreTabla, ValorDVV, FechaCalculo) VALUES (@NombreTabla, @ValorDVV, GETDATE())",
                conexion, transaccion))
            {
                comandoInsert.Parameters.Add(new SqlParameter("@NombreTabla", tabla.Nombre));
                comandoInsert.Parameters.Add(new SqlParameter("@ValorDVV", dvv));
                comandoInsert.ExecuteNonQuery();
            }
        }
    }

    private void ActualizarDvhFila(SqlConnection conexion, SqlTransaction transaccion, TablaVerificable tabla, DataRow fila, long dvh)
    {
        List<string> condiciones = new List<string>();

        foreach (string columna in tabla.ColumnasClave)
        {
            condiciones.Add(string.Format("{0} = @{0}", columna));
        }

        string consultaUpdate = string.Format("UPDATE {0} SET DVH = @Dvh WHERE {1}", tabla.Nombre, string.Join(" AND ", condiciones));

        using (SqlCommand comandoUpdate = new SqlCommand(consultaUpdate, conexion, transaccion))
        {
            comandoUpdate.Parameters.Add(new SqlParameter("@Dvh", dvh));

            foreach (string columna in tabla.ColumnasClave)
            {
                comandoUpdate.Parameters.Add(new SqlParameter("@" + columna, fila[columna]));
            }

            comandoUpdate.ExecuteNonQuery();
        }
    }

    public List<ResultadoVerificacionTabla> VerificarTodo()
    {
        List<ResultadoVerificacionTabla> resultadosInvalidos = new List<ResultadoVerificacionTabla>();

        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
        {
            conexion.Open();

            foreach (TablaVerificable tabla in TablasVerificables)
            {
                ResultadoVerificacionTabla resultado = VerificarTabla(conexion, tabla);

                if (!resultado.Integra)
                {
                    resultadosInvalidos.Add(resultado);
                }
            }
        }

        return resultadosInvalidos;
    }

    private ResultadoVerificacionTabla VerificarTabla(SqlConnection conexion, TablaVerificable tabla)
    {
        long dvvAlmacenado = ObtenerDvvAlmacenado(conexion, tabla.Nombre);

        DataTable filas = new DataTable();

        using (SqlCommand comando = new SqlCommand(string.Format("SELECT * FROM {0}", tabla.Nombre), conexion))
        {
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                filas.Load(lector);
            }
        }

        List<long> valoresDvhAlmacenados = new List<long>();
        List<string> clavesFilasInvalidas = new List<string>();

        foreach (DataRow fila in filas.Rows)
        {
            object valorAlmacenado = fila["DVH"];

            if (valorAlmacenado != DBNull.Value)
            {
                valoresDvhAlmacenados.Add(Convert.ToInt64(valorAlmacenado));
            }

            string cadenaBase = IntegridadHelper.ConstruirCadenaBase(fila);
            long dvhCalculado = IntegridadHelper.CalcularDVH(cadenaBase);

            bool filaValida = valorAlmacenado != DBNull.Value
                && Convert.ToInt64(valorAlmacenado) == dvhCalculado;

            if (!filaValida)
            {
                clavesFilasInvalidas.Add(FormatearClave(tabla, fila));
            }
        }

        long dvvCalculado = IntegridadHelper.CalcularDVV(valoresDvhAlmacenados);

        ResultadoVerificacionTabla resultado = new ResultadoVerificacionTabla
        {
            NombreTabla = tabla.Nombre,
            ValorDvvAlmacenado = dvvAlmacenado,
            ValorDvvCalculado = dvvCalculado,
            Integra = clavesFilasInvalidas.Count == 0 && dvvAlmacenado == dvvCalculado
        };

        if (!resultado.Integra)
        {
            resultado.ClavesFilasInvalidas.AddRange(clavesFilasInvalidas);
        }

        return resultado;
    }

    private string FormatearClave(TablaVerificable tabla, DataRow fila)
    {
        List<string> partes = new List<string>();

        foreach (string columna in tabla.ColumnasClave)
        {
            partes.Add(string.Format("{0}={1}", columna, fila[columna]));
        }

        return string.Join(", ", partes);
    }

    private long ObtenerDvvAlmacenado(SqlConnection conexion, string nombreTabla)
    {
        using (SqlCommand comando = new SqlCommand(
            "SELECT ValorDVV FROM DigitosVerticales WHERE NombreTabla = @NombreTabla", conexion))
        {
            comando.Parameters.Add(new SqlParameter("@NombreTabla", nombreTabla));
            object resultado = comando.ExecuteScalar();

            if (resultado == null)
            {
                return long.MinValue;
            }

            return Convert.ToInt64(resultado);
        }
    }
}
