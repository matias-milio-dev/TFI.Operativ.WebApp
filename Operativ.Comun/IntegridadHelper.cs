using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Operativ.Comun
{
    public static class IntegridadHelper
    {
        public static readonly IReadOnlyDictionary<string, string> TablasCriticas = new Dictionary<string, string>
        {
            { "Usuario", "IdUsuario" },
            { "Familia", "IdFamilia" },
            { "Patente", "IdPatente" },
            { "UsuarioFamilia", "IdUsuarioFamilia" },
            { "FamiliaPatente", "IdFamiliaPatente" },
            { "UsuarioPatente", "IdUsuarioPatente" },
            { "Bitacora", "IdBitacora" },
            { "Cliente", "IdCliente" },
            { "Paquete", "IdPaquete" },
            { "Suscripcion", "IdSuscripcion" },
            { "Pago", "IdPago" },
            { "Factura", "IdFactura" },
            { "Activo", "IdActivo" },
            { "Incidente", "IdIncidente" },
        };

        public static string FormatoValorGenerico(object valor)
        {
            if (valor == null || valor == DBNull.Value) return string.Empty;
            if (valor is bool booleano) return FormatoBit(booleano);
            if (valor is byte[] binario) return "0x" + ConvertirAHex(binario).ToUpperInvariant();
            if (valor is DateTime fecha) return FormatoFecha(fecha);
            if (valor is decimal decimal_) return decimal_.ToString(CultureInfo.InvariantCulture);
            return Convert.ToString(valor, CultureInfo.InvariantCulture);
        }

        public static byte[] CalcularDigitoVerificador(string valores)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.Unicode.GetBytes(valores ?? string.Empty));
            }
        }

        public static string ConvertirAHex(byte[] valor)
        {
            if (valor == null) return string.Empty;
            var constructor = new StringBuilder(valor.Length * 2);
            foreach (byte b in valor)
            {
                constructor.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }
            return constructor.ToString();
        }

        public static string FormatoBit(bool valor) => valor ? "1" : "0";

        public static string FormatoDecimal(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);

        public static string FormatoFecha(System.DateTime valor) => valor.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
    }
}
