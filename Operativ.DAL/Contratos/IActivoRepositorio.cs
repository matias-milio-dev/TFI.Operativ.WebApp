using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IActivoRepositorio
{
    List<Activo> Listar(string filtro, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarActivos(string filtro, int? idCliente);

    List<Activo> ListarPorCliente(int idCliente);

    Activo GetPorId(int idActivo);

    int Insertar(Activo activo);

    void Modificar(Activo activo);

    void BajaLogica(int idActivo);

    bool ExisteNumeroSerie(string numeroSerie, int? idActivoExcluir);
}
