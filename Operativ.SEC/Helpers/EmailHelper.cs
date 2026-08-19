using System.Net;
using System.Net.Mail;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers;
public static class EmailHelper
{
    public static void EnviarContrasenaTemporal(string emailDestino, string nombreUsuario, string contrasenaTemporal)
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
                Subject = "Operativ - Recuperación de contraseña",
                IsBodyHtml = false,
                Body = "Hola " + nombreUsuario + ","
                    + "\n\nSu nueva contraseña temporal es: " + contrasenaTemporal
                    + "\n\nPor seguridad, cámbiela luego de iniciar sesión."
                    + "\n\nOperativ."
            })
            {
                mensaje.To.Add(emailDestino);

                cliente.Send(mensaje);
            }
        }
    }

    public static void EnviarBienvenida(string emailDestino, string nombreUsuario, string contrasenaTemporal)
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
                Subject = "Operativ - Bienvenido a la plataforma",
                IsBodyHtml = false,
                Body = "Hola " + nombreUsuario + ","
                    + "\n\nSe creó su usuario en Operativ. Su contraseña temporal es: " + contrasenaTemporal
                    + "\n\nPor seguridad, cámbiela luego de iniciar sesión."
                    + "\n\nOperativ."
            })
            {
                mensaje.To.Add(emailDestino);

                cliente.Send(mensaje);
            }
        }
    }
}
