using System;

namespace Operativ.BE.Modelos;

public static class CalculoImpuestos
{
    public static decimal CalcularIva(decimal subtotal, decimal porcentajeIva)
    {
        return Math.Round(subtotal * porcentajeIva / 100m, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal CalcularTotal(decimal subtotal, decimal porcentajeIva)
    {
        return subtotal + CalcularIva(subtotal, porcentajeIva);
    }
}
