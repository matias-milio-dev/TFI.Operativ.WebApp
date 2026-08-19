using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Conexion;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class BitacoraRepositorio : IBitacoraRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public BitacoraRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public void Registrar(Bitacora entrada)
    {
        string consulta = "INSERT INTO Bitacora (IdUsuario, Accion, Criticidad, Descripcion) "
            + "VALUES (@IdUsuario, @Accion, @Criticidad, @Descripcion); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        object descripcion = entrada.Descripcion ?? (object)DBNull.Value;
        object idUsuario = entrada.IdUsuario.HasValue ? (object)entrada.IdUsuario.Value : DBNull.Value;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@Accion", entrada.Accion.ToString()),
            new SqlParameter("@Criticidad", entrada.Criticidad.ToString()),
            new SqlParameter("@Descripcion", descripcion)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idBitacora = Convert.ToInt32(resultado);
        ActualizarDVH(idBitacora);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Bitacora", "IdBitacora", id);
    }
}
