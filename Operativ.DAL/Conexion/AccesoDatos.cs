using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Operativ.DAL.Conexion;

//Clase que representa las operaciones comunes que podemos hacer con ADO.NET
//para interactuar con la base de datos SQL
public class AccesoDatos
{
    //Ejecuta una consulta que devuelve un datatable, basada en la sql y lista de
    //sql parameters que se pasa en la firma del metodo.
    //Dentro de un bloque using, para correcto manejo de los recursos no administrados, 
    //Usando el patron singleton obtenemos la cadena de conexion para crear el objeto SqlConnection
    //Creamos el objeto sql command pasandole el commandtext (consulta) y la conexion al constructor
    //Agregamos los parametros al comando con el metodo privado AgregarParametros y abrimos la conexion.
    //Finalmente llenamos el datatable de resultado ejecutando el sql command.
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

    //Ejecuta una consulta que impacta la base de datos, basada en la sql y lista de
    //sql parameters que se pasa en la firma del metodo.
    //Dentro de un bloque using, para correcto manejo de los recursos no administrados, 
    //Usando el patron singleton obtenemos la cadena de conexion para crear el objeto SqlConnection
    //Creamos el objeto sql command pasandole el commandtext (consulta) y la conexion al constructor
    //Agregamos los parametros al comando con el metodo privado AgregarParametros y abrimos la conexion.
    //Finalmente ejecutamos la query y obtenemos las filas afectadas como int que es lo que devolvemos.
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

    //Ejecuta una consulta y devuelve el primer resultado, basada en la sql y lista de
    //sql parameters que se pasa en la firma del metodo.
    //Dentro de un bloque using, para correcto manejo de los recursos no administrados, 
    //Usando el patron singleton obtenemos la cadena de conexion para crear el objeto SqlConnection
    //Creamos el objeto sql command pasandole el commandtext (consulta) y la conexion al constructor
    //Agregamos los parametros al comando con el metodo privado AgregarParametros y abrimos la conexion.
    //Finalmente ejecutamos la query y obtenemos el primer resultado como objeto.
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

    //Recorre la lista de parametros, si esta inicializada y la agrega al comando
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
