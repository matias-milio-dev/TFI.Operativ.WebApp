using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class AccionBitacora
{
    public TipoAccionBitacora Tipo { get; }

    public CriticidadBitacora Criticidad { get; }

    public string Descripcion { get; }

    public AccionBitacora(TipoAccionBitacora tipo, CriticidadBitacora criticidad, string descripcion)
    {
        Tipo = tipo;
        Criticidad = criticidad;
        Descripcion = descripcion;
    }

    public static readonly AccionBitacora LoginExitoso =
        new(TipoAccionBitacora.LoginExitoso, CriticidadBitacora.Informativo, "Inicio de sesión exitoso");
    public static readonly AccionBitacora LoginBloqueado =
        new(TipoAccionBitacora.LoginBloqueado, CriticidadBitacora.Critico, "Usuario bloqueado tras {0} intentos fallidos");
    public static readonly AccionBitacora RecuperacionContrasena =
        new(TipoAccionBitacora.RecuperacionContrasena, CriticidadBitacora.Advertencia, "Contraseña restablecida por recuperación");
    public static readonly AccionBitacora CierreSesion =
        new(TipoAccionBitacora.CierreSesion, CriticidadBitacora.Informativo, "Cierre de sesión");
    public static readonly AccionBitacora IntentoLoginFallido =
        new(TipoAccionBitacora.IntentoLoginFallido, CriticidadBitacora.Critico, "Login con credenciales invalidas");
    public static readonly AccionBitacora AltaUsuario =
        new(TipoAccionBitacora.AltaUsuario, CriticidadBitacora.Informativo, "Alta de usuario");
    public static readonly AccionBitacora BajaUsuario =
        new(TipoAccionBitacora.BajaUsuario, CriticidadBitacora.Advertencia, "Baja lógica de usuario");
    public static readonly AccionBitacora ModificacionUsuario =
        new(TipoAccionBitacora.ModificacionUsuario, CriticidadBitacora.Informativo, "Modificación de datos de usuario");
    public static readonly AccionBitacora DesbloqueoUsuario =
        new(TipoAccionBitacora.DesbloqueoUsuario, CriticidadBitacora.Advertencia, "Desbloqueo de usuario");
    public static readonly AccionBitacora CambioClave =
        new(TipoAccionBitacora.CambioClave, CriticidadBitacora.Informativo, "Cambio de contraseña por autogestión");
    public static readonly AccionBitacora ReparacionEmergenciaBaseDatos =
        new(TipoAccionBitacora.ReparacionEmergenciaBaseDatos, CriticidadBitacora.Critico, "Base de datos reparada mediante acceso de emergencia del Web Master");
    public static readonly AccionBitacora IntegridadCorrupta =
        new(TipoAccionBitacora.IntegridadCorrupta, CriticidadBitacora.Critico, "Se detectó una alteración en la integridad de los datos del sistema");
    public static readonly AccionBitacora BloqueoManualUsuario =
        new(TipoAccionBitacora.BloqueoManualUsuario, CriticidadBitacora.Advertencia, "Bloqueo manual de usuario por administrador");
    public static readonly AccionBitacora AsignacionPatente =
        new(TipoAccionBitacora.AsignacionPatente, CriticidadBitacora.Advertencia, "Asignación de patentes individuales a un usuario");
    public static readonly AccionBitacora RemocionPatente =
        new(TipoAccionBitacora.RemocionPatente, CriticidadBitacora.Advertencia, "Remoción de patentes individuales de un usuario");
    public static readonly AccionBitacora BackupBaseDatos =
        new(TipoAccionBitacora.BackupBaseDatos, CriticidadBitacora.Advertencia, "Backup de la base de datos generado bajo demanda");
    public static readonly AccionBitacora RestoreBaseDatos =
        new(TipoAccionBitacora.RestoreBaseDatos, CriticidadBitacora.Critico, "Restore de la base de datos ejecutado bajo demanda");
    public static readonly AccionBitacora AltaPaquete =
        new(TipoAccionBitacora.AltaPaquete, CriticidadBitacora.Informativo, "Alta de paquete de configuración");
    public static readonly AccionBitacora BajaPaquete =
        new(TipoAccionBitacora.BajaPaquete, CriticidadBitacora.Advertencia, "Baja lógica de paquete de configuración");
    public static readonly AccionBitacora ModificacionPaquete =
        new(TipoAccionBitacora.ModificacionPaquete, CriticidadBitacora.Informativo, "Modificación de paquete de configuración");

    public static List<AccionBitacora> ObtenerTodas()
    {
        return new List<AccionBitacora>
        {
            LoginExitoso,
            LoginBloqueado,
            RecuperacionContrasena,
            CierreSesion,
            IntentoLoginFallido,
            AltaUsuario,
            BajaUsuario,
            ModificacionUsuario,
            DesbloqueoUsuario,
            CambioClave,
            ReparacionEmergenciaBaseDatos,
            IntegridadCorrupta,
            BloqueoManualUsuario,
            AsignacionPatente,
            RemocionPatente,
            BackupBaseDatos,
            RestoreBaseDatos,
            AltaPaquete,
            BajaPaquete,
            ModificacionPaquete
        };
    }

    public static AccionBitacora ObtenerPorTipo(TipoAccionBitacora tipo)
    {
        foreach (AccionBitacora accion in ObtenerTodas())
        {
            if (accion.Tipo == tipo)
            {
                return accion;
            }
        }

        return null;
    }
}
