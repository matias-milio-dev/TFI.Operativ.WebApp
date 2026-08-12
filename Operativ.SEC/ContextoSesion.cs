using System.Collections.Generic;
using System.Linq;
using System.Web;
using Operativ.BE;

namespace Operativ.SEC
{
    public class ContextoSesion
    {
        private const string ClaveSesion = "Operativ.SEC.ContextoSesion";

        public Usuario UsuarioActual { get; private set; }
        public List<Familia> Familias { get; private set; } = new List<Familia>();
        public List<Patente> PatentesEfectivas { get; private set; } = new List<Patente>();

        public bool EstaAutenticado => UsuarioActual != null;

        public string CodigoPerfil => UsuarioActual?.CodigoPerfil;

        private ContextoSesion() { }

        public static ContextoSesion Actual
        {
            get
            {
                if (HttpContext.Current?.Session == null)
                {
                    return new ContextoSesion();
                }

                var contexto = HttpContext.Current.Session[ClaveSesion] as ContextoSesion;
                if (contexto == null)
                {
                    contexto = new ContextoSesion();
                    HttpContext.Current.Session[ClaveSesion] = contexto;
                }
                return contexto;
            }
        }

        public void IniciarSesion(Usuario usuario, List<Familia> familias, List<Patente> patentesEfectivas)
        {
            UsuarioActual = usuario;
            Familias = familias ?? new List<Familia>();
            PatentesEfectivas = patentesEfectivas ?? new List<Patente>();

            if (HttpContext.Current?.Session != null)
            {
                HttpContext.Current.Session[ClaveSesion] = this;
            }
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
            Familias = new List<Familia>();
            PatentesEfectivas = new List<Patente>();
            HttpContext.Current?.Session?.Clear();
            HttpContext.Current?.Session?.Abandon();
        }

        public bool TienePatente(string codigoPatente)
        {
            return PatentesEfectivas.Any(p => p.Codigo == codigoPatente);
        }

        public bool PerteneceAFamilia(string nombreFamilia)
        {
            return Familias.Any(f => f.Nombre == nombreFamilia);
        }
    }
}
