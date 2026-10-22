using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers;
public static class ClaveHelper
{
    public static bool EsCompleja(string clave)
    {
        string patron = @"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$";
        return Regex.IsMatch(clave, patron);
    }

    public static string GenerarContrasenaTemporal()
    {
        string candidato;

        using (RNGCryptoServiceProvider generadorAleatorio = new RNGCryptoServiceProvider())
        {
            do
            {
                candidato = GenerarCandidatoContrasenaTemporal(generadorAleatorio);
            }
            while (!EsCompleja(candidato));
        }

        return candidato;
    }

    private static string GenerarCandidatoContrasenaTemporal(RNGCryptoServiceProvider generadorAleatorio)
    {
        string caracteresValidos = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%&*?";
        StringBuilder resultado = new StringBuilder();
        byte[] bytesAleatorios = new byte[1];

        for (int indice = 0; indice < ConfiguracionAplicacion.LongitudContrasenaTemporal; indice++)
        {
            generadorAleatorio.GetBytes(bytesAleatorios);
            int posicion = bytesAleatorios[0] % caracteresValidos.Length;
            resultado.Append(caracteresValidos[posicion]);
        }

        return resultado.ToString();
    }
}
