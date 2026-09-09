using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.SEC.Implementaciones;
public class BackupService : IBackupService
{
    private readonly IBackupRepositorio backupRepositorio;
    private readonly IBitacoraService bitacoraService;

    public BackupService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        backupRepositorio = fabricaRepositorio.CrearBackupRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<ArchivoBackup> ListarBackups()
    {
        List<ArchivoBackup> backups = new List<ArchivoBackup>();
        string carpeta = ConfiguracionAplicacion.CarpetaBackups;

        if (!Directory.Exists(carpeta))
        {
            return backups;
        }

        foreach (string ruta in Directory.GetFiles(carpeta, "*.bak"))
        {
            FileInfo info = new FileInfo(ruta);
            backups.Add(new ArchivoBackup
            {
                NombreArchivo = info.Name,
                FechaCreacion = info.CreationTime,
                TamanioBytes = info.Length
            });
        }

        OrdenarPorFechaDescendente(backups);

        return backups;
    }

    public string CrearBackup()
    {
        string carpeta = ConfiguracionAplicacion.CarpetaBackups;

        if (!Directory.Exists(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        string nombreArchivo = "OperativDb_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
        string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

        try
        {
            backupRepositorio.EjecutarBackup(rutaCompleta);
        }
        catch (SqlException excepcion)
        {
            throw new OperativException(TipoError.ErrorOperacionBackupFallida, new string[] { excepcion.Message });
        }

        bitacoraService.Registrar(null, TipoAccionBitacora.BackupBaseDatos, rutaCompleta);

        return nombreArchivo;
    }

    public string ObtenerRutaCompleta(string nombreArchivo)
    {
        return Path.Combine(ConfiguracionAplicacion.CarpetaBackups, nombreArchivo);
    }

    public string PrepararRutaParaSubida()
    {
        string carpeta = ConfiguracionAplicacion.CarpetaBackups;

        if (!Directory.Exists(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        string nombreArchivo = "Subido_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".bak";
        return Path.Combine(carpeta, nombreArchivo);
    }

    public void RestaurarBackup(string nombreArchivo)
    {
        RestaurarDesdeRuta(ObtenerRutaCompleta(nombreArchivo), nombreArchivo);
    }

    public void RestaurarBackupDesdeRuta(string rutaArchivo)
    {
        RestaurarDesdeRuta(rutaArchivo, Path.GetFileName(rutaArchivo));
    }

    private void RestaurarDesdeRuta(string rutaCompleta, string nombreArchivo)
    {
        if (!File.Exists(rutaCompleta))
        {
            throw new OperativException(TipoError.ErrorArchivoBackupNoExiste, new string[] { nombreArchivo });
        }

        try
        {
            backupRepositorio.EjecutarRestore(rutaCompleta);
        }
        catch (SqlException excepcion)
        {
            throw new OperativException(TipoError.ErrorOperacionBackupFallida, new string[] { excepcion.Message });
        }

        bitacoraService.Registrar(null, TipoAccionBitacora.RestoreBaseDatos, rutaCompleta);
    }

    private void OrdenarPorFechaDescendente(List<ArchivoBackup> backups)
    {
        for (int i = 0; i < backups.Count - 1; i++)
        {
            for (int j = 0; j < backups.Count - 1 - i; j++)
            {
                if (backups[j].FechaCreacion < backups[j + 1].FechaCreacion)
                {
                    ArchivoBackup temporal = backups[j];
                    backups[j] = backups[j + 1];
                    backups[j + 1] = temporal;
                }
            }
        }
    }
}
