namespace Operativ.DAL.Contratos;
public interface IBackupRepositorio
{
    void EjecutarBackup(string rutaArchivo);

    void EjecutarRestore(string rutaArchivo);
}
