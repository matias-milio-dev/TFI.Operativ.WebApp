using System.Collections.Generic;
using System.Data.SqlClient;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;

namespace Operativ.DAL.Implementaciones;
public class BackupRepositorio : IBackupRepositorio
{
    private readonly AccesoDatos accesoDatos;

    public BackupRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public void EjecutarBackup(string rutaArchivo)
    {
        string consulta = "EXEC master.dbo.uspBackupOperativDb @RutaArchivo";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@RutaArchivo", rutaArchivo)
        };

        accesoDatos.EjecutarConsultaEnMaster(consulta, parametros);
    }

    public void EjecutarRestore(string rutaArchivo)
    {
        string consulta = "EXEC master.dbo.uspRestoreOperativDb @RutaArchivo";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@RutaArchivo", rutaArchivo)
        };

        accesoDatos.EjecutarConsultaEnMaster(consulta, parametros);
    }
}
