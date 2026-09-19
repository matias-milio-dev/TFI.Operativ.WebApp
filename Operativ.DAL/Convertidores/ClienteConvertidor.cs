using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores;
public static class ClienteConvertidor
{
    public static Cliente ToCliente(this DataRow fila)
    {
        Cliente cliente = new Cliente
        {
            IdCliente = (int)fila["IdCliente"],
            RazonSocial = fila["RazonSocial"].ToString(),
            Cuit = fila["Cuit"].ToString(),
            Email = fila["Email"].ToString(),
            Activo = (bool)fila["Activo"]
        };

        return cliente;
    }

    public static List<Cliente> ToListaClientes(this DataTable tabla)
    {
        List<Cliente> clientes = new List<Cliente>();

        foreach (DataRow fila in tabla.Rows)
        {
            clientes.Add(fila.ToCliente());
        }

        return clientes;
    }
}
