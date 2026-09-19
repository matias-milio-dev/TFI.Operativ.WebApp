using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IActivoRepositorio
{
    List<Activo> Listar(string filtro, int numeroPagina, int tamanioPagina);

    int ContarActivos(string filtro);

    Activo GetPorId(int idActivo);

    int Insertar(Activo activo);

    void Modificar(Activo activo);

    void BajaLogica(int idActivo);

    bool ExisteNumeroSerie(string numeroSerie, int? idActivoExcluir);
}
