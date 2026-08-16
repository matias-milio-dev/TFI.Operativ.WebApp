using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Conexion;

namespace Operativ.DAL.Implementaciones
{
    public class BitacoraRepositorio : IBitacoraRepositorio
    {
        private readonly AccesoDatos accesoDatos;

        public BitacoraRepositorio()
        {
            accesoDatos = new AccesoDatos();
        }

        public void Registrar(Bitacora entrada)
        {
            string consulta = "INSERT INTO Bitacora (IdUsuario, Accion, Criticidad, Descripcion) "
                + "VALUES (@IdUsuario, @Accion, @Criticidad, @Descripcion)";

            object descripcion = entrada.Descripcion ?? (object)DBNull.Value;

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", entrada.IdUsuario),
                new SqlParameter("@Accion", entrada.Accion.ToString()),
                new SqlParameter("@Criticidad", entrada.Criticidad.ToString()),
                new SqlParameter("@Descripcion", descripcion)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);
        }
    }
}
