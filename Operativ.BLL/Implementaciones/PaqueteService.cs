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
public class PaqueteService : IPaqueteService
{
    private readonly IPaqueteRepositorio paqueteRepositorio;
    private readonly IProgramaRepositorio programaRepositorio;
    private readonly IBitacoraService bitacoraService;

    public PaqueteService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        paqueteRepositorio = fabricaRepositorio.CrearPaqueteRepositorio();
        programaRepositorio = fabricaRepositorio.CrearProgramaRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<Paquete> ListarPaquetes(string filtro, int numeroPagina, int tamanioPagina)
    {
        return paqueteRepositorio.Listar(filtro, numeroPagina, tamanioPagina);
    }

    public int ContarPaquetes(string filtro)
    {
        return paqueteRepositorio.ContarPaquetes(filtro);
    }

    public Paquete ObtenerPaquetePorId(int idPaquete)
    {
        Paquete paquete = paqueteRepositorio.GetPorId(idPaquete)
            ?? throw new OperativException(TipoError.ErrorPaqueteNoExiste);

        paquete.Programas = paqueteRepositorio.GetProgramasDePaquete(idPaquete);

        return paquete;
    }

    public List<Programa> ListarProgramas()
    {
        return programaRepositorio.ListarTodos();
    }

    public List<Paquete> ListarPaquetesHabilitados(int? idPaqueteIncluir)
    {
        return paqueteRepositorio.ListarHabilitados(idPaqueteIncluir);
    }

    public int AltaPaquete(Paquete paquete, int[] idsPrograma)
    {
        ValidarPaquete(paquete, idsPrograma, null);

        int idPaquete = paqueteRepositorio.Insertar(paquete);
        paqueteRepositorio.AsignarProgramas(idPaquete, idsPrograma);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.AltaPaquete, paquete.Nombre);

        return idPaquete;
    }

    public void ModificarPaquete(Paquete paquete, int[] idsPrograma)
    {
        ValidarPaquete(paquete, idsPrograma, paquete.IdPaquete);

        paqueteRepositorio.Modificar(paquete);
        SincronizarProgramas(paquete.IdPaquete, idsPrograma);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.ModificacionPaquete, paquete.Nombre);
    }

    public void BajaPaquete(int idPaquete)
    {
        Paquete paquete = ObtenerPaquetePorId(idPaquete);

        paqueteRepositorio.BajaLogica(idPaquete);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.BajaPaquete, paquete.Nombre);
    }

    private void ValidarPaquete(Paquete paquete, int[] idsPrograma, int? idPaqueteExcluir)
    {
        if (idsPrograma.Length == 0)
        {
            throw new OperativException(TipoError.ErrorPaqueteSinProgramas);
        }

        if (paqueteRepositorio.ExisteNombre(paquete.Nombre, idPaqueteExcluir))
        {
            throw new OperativException(TipoError.ErrorPaqueteYaExiste, new string[] { paquete.Nombre });
        }
    }

    private void SincronizarProgramas(int idPaquete, int[] idsPrograma)
    {
        List<Programa> programasActuales = paqueteRepositorio.GetProgramasDePaquete(idPaquete);
        List<int> idsActuales = new List<int>();

        foreach (Programa programa in programasActuales)
        {
            idsActuales.Add(programa.IdPrograma);
        }

        List<int> idsSeleccionados = new List<int>();

        foreach (int idPrograma in idsPrograma)
        {
            idsSeleccionados.Add(idPrograma);
        }

        List<int> idsAAsignar = new List<int>();

        foreach (int idPrograma in idsSeleccionados)
        {
            if (!idsActuales.Contains(idPrograma))
            {
                idsAAsignar.Add(idPrograma);
            }
        }

        List<int> idsAQuitar = new List<int>();

        foreach (int idPrograma in idsActuales)
        {
            if (!idsSeleccionados.Contains(idPrograma))
            {
                idsAQuitar.Add(idPrograma);
            }
        }

        paqueteRepositorio.AsignarProgramas(idPaquete, idsAAsignar.ToArray());
        paqueteRepositorio.QuitarProgramas(idPaquete, idsAQuitar.ToArray());
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