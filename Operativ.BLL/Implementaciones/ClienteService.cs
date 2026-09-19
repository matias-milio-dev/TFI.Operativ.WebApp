using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;

namespace Operativ.BLL.Implementaciones;
public class ClienteService : IClienteService
{
    private readonly IClienteRepositorio clienteRepositorio;
    private readonly IUsuarioService usuarioService;
    private readonly IBitacoraService bitacoraService;

    public ClienteService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        clienteRepositorio = fabricaRepositorio.CrearClienteRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<Cliente> ListarClientes(string filtro, int numeroPagina, int tamanioPagina)
    {
        return clienteRepositorio.Listar(filtro, numeroPagina, tamanioPagina);
    }

    public int ContarClientes(string filtro)
    {
        return clienteRepositorio.ContarClientes(filtro);
    }

    public List<Cliente> ListarClientesActivos(int? idClienteIncluir)
    {
        return clienteRepositorio.ListarActivos(idClienteIncluir);
    }

    public List<Usuario> ListarUsuariosClienteDisponibles()
    {
        return usuarioService.ListarUsuariosClienteSinEmpresa();
    }

    public Cliente ObtenerClientePorId(int idCliente)
    {
        return clienteRepositorio.GetPorId(idCliente)
            ?? throw new OperativException(TipoError.ErrorClienteNoExiste);
    }

    public int AltaCliente(Cliente cliente, int idUsuarioCliente)
    {
        ValidarCuit(cliente, null);

        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuarioCliente);
        ValidarUsuarioDisponibleParaEmpresa(usuario);

        int idCliente = clienteRepositorio.Insertar(cliente);

        usuarioService.AsignarEmpresa(idUsuarioCliente, idCliente);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.AltaCliente, cliente.RazonSocial);

        return idCliente;
    }

    public void ModificarCliente(Cliente cliente)
    {
        ValidarCuit(cliente, cliente.IdCliente);

        clienteRepositorio.Modificar(cliente);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.ModificacionCliente, cliente.RazonSocial);
    }

    public void BajaCliente(int idCliente)
    {
        Cliente cliente = ObtenerClientePorId(idCliente);

        clienteRepositorio.BajaLogica(idCliente);

        bitacoraService.Registrar(ObtenerIdUsuarioActual(), TipoAccionBitacora.BajaCliente, cliente.RazonSocial);
    }

    private void ValidarUsuarioDisponibleParaEmpresa(Usuario usuario)
    {
        bool esFamiliaCliente = usuario.NombreFamilia == NombreFamilia.Cliente;

        if (!esFamiliaCliente || usuario.IdCliente.HasValue)
        {
            throw new OperativException(TipoError.ErrorUsuarioNoDisponibleParaEmpresa);
        }
    }

    private void ValidarCuit(Cliente cliente, int? idClienteExcluir)
    {
        if (clienteRepositorio.ExisteCuit(cliente.Cuit, idClienteExcluir))
        {
            throw new OperativException(TipoError.ErrorClienteYaExiste, new string[] { cliente.Cuit });
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
