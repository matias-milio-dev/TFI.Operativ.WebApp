using System;
using System.Collections.Generic;

namespace Operativ.BE
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string CorreoElectronico { get; set; }
        public byte[] ClaveHash { get; set; }
        public byte[] ClaveSalt { get; set; }
        public int IdPerfil { get; set; }
        public string CodigoPerfil { get; set; }
        public byte CantidadIntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }
        public bool ClaveTemporal { get; set; }
        public bool Activo { get; set; }
        public string IdiomaPreferido { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimoLogin { get; set; }

        public List<Familia> Familias { get; set; }
        public List<Patente> PatentesEfectivas { get; set; }

        public Usuario()
        {
            Familias = new List<Familia>();
            PatentesEfectivas = new List<Patente>();
        }
    }
}
