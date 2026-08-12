using System;

namespace Operativ.BE
{
    public class Bitacora
    {
        public long IdBitacora { get; set; }
        public int? IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaHora { get; set; }
        public string Accion { get; set; }
        public string EntidadAfectada { get; set; }
        public string IdEntidadAfectada { get; set; }
        public string Descripcion { get; set; }
        public string CodigoCriticidad { get; set; }
        public string DireccionIP { get; set; }
    }
}
