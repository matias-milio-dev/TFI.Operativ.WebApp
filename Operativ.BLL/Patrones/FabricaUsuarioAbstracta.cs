using Operativ.BE;

namespace Operativ.BLL.Patrones
{
    public abstract class FabricaUsuarioAbstracta
    {
        public abstract PerfilUsuario Perfil { get; }

        public virtual Usuario CrearUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, string idiomaPreferido)
        {
            return new Usuario
            {
                NombreUsuario = nombreUsuario,
                NombreCompleto = nombreCompleto,
                CorreoElectronico = correoElectronico,
                IdPerfil = (int)Perfil,
                CodigoPerfil = Perfil.ToString().ToUpperInvariant(),
                IdiomaPreferido = string.IsNullOrEmpty(idiomaPreferido) ? "es" : idiomaPreferido,
                ClaveTemporal = true,
                Activo = true
            };
        }

        public static FabricaUsuarioAbstracta ObtenerFabrica(PerfilUsuario perfil)
        {
            switch (perfil)
            {
                case PerfilUsuario.WebMaster: return new FabricaUsuarioWebMaster();
                case PerfilUsuario.Administrador: return new FabricaUsuarioAdministrador();
                case PerfilUsuario.Comercial: return new FabricaUsuarioComercial();
                case PerfilUsuario.Cliente: return new FabricaUsuarioCliente();
                default: return new FabricaUsuarioCliente();
            }
        }
    }

    public class FabricaUsuarioWebMaster : FabricaUsuarioAbstracta
    {
        public override PerfilUsuario Perfil => PerfilUsuario.WebMaster;
    }

    public class FabricaUsuarioAdministrador : FabricaUsuarioAbstracta
    {
        public override PerfilUsuario Perfil => PerfilUsuario.Administrador;
    }

    public class FabricaUsuarioComercial : FabricaUsuarioAbstracta
    {
        public override PerfilUsuario Perfil => PerfilUsuario.Comercial;
    }

    public class FabricaUsuarioCliente : FabricaUsuarioAbstracta
    {
        public override PerfilUsuario Perfil => PerfilUsuario.Cliente;
    }
}
