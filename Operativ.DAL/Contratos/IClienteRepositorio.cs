using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IClienteRepositorio
{
    List<Cliente> Listar(string filtro, int numeroPagina, int tamanioPagina);

    int ContarClientes(string filtro);

    List<Cliente> ListarActivos(int? idClienteIncluir);

    Cliente GetPorId(int idCliente);

    int Insertar(Cliente cliente);

    void Modificar(Cliente cliente);

    void BajaLogica(int idCliente);

    bool ExisteCuit(string cuit, int? idClienteExcluir);
}
