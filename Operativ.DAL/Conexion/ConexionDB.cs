using System.Configuration;

namespace Operativ.DAL.Conexion
{
    public class ConexionDB
    {
        private static ConexionDB instancia;
        private static readonly object bloqueo = new object();
        private readonly string cadenaConexion;

        private ConexionDB()
        {
            cadenaConexion = ConfigurationManager.ConnectionStrings["OperativDb"].ConnectionString;
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
    }
}
