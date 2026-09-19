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
public class ActivoService : IActivoService
{
    private readonly IActivoRepositorio activoRepositorio;
    private readonly IBitacoraService bitacoraService;

    public ActivoService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        activoRepositorio = fabricaRepositorio.CrearActivoRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<Activo> ListarActivos(string filtro, int numeroPagina, int tamanioPagina)
    {
        return activoRepositorio.Listar(filtro, numeroPagina, tamanioPagina);
    }

    public int ContarActivos(string filtro)
    {
        return activoRepositorio.ContarActivos(filtro);
    }

    public Activo ObtenerActivoPorId(int idActivo)
    {
        return activoRepositorio.GetPorId(idActivo)
            ?? throw new OperativException(TipoError.ErrorActivoNoExiste);
    }

    public int AltaActivo(Activo activo)
    {
        ValidarNumeroSerie(activo, null);

        int idActivo = activoRepositorio.Insertar(activo);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.AltaActivo, activo.NumeroSerie);

        return idActivo;
    }

    public void ModificarActivo(Activo activo)
    {
        ValidarNumeroSerie(activo, activo.IdActivo);

        activoRepositorio.Modificar(activo);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.ModificacionActivo, activo.NumeroSerie);
    }

    public void BajaActivo(int idActivo)
    {
        Activo activo = ObtenerActivoPorId(idActivo);

        activoRepositorio.BajaLogica(idActivo);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.BajaActivo, activo.NumeroSerie);
    }

    private void ValidarNumeroSerie(Activo activo, int? idActivoExcluir)
    {
        if (activoRepositorio.ExisteNumeroSerie(activo.NumeroSerie, idActivoExcluir))
        {
            throw new OperativException(TipoError.ErrorActivoYaExiste, new string[] { activo.NumeroSerie });
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
