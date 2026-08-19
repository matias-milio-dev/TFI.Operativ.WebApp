using System;
using Operativ.BE.Enums;

namespace Operativ.BE.Entidades;
public class Bitacora
{
    public int IdBitacora { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime FechaHora { get; set; }

    public TipoAccionBitacora Accion { get; set; }

    public CriticidadBitacora Criticidad { get; set; }

    public string Descripcion { get; set; }
}
