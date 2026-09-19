using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fachadas;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.WebServices.Modelos;

namespace Operativ.BLL.Implementaciones;
public class SuscripcionService : ISuscripcionService
{
    private readonly ISuscripcionRepositorio suscripcionRepositorio;
    private readonly IPlanRepositorio planRepositorio;
    private readonly IBitacoraService bitacoraService;
    private readonly ServicioFacade servicioFacade;

    public SuscripcionService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        suscripcionRepositorio = fabricaRepositorio.CrearSuscripcionRepositorio();
        planRepositorio = fabricaRepositorio.CrearPlanRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();

        servicioFacade = new ServicioFacade();
    }

    public List<Plan> ListarPlanes()
    {
        return planRepositorio.ListarActivos();
    }

    public List<Suscripcion> ListarSuscripciones(string filtro, int? idCliente, int numeroPagina, int tamanioPagina)
    {
        List<Suscripcion> suscripciones = suscripcionRepositorio.Listar(filtro, idCliente, numeroPagina, tamanioPagina);

        foreach (Suscripcion suscripcion in suscripciones)
        {
            AplicarEstadoDerivado(suscripcion);
        }

        return suscripciones;
    }

    public int ContarSuscripciones(string filtro, int? idCliente)
    {
        return suscripcionRepositorio.ContarSuscripciones(filtro, idCliente);
    }

    public Suscripcion ObtenerSuscripcionVigente(int idCliente)
    {
        Suscripcion suscripcion = suscripcionRepositorio.GetVigentePorCliente(idCliente);

        AplicarEstadoDerivado(suscripcion);

        return suscripcion;
    }

    public Suscripcion ObtenerSuscripcionPorId(int idSuscripcion)
    {
        Suscripcion suscripcion = suscripcionRepositorio.GetPorId(idSuscripcion)
            ?? throw new OperativException(TipoError.ErrorSuscripcionNoExiste);

        AplicarEstadoDerivado(suscripcion);

        return suscripcion;
    }

    public bool TieneSuscripcionActiva(int idCliente)
    {
        Suscripcion suscripcion = ObtenerSuscripcionVigente(idCliente);

        if (suscripcion == null)
        {
            return false;
        }

        return suscripcion.EsVigente;
    }

    public ResumenSuscripcionXml GenerarResumen(int idSuscripcion)
    {
        return servicioFacade.GenerarResumenSuscripcion(idSuscripcion);
    }

    public int AltaSuscripcion(int idCliente, int idPlan)
    {
        ValidarSinSuscripcionVigente(idCliente);

        Plan plan = planRepositorio.GetPorId(idPlan)
            ?? throw new OperativException(TipoError.ErrorSuscripcionNoExiste);

        Suscripcion suscripcion = new Suscripcion
        {
            IdCliente = idCliente,
            IdPlan = plan.IdPlan,
            PrecioAnual = plan.PrecioAnual,
            Estado = EstadoSuscripcion.PendientePago,
            FechaFinTrial = DateTime.Now.Date.AddDays(ConfiguracionAplicacion.DiasTrialSuscripcion)
        };

        int idSuscripcion = suscripcionRepositorio.Insertar(suscripcion);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.AltaSuscripcion, plan.Nombre);

        return idSuscripcion;
    }

    public void CancelarSuscripcion(int idSuscripcion)
    {
        Suscripcion suscripcion = ObtenerSuscripcionPorId(idSuscripcion);

        if (suscripcion.Estado == EstadoSuscripcion.Cancelada)
        {
            throw new OperativException(TipoError.ErrorSuscripcionNoExiste);
        }

        suscripcionRepositorio.Cancelar(idSuscripcion);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.CancelacionSuscripcion, suscripcion.NombrePlan);
    }

    private void AplicarEstadoDerivado(Suscripcion suscripcion)
    {
        if (suscripcion == null)
        {
            return;
        }

        if (suscripcion.Estado == EstadoSuscripcion.PendientePago && suscripcion.FechaFinTrial.Date < DateTime.Now.Date)
        {
            suscripcion.Estado = EstadoSuscripcion.Vencida;
            return;
        }

        if (suscripcion.Estado == EstadoSuscripcion.Activa && suscripcion.FechaVencimiento.HasValue && suscripcion.FechaVencimiento.Value.Date < DateTime.Now.Date)
        {
            suscripcion.Estado = EstadoSuscripcion.Vencida;
        }
    }

    private void ValidarSinSuscripcionVigente(int idCliente)
    {
        Suscripcion vigente = ObtenerSuscripcionVigente(idCliente);

        if (vigente != null && vigente.EsVigente)
        {
            throw new OperativException(TipoError.ErrorSuscripcionVigenteExistente);
        }
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
