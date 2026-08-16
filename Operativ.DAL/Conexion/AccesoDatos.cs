using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Operativ.DAL.Conexion
{
    public class AccesoDatos
    {
        public DataTable EjecutarReader(string consulta, List<SqlParameter> parametros)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, parametros);
                    conexion.Open();

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        tabla.Load(lector);
                    }
                }
            }
            return tabla;
        }

        public int EjecutarConsulta(string consulta, List<SqlParameter> parametros)
        {
            int filasAfectadas;
            using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, parametros);
                    conexion.Open();
                    filasAfectadas = comando.ExecuteNonQuery();
                }
            }
            return filasAfectadas;
        }

        public object EjecutarEscalar(string consulta, List<SqlParameter> parametros)
        {
            object resultado;
            using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
            {
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, parametros);
                    conexion.Open();
                    resultado = comando.ExecuteScalar();
                }
            }
            return resultado;
        }

        private void AgregarParametros(SqlCommand comando, List<SqlParameter> parametros)
        {
            if (parametros != null)
            {
                foreach (SqlParameter parametro in parametros)
                {
                    comando.Parameters.Add(parametro);
                }
            }
        }
    }
}
