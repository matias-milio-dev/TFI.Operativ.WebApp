using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IBackupService
{
    List<ArchivoBackup> ListarBackups();

    string CrearBackup();

    string ObtenerRutaCompleta(string nombreArchivo);

    string PrepararRutaParaSubida();

    void RestaurarBackup(string nombreArchivo);

    void RestaurarBackupDesdeRuta(string rutaArchivo);
}
