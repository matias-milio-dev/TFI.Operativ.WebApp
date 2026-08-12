using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE;
using Operativ.DAL;
using Operativ.SEC;

namespace Operativ.BLL
{
    public interface IBitacoraBLL
    {
        void Registrar(string accion, string entidadAfectada, string idEntidadAfectada,
            string descripcion, string codigoCriticidad = "INFORMATIVA", string direccionIp = null);
        List<Bitacora> Listar(DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario,
            string accion, string codigoCriticidad, int numeroPagina, int tamanioPagina);
    }

    public class BitacoraBLL : IBitacoraBLL
    {
        private readonly IBitacoraDAL _bitacoraDAL = FabricaDAL.Instancia.CrearBitacoraDAL();

        public void Registrar(string accion, string entidadAfectada, string idEntidadAfectada,
            string descripcion, string codigoCriticidad = "INFORMATIVA", string direccionIp = null)
        {
            int? idUsuario = ContextoSesion.Actual.EstaAutenticado ? ContextoSesion.Actual.UsuarioActual.IdUsuario : (int?)null;
            _bitacoraDAL.Registrar(idUsuario, accion, entidadAfectada, idEntidadAfectada, descripcion, codigoCriticidad, direccionIp);
        }

        public List<Bitacora> Listar(DateTime? fechaDesde, DateTime? fechaHasta, int? idUsuario,
            string accion, string codigoCriticidad, int numeroPagina, int tamanioPagina)
        {
            DataTable tabla = _bitacoraDAL.Listar(fechaDesde, fechaHasta, idUsuario, accion, codigoCriticidad, numeroPagina, tamanioPagina);

            var lista = new List<Bitacora>();
            foreach (DataRow fila in tabla.Rows)
            {
                lista.Add(new Bitacora
                {
                    IdBitacora = (long)fila["IdBitacora"],
                    FechaHora = (DateTime)fila["FechaHora"],
                    NombreUsuario = fila["NombreUsuario"] as string,
                    Accion = (string)fila["Accion"],
                    EntidadAfectada = (string)fila["EntidadAfectada"],
                    IdEntidadAfectada = fila["IdEntidadAfectada"] as string,
                    Descripcion = fila["Descripcion"] as string,
                    CodigoCriticidad = (string)fila["CodigoCriticidad"],
                    DireccionIP = fila["DireccionIP"] as string
                });
            }
            return lista;
        }
    }
}
