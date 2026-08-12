using System;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public sealed class ConexionDB
    {
        private static readonly Lazy<ConexionDB> _instancia = new Lazy<ConexionDB>(() => new ConexionDB());

        public static ConexionDB Instancia => _instancia.Value;

        public string CadenaConexion { get; }

        private ConexionDB()
        {
            CadenaConexion = ConfiguracionAplicacion.CadenaConexion;
            if (string.IsNullOrEmpty(CadenaConexion))
            {
                throw new InvalidOperationException("No se configuró la cadena de conexión 'Operativ' en web.config.");
            }
        }

        public SqlConnection NuevaConexion()
        {
            return new SqlConnection(CadenaConexion);
        }
    }
}
