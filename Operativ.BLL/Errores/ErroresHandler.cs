using System;
using System.Data.SqlClient;
using Operativ.BE.Enums;

namespace Operativ.BLL.Errores
{
    public class ErroresHandler
    {
        public string GetMensaje(TipoError tipoError)
        {
            return GetMensaje(tipoError, null);
        }

        public string GetMensaje(TipoError tipoError, string[] parametros)
        {
            string codigo = GetCodigo(tipoError);
            string texto = GetTexto(tipoError, parametros);
            return codigo + " - " + texto;
        }

        public string GetMensaje(OperativException excepcion)
        {
            return GetMensaje(excepcion.TipoError, excepcion.Parametros);
        }

        public OperativException TraducirExcepcion(Exception excepcion)
        {
            if (excepcion is OperativException)
            {
                return (OperativException)excepcion;
            }

            if (excepcion is SqlException)
            {
                return new OperativException(TipoError.ErrorConexionBaseDatos);
            }

            return new OperativException(TipoError.ErrorConexionBaseDatos);
        }

        private string GetCodigo(TipoError tipoError)
        {
            switch (tipoError)
            {
                case TipoError.ErrorUsuarioNoExiste:
                    return "ERR01";
                case TipoError.ErrorContrasenaIncorrecta:
                    return "ERR02";
                case TipoError.ErrorUsuarioBloqueado:
                    return "ERR03";
                case TipoError.ErrorConexionBaseDatos:
                    return "ERR05";
                case TipoError.ErrorSesionExpirada:
                    return "ERR11";
                default:
                    return "ERR00";
            }
        }

        private string GetTexto(TipoError tipoError, string[] parametros)
        {
            switch (tipoError)
            {
                case TipoError.ErrorUsuarioNoExiste:
                    return "El usuario no existe en el sistema";
                case TipoError.ErrorContrasenaIncorrecta:
                    return "La contraseña ingresada es incorrecta (Quedan " + parametros[0] + " intentos)";
                case TipoError.ErrorUsuarioBloqueado:
                    return "El usuario " + parametros[0] + " ha sido bloqueado";
                case TipoError.ErrorConexionBaseDatos:
                    return "No se puede conectar a la base de datos";
                case TipoError.ErrorSesionExpirada:
                    return "No hay sesión iniciada o expiró";
                default:
                    return "Error desconocido";
            }
        }
    }
}
