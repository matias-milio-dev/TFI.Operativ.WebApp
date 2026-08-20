using System;
using System.Security.Cryptography;
using System.Text;

namespace Operativ.SEC.Helpers;

//Helper statico que contiene metodos para generar contrasenas
//y validarlas
public static class HashHelper
{
    //Genera salt usando la clase RNGCryptoServiceProvider de Cryptography
    //obteniendolos apartir de una secuencia de bytes aleatorios.
    public static string GenerarSalt()
    {
        byte[] bytesAleatorios = new byte[16];

        using (RNGCryptoServiceProvider generador = new RNGCryptoServiceProvider())
        {
            generador.GetBytes(bytesAleatorios);
        }

        return Convert.ToBase64String(bytesAleatorios);
    }

    //Genera hash con el algoritmo SHA256 de Criptography
    public static string GenerarHash(string contrasena, string salt)
    {
        using (SHA256 algoritmo = SHA256.Create())
        {
            byte[] bytesEntrada = Encoding.UTF8.GetBytes(contrasena + salt);
            byte[] bytesHash = algoritmo.ComputeHash(bytesEntrada);
            return Convert.ToBase64String(bytesHash);
        }
    }

    //Compara el hash almacenado con el que calcula nuevamente con la contrasena por parametro.
    public static bool ValidarContrasena(string contrasena, string salt, string hashAlmacenado)
    {
        string hashCalculado = HashHelper.GenerarHash(contrasena, salt);
        return string.Equals(hashCalculado, hashAlmacenado, StringComparison.Ordinal);
    }
}
