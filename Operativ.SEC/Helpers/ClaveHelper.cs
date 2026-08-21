using System;
using System.Text;
using System.Text.RegularExpressions;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers;
public static class ClaveHelper
{
    public static bool EsCompleja(string clave)
    {
        string patron = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$";
        return Regex.IsMatch(clave, patron);
    }

    public static string GenerarContrasenaTemporal()
    {
        Random generadorAleatorio = new Random();
        string candidato;

        do
        {
            candidato = GenerarCandidatoContrasenaTemporal(generadorAleatorio);
        }
        while (!EsCompleja(candidato));

        return candidato;
    }

    private static string GenerarCandidatoContrasenaTemporal(Random generadorAleatorio)
    {
        string caracteresValidos = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        StringBuilder resultado = new StringBuilder();

        for (int indice = 0; indice < ConfiguracionAplicacion.LongitudContrasenaTemporal; indice++)
        {
            int posicion = generadorAleatorio.Next(caracteresValidos.Length);
            resultado.Append(caracteresValidos[posicion]);
        }

        return resultado.ToString();
    }
}
