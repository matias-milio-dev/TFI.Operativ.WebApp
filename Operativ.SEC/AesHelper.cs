using System;
using System.IO;
using System.Security.Cryptography;

namespace Operativ.SEC
{
    public static class AesHelper
    {
        public static string Encriptar(string textoPlano, string claveBase64)
        {
            if (textoPlano == null) throw new ArgumentNullException(nameof(textoPlano));

            byte[] clave = ObtenerClaveDe256Bits(claveBase64);

            using (var aes = Aes.Create())
            {
                aes.Key = clave;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (var encriptador = aes.CreateEncryptor())
                using (var flujoMemoria = new MemoryStream())
                {
                    flujoMemoria.Write(aes.IV, 0, aes.IV.Length);
                    using (var flujoCripto = new CryptoStream(flujoMemoria, encriptador, CryptoStreamMode.Write))
                    using (var escritor = new StreamWriter(flujoCripto))
                    {
                        escritor.Write(textoPlano);
                    }
                    return Convert.ToBase64String(flujoMemoria.ToArray());
                }
            }
        }

        public static string Desencriptar(string textoCifradoBase64, string claveBase64)
        {
            if (string.IsNullOrEmpty(textoCifradoBase64)) throw new ArgumentNullException(nameof(textoCifradoBase64));

            byte[] clave = ObtenerClaveDe256Bits(claveBase64);
            byte[] datosCompletos = Convert.FromBase64String(textoCifradoBase64);

            using (var aes = Aes.Create())
            {
                aes.Key = clave;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] iv = new byte[16];
                Buffer.BlockCopy(datosCompletos, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var desencriptador = aes.CreateDecryptor())
                using (var flujoMemoria = new MemoryStream(datosCompletos, iv.Length, datosCompletos.Length - iv.Length))
                using (var flujoCripto = new CryptoStream(flujoMemoria, desencriptador, CryptoStreamMode.Read))
                using (var lector = new StreamReader(flujoCripto))
                {
                    return lector.ReadToEnd();
                }
            }
        }

        private static byte[] ObtenerClaveDe256Bits(string claveBase64)
        {
            if (string.IsNullOrEmpty(claveBase64))
                throw new InvalidOperationException("No se configuró la clave maestra AES (AppSettings:ClaveMaestraAes).");

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(claveBase64));
            }
        }
    }
}
