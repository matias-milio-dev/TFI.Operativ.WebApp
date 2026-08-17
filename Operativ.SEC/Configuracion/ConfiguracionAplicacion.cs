using System.Configuration;

namespace Operativ.SEC.Configuracion
{
    public static class ConfiguracionAplicacion
    {
        public const int IntentosMaximosLogin = 3;

        public const int LongitudContrasenaTemporal = 10;

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
}
