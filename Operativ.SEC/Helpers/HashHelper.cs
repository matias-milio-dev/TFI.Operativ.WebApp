using System;
using System.Security.Cryptography;
using System.Text;

namespace Operativ.SEC.Helpers;
public static class HashHelper
{
    public static string GenerarSalt()
    {
        byte[] bytesAleatorios = new byte[16];

        using (RNGCryptoServiceProvider generador = new RNGCryptoServiceProvider())
        {
            generador.GetBytes(bytesAleatorios);
        }

        return Convert.ToBase64String(bytesAleatorios);
    }

    public static string GenerarHash(string contrasena, string salt)
    {
        using (SHA256 algoritmo = SHA256.Create())
        {
            byte[] bytesEntrada = Encoding.UTF8.GetBytes(contrasena + salt);
            byte[] bytesHash = algoritmo.ComputeHash(bytesEntrada);
            return Convert.ToBase64String(bytesHash);
        }
    }

    public static bool ValidarContrasena(string contrasena, string salt, string hashAlmacenado)
    {
        string hashCalculado = HashHelper.GenerarHash(contrasena, salt);
        return string.Equals(hashCalculado, hashAlmacenado, StringComparison.Ordinal);
    }
}
