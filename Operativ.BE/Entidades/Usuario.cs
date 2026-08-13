using System.Collections.Generic;

namespace Operativ.BE.Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; }

        public string Contrasena { get; set; }

        public string Salt { get; set; }

        public string Email { get; set; }

        public string NombreCompleto { get; set; }

        public bool Bloqueado { get; set; }

        public int IntentosFallidos { get; set; }

        public bool Activo { get; set; }

        public List<Familia> Familias { get; set; }

        public Usuario()
        {
            Familias = new List<Familia>();
        }
    }
}
