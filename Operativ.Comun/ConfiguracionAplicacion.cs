using System;
using System.Configuration;

namespace Operativ.Comun
{
    public static class ConfiguracionAplicacion
    {
        public static string CadenaConexion =>
            ConfigurationManager.ConnectionStrings["Operativ"]?.ConnectionString;

        public const int IntentosMaximosLogin = 3;

        public static string ClaveMaestraAes =>
            ConfigurationManager.AppSettings["ClaveMaestraAes"];

        public static int TimeoutSesionMinutos =>
            int.TryParse(ConfigurationManager.AppSettings["TimeoutSesionMinutos"], out var valor) ? valor : 20;

        public static string RutaXmlGenerado =>
            ConfigurationManager.AppSettings["RutaXmlGenerado"] ??
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "XmlGenerado");

        public static string SmtpServidor =>
            ConfigurationManager.AppSettings["SmtpServidor"];

        public static int SmtpPuerto =>
            int.TryParse(ConfigurationManager.AppSettings["SmtpPuerto"], out var valor) ? valor : 25;

        public static string SmtpUsuario =>
            ConfigurationManager.AppSettings["SmtpUsuario"];

        public static string SmtpClave =>
            ConfigurationManager.AppSettings["SmtpClave"];

        public static bool SmtpUsarSsl =>
            bool.TryParse(ConfigurationManager.AppSettings["SmtpUsarSsl"], out var valor) && valor;

        public static string SmtpRemitente =>
            ConfigurationManager.AppSettings["SmtpRemitente"];
    }
}
