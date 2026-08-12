using System;
using System.Security.Cryptography;
using System.Text;

namespace Operativ.SEC
{
    public static class HashHelper
    {
        private const int TamanioSaltBytes = 32;

        public static byte[] GenerarSalt()
        {
            var salt = new byte[TamanioSaltBytes];
            using (var generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(salt);
            }
            return salt;
        }

        public static byte[] CalcularHash(string clave, byte[] salt)
        {
            if (clave == null) throw new ArgumentNullException(nameof(clave));
            if (salt == null) throw new ArgumentNullException(nameof(salt));

            byte[] claveBytes = Encoding.Unicode.GetBytes(clave);
            byte[] datos = new byte[salt.Length + claveBytes.Length];
            Buffer.BlockCopy(salt, 0, datos, 0, salt.Length);
            Buffer.BlockCopy(claveBytes, 0, datos, salt.Length, claveBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(datos);
            }
        }

        public static bool VerificarClave(string claveIngresada, byte[] hashAlmacenado, byte[] salt)
        {
            byte[] hashCalculado = CalcularHash(claveIngresada, salt);
            return CompararEnTiempoConstante(hashCalculado, hashAlmacenado);
        }

        private static bool CompararEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a == null
                || b == null
                || a.Length != b.Length)
            {
                return false;
            }
            int diferencia = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diferencia |= a[i] ^ b[i];
            }
            return diferencia == 0;
        }

        public static string GenerarClaveTemporal(int longitud = 10)
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var bytesAleatorios = new byte[longitud];
            using (var generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(bytesAleatorios);
            }

            var resultado = new StringBuilder(longitud);
            foreach (byte b in bytesAleatorios)
            {
                resultado.Append(alfabeto[b % alfabeto.Length]);
            }
            return resultado.ToString();
        }
    }
}
