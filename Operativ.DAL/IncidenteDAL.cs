using System.Data;
using System.Data.SqlClient;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IIncidenteDAL
    {
        DataTable ListarPorActivo(int idActivo);
        int Insertar(int idActivo, string codigoCategoria, string descripcion, string prioridad);
        void ActualizarRutaXml(int idIncidente, string rutaXmlGenerado);
        void Cerrar(int idIncidente);
    }

    public class IncidenteDAL : IIncidenteDAL
    {
        public DataTable ListarPorActivo(int idActivo)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT i.IdIncidente, i.IdActivo, ci.Codigo AS CodigoCategoria, i.Descripcion, i.Prioridad, i.Estado,
                       i.FechaAlta, i.FechaCierre, i.RutaXmlGenerado
                FROM dbo.Incidente i
                INNER JOIN dbo.CategoriaIncidente ci ON ci.IdCategoriaIncidente = i.IdCategoriaIncidente
                WHERE i.IdActivo = @IdActivo
                ORDER BY i.FechaAlta DESC",
                comando => comando.Parameters.Add("@IdActivo", SqlDbType.Int).Value = idActivo);
        }

        public int Insertar(int idActivo, string codigoCategoria, string descripcion, string prioridad)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idIncidenteNuevo;
                int idCategoria;
                using (var comando = new SqlCommand(@"
                    DECLARE @IdCategoria INT = (SELECT IdCategoriaIncidente FROM dbo.CategoriaIncidente WHERE Codigo = @CodigoCategoria);
                    INSERT INTO dbo.Incidente (IdActivo, IdCategoriaIncidente, Descripcion, Prioridad)
                    VALUES (@IdActivo, @IdCategoria, @Descripcion, @Prioridad);
                    SELECT CAST(SCOPE_IDENTITY() AS INT), @IdCategoria;", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdActivo", SqlDbType.Int).Value = idActivo;
                    comando.Parameters.Add("@CodigoCategoria", SqlDbType.VarChar, 20).Value = codigoCategoria;
                    comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = descripcion;
                    comando.Parameters.Add("@Prioridad", SqlDbType.VarChar, 10).Value = prioridad;

                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idIncidenteNuevo = (int)lector[0];
                        idCategoria = (int)lector[1];
                    }
                }

                string valores = string.Join("|", idIncidenteNuevo.ToString(), idActivo.ToString(), idCategoria.ToString(), prioridad, "ABIERTO");
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Incidente", "IdIncidente", idIncidenteNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Incidente", "IdIncidente");

                return idIncidenteNuevo;
            });
        }

        public void ActualizarRutaXml(int idIncidente, string rutaXmlGenerado)
        {
            DALHelper.EjecutarNonQuery("UPDATE dbo.Incidente SET RutaXmlGenerado = @RutaXmlGenerado WHERE IdIncidente = @IdIncidente", comando =>
            {
                comando.Parameters.Add("@IdIncidente", SqlDbType.Int).Value = idIncidente;
                comando.Parameters.Add("@RutaXmlGenerado", SqlDbType.VarChar, 260).Value = rutaXmlGenerado;
            });
        }

        public void Cerrar(int idIncidente)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                int idActivo, idCategoria;
                string prioridad;
                using (var comando = new SqlCommand(@"
                    UPDATE dbo.Incidente SET Estado = 'CERRADO', FechaCierre = SYSDATETIME() WHERE IdIncidente = @IdIncidente;
                    SELECT IdActivo, IdCategoriaIncidente, Prioridad FROM dbo.Incidente WHERE IdIncidente = @IdIncidente;", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdIncidente", SqlDbType.Int).Value = idIncidente;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        idActivo = (int)lector["IdActivo"];
                        idCategoria = (int)lector["IdCategoriaIncidente"];
                        prioridad = (string)lector["Prioridad"];
                    }
                }

                string valores = string.Join("|", idIncidente.ToString(), idActivo.ToString(), idCategoria.ToString(), prioridad, "CERRADO");
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Incidente", "IdIncidente", idIncidente, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Incidente", "IdIncidente");
            });
        }
    }
}
