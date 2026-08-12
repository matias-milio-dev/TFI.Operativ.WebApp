using System;
using System.Data;
using System.Collections.Generic;
using Operativ.BE;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;
using Operativ.SEC;

namespace Operativ.BLL
{
    public interface IUsuarioBLL
    {
        Usuario IniciarSesion(string nombreUsuario, string clave, string direccionIp);
        void CerrarSesion();
        int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico,
            PerfilUsuario perfil, string idiomaPreferido, out string claveTemporalGenerada);
        void ModificarUsuario(Usuario usuario);
        void BajaUsuario(int idUsuario);
        void DesbloquearUsuario(int idUsuario);
        DataTable ListarUsuarios(string filtro, int numeroPagina, int tamanioPagina);
        Usuario ObtenerUsuario(int idUsuario);
        string RecuperarClave(string nombreUsuario);
        void CambiarClavePropia(string claveActual, string claveNueva);
    }

    public class UsuarioBLL : IUsuarioBLL
    {
        private readonly IUsuarioDAL _usuarioDAL = FabricaDAL.Instancia.CrearUsuarioDAL();
        private readonly IPermisosDAL _permisosDAL = FabricaDAL.Instancia.CrearPermisosDAL();
        private readonly ISistemaDAL _sistemaDAL = FabricaDAL.Instancia.CrearSistemaDAL();
        private readonly IBitacoraDAL _bitacoraDAL = FabricaDAL.Instancia.CrearBitacoraDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public Usuario IniciarSesion(string nombreUsuario, string clave, string direccionIp)
        {
            if (!_sistemaDAL.VerificarIntegridadLogin())
            {
                _bitacoraDAL.Registrar(null, "INTEGRIDAD", "Usuario", null,
                    "Verificación de integridad DVV fallida al intentar iniciar sesión.", "CRITICA", direccionIp);
                throw new ExcepcionNegocio(CodigosError.ErrorIntegridadCorrupta);
            }

            Usuario usuario = GestorAutenticacion.ValidarCredenciales(nombreUsuario, clave);

            List<Familia> familias = _usuarioDAL.ObtenerFamilias(usuario.IdUsuario);
            foreach (var familia in familias)
            {
                familia.Patentes = _permisosDAL.ObtenerPatentesDeFamilia(familia.IdFamilia);
            }
            List<Patente> patentesDirectas = _usuarioDAL.ObtenerPatentesDirectasDeUsuario(usuario.IdUsuario);

            usuario.Familias = familias;
            usuario.PatentesEfectivas = ArmadorPermisosEfectivos.Armar(familias, patentesDirectas);

            GestorAutenticacion.IniciarSesion(usuario);

            _bitacoraDAL.Registrar(usuario.IdUsuario, "LOGIN", "Usuario", usuario.IdUsuario.ToString(),
                "Inicio de sesión exitoso.", "INFORMATIVA", direccionIp);

            return usuario;
        }

        public void CerrarSesion()
        {
            var usuario = ContextoSesion.Actual.UsuarioActual;
            GestorAutenticacion.CerrarSesion();
            if (usuario != null)
            {
                _bitacoraDAL.Registrar(usuario.IdUsuario, "LOGOUT", "Usuario", usuario.IdUsuario.ToString(), "Cierre de sesión.", "INFORMATIVA", null);
            }
        }

        public int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico,
            PerfilUsuario perfil, string idiomaPreferido, out string claveTemporalGenerada)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)
                || string.IsNullOrWhiteSpace(nombreCompleto)
                || string.IsNullOrWhiteSpace(correoElectronico))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }

            FabricaUsuarioAbstracta fabrica = FabricaUsuarioAbstracta.ObtenerFabrica(perfil);
            Usuario nuevoUsuario = fabrica.CrearUsuario(nombreUsuario, nombreCompleto, correoElectronico, idiomaPreferido);

            claveTemporalGenerada = HashHelper.GenerarClaveTemporal();
            byte[] salt = HashHelper.GenerarSalt();
            nuevoUsuario.ClaveSalt = salt;
            nuevoUsuario.ClaveHash = HashHelper.CalcularHash(claveTemporalGenerada, salt);

            int idUsuarioNuevo = _usuarioDAL.Insertar(nuevoUsuario);

            _bitacoraBLL.Registrar("ALTA", "Usuario", idUsuarioNuevo.ToString(),
                $"Alta de usuario '{nombreUsuario}' con perfil {perfil}.", "ADVERTENCIA");

            string asunto = TextoHelper.Resolver("EmailAsuntoBienvenida", nuevoUsuario.IdiomaPreferido);
            string cuerpo = string.Format(TextoHelper.Resolver("EmailCuerpoBienvenida", nuevoUsuario.IdiomaPreferido), nombreCompleto, claveTemporalGenerada);
            EmailHelper.Enviar(correoElectronico, asunto, cuerpo);
            _bitacoraBLL.Registrar("ENVIO_EMAIL", "Usuario", idUsuarioNuevo.ToString(), $"Email de bienvenida enviado a '{correoElectronico}'.", "INFORMATIVA");

            return idUsuarioNuevo;
        }

        public void ModificarUsuario(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.NombreCompleto)
                || string.IsNullOrWhiteSpace(usuario.CorreoElectronico))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }

            _usuarioDAL.Modificar(usuario);
            _bitacoraBLL.Registrar("MODIFICACION", "Usuario", usuario.IdUsuario.ToString(), "Modificación de datos de usuario.", "ADVERTENCIA");
        }

        public void BajaUsuario(int idUsuario)
        {
            _usuarioDAL.Baja(idUsuario);
            _bitacoraBLL.Registrar("BAJA", "Usuario", idUsuario.ToString(), "Baja lógica de usuario.", "GRAVE");
        }

        public void DesbloquearUsuario(int idUsuario)
        {
            _usuarioDAL.Desbloquear(idUsuario);
            _bitacoraBLL.Registrar("DESBLOQUEO", "Usuario", idUsuario.ToString(), "Usuario desbloqueado manualmente.", "ADVERTENCIA");
        }

        public DataTable ListarUsuarios(string filtro, int numeroPagina, int tamanioPagina)
        {
            return _usuarioDAL.Listar(filtro, numeroPagina, tamanioPagina);
        }

        public Usuario ObtenerUsuario(int idUsuario)
        {
            return _usuarioDAL.ObtenerPorId(idUsuario);
        }

        public string RecuperarClave(string nombreUsuario)
        {
            Usuario usuario = _usuarioDAL.ObtenerPorNombreUsuario(nombreUsuario);
            if (usuario == null)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorRegistroNoEncontrado);
            }

            string claveTemporal = GestorRecuperacionClave.GenerarNuevaClaveTemporal(usuario.IdUsuario);

            _bitacoraDAL.Registrar(usuario.IdUsuario, "RECUPERACION_CLAVE", "Usuario", usuario.IdUsuario.ToString(),
                "Generación de contraseña temporal por recuperación.", "ADVERTENCIA", null);

            string asunto = TextoHelper.Resolver("EmailAsuntoRecuperacionClave", usuario.IdiomaPreferido);
            string cuerpo = string.Format(TextoHelper.Resolver("EmailCuerpoRecuperacionClave", usuario.IdiomaPreferido), usuario.NombreCompleto, claveTemporal);
            EmailHelper.Enviar(usuario.CorreoElectronico, asunto, cuerpo);
            _bitacoraDAL.Registrar(usuario.IdUsuario, "ENVIO_EMAIL", "Usuario", usuario.IdUsuario.ToString(),
                $"Email de recuperación de contraseña enviado a '{usuario.CorreoElectronico}'.", "INFORMATIVA", null);

            return claveTemporal;
        }

        public void CambiarClavePropia(string claveActual, string claveNueva)
        {
            var usuario = ContextoSesion.Actual.UsuarioActual;

            Usuario usuarioCompleto = _usuarioDAL.ObtenerPorNombreUsuario(usuario.NombreUsuario);
            if (!HashHelper.VerificarClave(claveActual, usuarioCompleto.ClaveHash, usuarioCompleto.ClaveSalt))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorUsuarioOClaveIncorrectos);
            }

            GestorRecuperacionClave.CambiarClave(usuario.IdUsuario, claveNueva);
            _bitacoraBLL.Registrar("CAMBIO_CLAVE", "Usuario", usuario.IdUsuario.ToString(), "Cambio de contraseña por el propio usuario.", "ADVERTENCIA");
        }
    }
}
