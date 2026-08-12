using System;
using System.Collections.Generic;

namespace Operativ.WebServices
{
    public class IncidenteXml
    {
        public int IdIncidente { get; set; }
        public int IdActivo { get; set; }
        public string Descripcion { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    public class CatalogoXml
    {
        public string Filtro { get; set; }
        public DateTime FechaConsulta { get; set; }
        public List<PaqueteXml> Paquetes { get; set; } = new List<PaqueteXml>();
    }

    public class PaqueteXml
    {
        public int IdPaquete { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioBase { get; set; }
        public int CantidadActivosIncluidos { get; set; }
    }

    public class ResumenSuscripcionXml
    {
        public string Cuit { get; set; }
        public string RazonSocial { get; set; }
        public string CorreoElectronico { get; set; }
        public int IdPaquete { get; set; }
        public string NombrePaquete { get; set; }
        public decimal PrecioBase { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string ResumenHtml { get; set; }
    }
}
