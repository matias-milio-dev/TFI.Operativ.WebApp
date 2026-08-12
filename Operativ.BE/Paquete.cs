namespace Operativ.BE
{
    public class Paquete
    {
        public int IdPaquete { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public int CantidadActivosIncluidos { get; set; }
        public bool Activo { get; set; }
    }
}
