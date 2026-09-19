using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BLL.Contratos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;

namespace Operativ.BLL.Implementaciones;
public class IncidenteService : IIncidenteService
{
    private const string PrefijoNumeroIncidente = "INC";

    private readonly IIncidenteRepositorio incidenteRepositorio;
    private readonly IActivoRepositorio activoRepositorio;
    private readonly IBitacoraService bitacoraService;

    public IncidenteService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        incidenteRepositorio = fabricaRepositorio.CrearIncidenteRepositorio();
        activoRepositorio = fabricaRepositorio.CrearActivoRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<Incidente> ListarIncidentes(string filtro, EstadoIncidente? estado, int? idCliente, int numeroPagina, int tamanioPagina)
    {
        return incidenteRepositorio.Listar(filtro, estado, idCliente, numeroPagina, tamanioPagina);
    }

    public int ContarIncidentes(string filtro, EstadoIncidente? estado, int? idCliente)
    {
        return incidenteRepositorio.ContarIncidentes(filtro, estado, idCliente);
    }

    public Incidente ObtenerIncidentePorId(int idIncidente)
    {
        return incidenteRepositorio.GetPorId(idIncidente)
            ?? throw new OperativException(TipoError.ErrorIncidenteNoExiste);
    }

    public int AltaIncidente(Incidente incidente, int idCliente)
    {
        ValidarActivoDelCliente(incidente.IdActivo, idCliente);

        incidente.NumeroIncidente = GenerarNumeroIncidente();
        incidente.Estado = EstadoIncidente.Abierto;

        int idIncidente = incidenteRepositorio.Insertar(incidente);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.AltaIncidente, incidente.NumeroIncidente);

        return idIncidente;
    }

    public void CerrarIncidente(int idIncidente, string comentarioResolucion)
    {
        Incidente incidente = ObtenerIncidentePorId(idIncidente);

        if (incidente.EstaCerrado)
        {
            throw new OperativException(TipoError.ErrorIncidenteYaCerrado, new string[] { incidente.NumeroIncidente });
        }

        incidenteRepositorio.Cerrar(idIncidente, comentarioResolucion);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.CierreIncidente, incidente.NumeroIncidente);
    }

    private void ValidarActivoDelCliente(int idActivo, int idCliente)
    {
        Activo activo = activoRepositorio.GetPorId(idActivo);

        if (activo == null || !activo.Habilitado || activo.IdCliente != idCliente)
        {
            throw new OperativException(TipoError.ErrorActivoNoExiste);
        }
    }

    private string GenerarNumeroIncidente()
    {
        int anio = DateTime.Now.Year;
        int secuencia = incidenteRepositorio.ContarIncidentesDelAnio(anio) + 1;

        return PrefijoNumeroIncidente + "-" + anio.ToString() + "-" + secuencia.ToString("D5");
    }

    private int? ObtenerIdUsuarioActual()
    {
        SesionHandler sesionHandler = new SesionHandler();
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null)
        {
            return null;
        }

        return usuario.IdUsuario;
    }
}
