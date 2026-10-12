using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Operativ.BE.Enums;

namespace Operativ.BLL.Helpers;
public static class TarjetaHelper
{
    private static readonly Regex PatronVisa = new Regex(@"^4\d*$");
    private static readonly Regex PatronMastercard = new Regex(@"^(5[1-5]|2(2[2-9][1-9]|2[3-9]\d|[3-6]\d\d|7[01]\d|720))\d*$");
    private static readonly Regex PatronAmericanExpress = new Regex(@"^3[47]\d*$");
    private static readonly Regex PatronVencimiento = new Regex(@"^(0[1-9]|1[0-2])/(\d{2})$");
    private static readonly Regex PatronTitular = new Regex(@"^[\p{L}][\p{L}\s'\.\-]*$");

    public static string NormalizarNumero(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return string.Empty;
        }

        return numero.Replace(" ", string.Empty).Replace("-", string.Empty);
    }

    public static MarcaTarjeta DetectarMarca(string numero)
    {
        string digitos = NormalizarNumero(numero);

        if (PatronVisa.IsMatch(digitos))
        {
            return MarcaTarjeta.Visa;
        }

        if (PatronAmericanExpress.IsMatch(digitos))
        {
            return MarcaTarjeta.AmericanExpress;
        }

        if (PatronMastercard.IsMatch(digitos))
        {
            return MarcaTarjeta.Mastercard;
        }

        return MarcaTarjeta.Desconocida;
    }

    public static bool EsNumeroValido(string numero)
    {
        string digitos = NormalizarNumero(numero);
        MarcaTarjeta marca = DetectarMarca(digitos);

        if (marca == MarcaTarjeta.Desconocida || !Regex.IsMatch(digitos, @"^\d+$"))
        {
            return false;
        }

        int longitudEsperada = marca == MarcaTarjeta.AmericanExpress ? 15 : 16;

        if (digitos.Length != longitudEsperada && !(marca == MarcaTarjeta.Visa && digitos.Length == 13))
        {
            return false;
        }

        return CumpleLuhn(digitos);
    }

    public static bool EsTitularValido(string titular)
    {
        if (string.IsNullOrWhiteSpace(titular))
        {
            return false;
        }

        string normalizado = titular.Trim();

        return normalizado.Length >= 3 && normalizado.Length <= 60 && PatronTitular.IsMatch(normalizado);
    }

    public static bool EsVencimientoValido(string vencimiento, DateTime hoy)
    {
        if (string.IsNullOrWhiteSpace(vencimiento))
        {
            return false;
        }

        Match coincidencia = PatronVencimiento.Match(vencimiento.Trim());

        if (!coincidencia.Success)
        {
            return false;
        }

        int mes = int.Parse(coincidencia.Groups[1].Value, CultureInfo.InvariantCulture);
        int anio = 2000 + int.Parse(coincidencia.Groups[2].Value, CultureInfo.InvariantCulture);

        DateTime ultimoDiaVigente = new DateTime(anio, mes, 1).AddMonths(1).AddDays(-1);

        return ultimoDiaVigente.Date >= hoy.Date && anio <= hoy.Year + 20;
    }

    public static bool EsCodigoSeguridadValido(string codigo, string numero)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return false;
        }

        int longitud = DetectarMarca(numero) == MarcaTarjeta.AmericanExpress ? 4 : 3;

        return Regex.IsMatch(codigo.Trim(), @"^\d{" + longitud + "}$");
    }

    private static bool CumpleLuhn(string digitos)
    {
        int suma = 0;
        bool duplicar = false;

        for (int indice = digitos.Length - 1; indice >= 0; indice--)
        {
            int digito = digitos[indice] - '0';

            if (duplicar)
            {
                digito *= 2;

                if (digito > 9)
                {
                    digito -= 9;
                }
            }

            suma += digito;
            duplicar = !duplicar;
        }

        return suma % 10 == 0;
    }
}
