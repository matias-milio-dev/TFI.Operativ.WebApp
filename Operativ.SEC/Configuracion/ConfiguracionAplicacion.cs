using System.Configuration;

namespace Operativ.SEC.Configuracion;
public static class ConfiguracionAplicacion
{
    public static int IntentosMaximosLogin
    {
        get { return int.Parse(GetConfiguracion("Operativ.IntentosMaximosLogin", "3")); }
    }

    public static int LongitudContrasenaTemporal
    {
        get { return int.Parse(GetConfiguracion("Operativ.LongitudContrasenaTemporal", "10")); }
    }

    public static int TamanoPredeterminadoGrillaUsuarios
    {
        get { return int.Parse(GetConfiguracion("Operativ.TamanoPredeterminadoGrillaUsuarios", "10")); }
    }

    public static string ServidorSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Servidor", "localhost"); }
    }

    public static int PuertoSmtp
    {
        get { return int.Parse(GetConfiguracion("Operativ.Smtp.Puerto", "25")); }
    }

    public static string UsuarioSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Usuario", string.Empty); }
    }

    public static string ContrasenaSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Contrasena", string.Empty); }
    }

    public static bool UsarSslSmtp
    {
        get { return bool.Parse(GetConfiguracion("Operativ.Smtp.UsarSsl", "false")); }
    }

    public static string EmailRemitente
    {
        get { return GetConfiguracion("Operativ.Smtp.EmailRemitente", "no-responder@operativ.com"); }
    }

    public static bool HabilitarEnvioEmail
    {
        get { return bool.Parse(GetConfiguracion("HabilitarEnvioEmail", "false")); }
    }

    public static string RutaXmlEmergencia
    {
        get { return GetConfiguracion("Operativ.Emergencia.RutaXml", "~/App_Data/AccesoEmergencia.xml"); }
    }

    private static string GetConfiguracion(string clave, string valorPorDefecto)
    {
        string valor = ConfigurationManager.AppSettings[clave];

        if (string.IsNullOrEmpty(valor))
        {
            valor = valorPorDefecto;
        }

        return valor;
    }
}
