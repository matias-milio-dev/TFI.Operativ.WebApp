using System.Net;
using System.Net.Mail;
using System.Text;
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

    public static void EnviarFactura(string emailDestino, string numeroFactura, string cuerpoHtml, string facturaHtml)
    {
        string facturaDocumento = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>" + numeroFactura + "</title></head><body>"
            + facturaHtml + "</body></html>";

        Attachment adjunto = Attachment.CreateAttachmentFromString(facturaDocumento, "Factura_" + numeroFactura + ".html", Encoding.UTF8, "text/html");

        EnviarEmail(emailDestino, "Operativ - Factura " + numeroFactura, cuerpoHtml, true, adjunto);
    }

    private static void EnviarEmail(string emailDestino, string asunto, string cuerpo)
    {
        EnviarEmail(emailDestino, asunto, cuerpo, false, null);
    }

    private static void EnviarEmail(string emailDestino, string asunto, string cuerpo, bool esHtml, Attachment adjunto)
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
                IsBodyHtml = esHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                Body = cuerpo
            })
            {
                mensaje.To.Add(emailDestino);

                if (adjunto != null)
                {
                    mensaje.Attachments.Add(adjunto);
                }

                cliente.Send(mensaje);
            }
        }
    }
}
