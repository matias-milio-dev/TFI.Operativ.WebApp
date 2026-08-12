using System.Net;
using System.Net.Mail;
using Operativ.Comun;

namespace Operativ.SEC
{
    public static class EmailHelper
    {
        public static void Enviar(string destinatario, string asunto, string cuerpoHtml)
        {
            using (var mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(ConfiguracionAplicacion.SmtpRemitente);
                mensaje.To.Add(destinatario);
                mensaje.Subject = asunto;
                mensaje.Body = cuerpoHtml;
                mensaje.IsBodyHtml = true;

                using (var cliente = new SmtpClient(ConfiguracionAplicacion.SmtpServidor, ConfiguracionAplicacion.SmtpPuerto))
                {
                    cliente.EnableSsl = ConfiguracionAplicacion.SmtpUsarSsl;
                    if (!string.IsNullOrEmpty(ConfiguracionAplicacion.SmtpUsuario))
                    {
                        cliente.Credentials = new NetworkCredential(ConfiguracionAplicacion.SmtpUsuario, ConfiguracionAplicacion.SmtpClave);
                    }
                    cliente.Send(mensaje);
                }
            }
        }
    }
}
