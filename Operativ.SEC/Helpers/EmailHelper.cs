using System.Net;
using System.Net.Mail;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers;
public static class EmailHelper
{
    public static void EnviarContrasenaTemporal(string emailDestino, string nombreUsuario, string contrasenaTemporal)
    {
        string cuerpo = "Hola " + nombreUsuario + ","
            + "\n\nSu nueva contraseña temporal es: " + contrasenaTemporal
            + "\n\nPor seguridad, cámbiela luego de iniciar sesión."
            + "\n\nOperativ.";

        EnviarEmail(emailDestino, "Operativ - Recuperación de contraseña", cuerpo);
    }

    public static void EnviarBienvenida(string emailDestino, string nombreUsuario, string contrasenaTemporal)
    {
        string cuerpo = "Hola " + nombreUsuario + ","
            + "\n\nSe creó su usuario en Operativ. Su contraseña temporal es: " + contrasenaTemporal
            + "\n\nPor seguridad, cámbiela luego de iniciar sesión."
            + "\n\nOperativ.";

        EnviarEmail(emailDestino, "Operativ - Bienvenido a la plataforma", cuerpo);
    }

    private static void EnviarEmail(string emailDestino, string asunto, string cuerpo)
    {
        if (!ConfiguracionAplicacion.HabilitarEnvioEmail)
        {
            return;
        }

        using (SmtpClient cliente = new SmtpClient(ConfiguracionAplicacion.ServidorSmtp, ConfiguracionAplicacion.PuertoSmtp)
        {
            EnableSsl = ConfiguracionAplicacion.UsarSslSmtp
        })
        {
            if (!string.IsNullOrEmpty(ConfiguracionAplicacion.UsuarioSmtp))
            {
                cliente.Credentials = new NetworkCredential(ConfiguracionAplicacion.UsuarioSmtp, ConfiguracionAplicacion.ContrasenaSmtp);
            }

            using (MailMessage mensaje = new MailMessage
            {
                From = new MailAddress(ConfiguracionAplicacion.EmailRemitente),
                Subject = asunto,
                IsBodyHtml = false,
                Body = cuerpo
            })
            {
                mensaje.To.Add(emailDestino);

                cliente.Send(mensaje);
            }
        }
    }
}
