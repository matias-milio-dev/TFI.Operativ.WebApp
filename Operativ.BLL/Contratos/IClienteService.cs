using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos;
public interface IClienteService
{
    List<Cliente> ListarClientes(string filtro, int numeroPagina, int tamanioPagina);

    int ContarClientes(string filtro);

    List<Cliente> ListarClientesActivos(int? idClienteIncluir);

    List<Usuario> ListarUsuariosClienteDisponibles();

    Cliente ObtenerClientePorId(int idCliente);

    int AltaCliente(Cliente cliente, int idUsuarioCliente);

    void ModificarCliente(Cliente cliente);

    void BajaCliente(int idCliente);
}
