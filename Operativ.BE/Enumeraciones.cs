using System;

namespace Operativ.BE
{
    public enum PerfilUsuario
    {
        WebMaster = 1,
        Administrador = 2,
        Comercial = 3,
        Cliente = 4
    }

    public enum Criticidad
    {
        Informativa = 1,
        Advertencia = 2,
        Grave = 3,
        Critica = 4
    }

    public enum EstadoSuscripcion
    {
        Activa = 1,
        Vencida = 2,
        Cancelada = 3,
        PendientePago = 4
    }

    public enum MedioPago
    {
        Tarjeta = 1,
        Transferencia = 2,
        Efectivo = 3
    }
}
