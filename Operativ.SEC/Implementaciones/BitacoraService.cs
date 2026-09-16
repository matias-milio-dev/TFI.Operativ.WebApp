using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;
public class BitacoraService : IBitacoraService
{
    private const int LongitudMaximaDescripcion = 300;

    private readonly IBitacoraRepositorio bitacoraRepositorio;

    public BitacoraService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        bitacoraRepositorio = fabricaRepositorio.CrearBitacoraRepositorio();
    }

    public void Registrar(int? idUsuario, TipoAccionBitacora accion)
    {
        Registrar(idUsuario, accion, null);
    }

    public void Registrar(int? idUsuario, TipoAccionBitacora accion, string detalleAdicional)
    {
        AccionBitacora definicion = AccionBitacora.ObtenerPorTipo(accion);
        string descripcion = string.Format(definicion.Descripcion, ConfiguracionAplicacion.IntentosMaximosLogin);

        if (!string.IsNullOrEmpty(detalleAdicional))
        {
            descripcion = descripcion + ": " + detalleAdicional;

            if (descripcion.Length > LongitudMaximaDescripcion)
            {
                descripcion = descripcion.Substring(0, LongitudMaximaDescripcion);
            }
        }

        Bitacora entrada = new Bitacora
        {
            IdUsuario = NormalizarIdUsuario(idUsuario),
            Accion = accion,
            Criticidad = definicion.Criticidad,
            Descripcion = descripcion
        };

        bitacoraRepositorio.Registrar(entrada);
    }

    // El acceso de emergencia no tiene un Usuario persistido: llega con IdUsuario 0 y la
    // FK FK_Bitacora_Usuario rechaza el insert. Se audita sin usuario asociado.
    private static int? NormalizarIdUsuario(int? idUsuario)
    {
        if (!idUsuario.HasValue || idUsuario.Value <= 0)
        {
            return null;
        }

        return idUsuario;
    }

    public List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina)
    {
        DateTime fechaDesdeEfectiva = ObtenerFechaDesdeEfectiva(fechaDesde);
        DateTime fechaHastaEfectiva = ObtenerFechaHastaEfectiva(fechaHasta);

        return bitacoraRepositorio.Buscar(filtroUsuario, accion, criticidad, fechaDesdeEfectiva, fechaHastaEfectiva, numeroPagina, tamanioPagina);
    }

    public int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        DateTime fechaDesdeEfectiva = ObtenerFechaDesdeEfectiva(fechaDesde);
        DateTime fechaHastaEfectiva = ObtenerFechaHastaEfectiva(fechaHasta);

        return bitacoraRepositorio.ContarRegistros(filtroUsuario, accion, criticidad, fechaDesdeEfectiva, fechaHastaEfectiva);
    }

    private DateTime ObtenerFechaDesdeEfectiva(DateTime? fechaDesde)
    {
        DateTime limiteInferior = DateTime.Today.AddDays(-ConfiguracionAplicacion.DiasHistorialBitacora);

        if (fechaDesde.HasValue && fechaDesde.Value.Date > limiteInferior)
        {
            return fechaDesde.Value.Date;
        }

        return limiteInferior;
    }

    private DateTime ObtenerFechaHastaEfectiva(DateTime? fechaHasta)
    {
        DateTime hoy = DateTime.Today;

        if (fechaHasta.HasValue && fechaHasta.Value.Date < hoy)
        {
            return fechaHasta.Value.Date;
        }

        return hoy;
    }
}
