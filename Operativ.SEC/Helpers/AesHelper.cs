using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Operativ.SEC.Helpers;
public static class AesHelper
{
    private static readonly byte[] ClaveFija = Encoding.UTF8.GetBytes("Operativ2026ClaveSecreta256Bits");

    private static readonly byte[] VectorFijo = Encoding.UTF8.GetBytes("Operativ2026Vect");

    public static string Encriptar(string textoPlano)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = ClaveFija;
            aes.IV = VectorFijo;

            using (ICryptoTransform encriptador = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                using (MemoryStream flujoMemoria = new MemoryStream())
                {
                    using (CryptoStream flujoCripto = new CryptoStream(flujoMemoria, encriptador, CryptoStreamMode.Write))
                    {
                        byte[] bytesPlano = Encoding.UTF8.GetBytes(textoPlano);
                        flujoCripto.Write(bytesPlano, 0, bytesPlano.Length);
                        flujoCripto.FlushFinalBlock();
                        return Convert.ToBase64String(flujoMemoria.ToArray());
                    }
                }
            }
        }
    }

    public static string Desencriptar(string textoEncriptado)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = ClaveFija;
            aes.IV = VectorFijo;

            using (ICryptoTransform desencriptador = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] bytesEncriptados = Convert.FromBase64String(textoEncriptado);

                using (MemoryStream flujoMemoria = new MemoryStream(bytesEncriptados))
                {
                    using (CryptoStream flujoCripto = new CryptoStream(flujoMemoria, desencriptador, CryptoStreamMode.Read))
                    {
                        using (StreamReader lector = new StreamReader(flujoCripto))
                        {
                            return lector.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
