using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;

namespace Operativ.DAL.Contratos;
public interface IActivoRepositorio
{
    List<Activo> Listar(string filtro, int? idCliente, EstadoActivo? estado, int? idPaquete, int numeroPagina, int tamanioPagina);

    int ContarActivos(string filtro, int? idCliente, EstadoActivo? estado, int? idPaquete);

    ResumenActivos ObtenerResumen(int? idCliente);

    List<Activo> ListarPorCliente(int idCliente);

    Activo GetPorId(int idActivo);

    int Insertar(Activo activo);

    void Modificar(Activo activo);

    void BajaLogica(int idActivo);

    bool ExisteNumeroSerie(string numeroSerie, int? idActivoExcluir);
}
