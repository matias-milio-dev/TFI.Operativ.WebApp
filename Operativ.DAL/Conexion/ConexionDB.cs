using System.Configuration;
using System.Data.SqlClient;

namespace Operativ.DAL.Conexion;
public class ConexionDB
{
    private static ConexionDB instancia;
    private static readonly object bloqueo = new object();
    private readonly string cadenaConexion;
    private readonly string cadenaConexionMaster;

    private ConexionDB()
    {
        cadenaConexion = ConfigurationManager.ConnectionStrings["OperativDb"].ConnectionString;
        cadenaConexionMaster = ConstruirCadenaConexionMaster(cadenaConexion);
    }
    public static ConexionDB Instancia
    {
        get
        {
            lock (bloqueo)
            {
                if (instancia == null)
                {
                    instancia = new ConexionDB();
                }
            }
            return instancia;
        }
    }

    public string GetCadenaConexion()
    {
        return cadenaConexion;
    }

    public string GetCadenaConexionMaster()
    {
        return cadenaConexionMaster;
    }

    private string ConstruirCadenaConexionMaster(string cadenaOriginal)
    {
        SqlConnectionStringBuilder constructor = new SqlConnectionStringBuilder(cadenaOriginal);
        constructor.InitialCatalog = "master";
        return constructor.ConnectionString;
    }
}
