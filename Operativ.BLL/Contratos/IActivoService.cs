using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;

namespace Operativ.BLL.Contratos;
public interface IActivoService
{
    List<Activo> ListarActivos(string filtro, int? idCliente, EstadoActivo? estado, int? idPaquete, int numeroPagina, int tamanioPagina);

    int ContarActivos(string filtro, int? idCliente, EstadoActivo? estado, int? idPaquete);

    ResumenActivos ObtenerResumenActivos(int? idCliente);

    List<Activo> ListarActivosDeCliente(int idCliente);

    Activo ObtenerActivoPorId(int idActivo);

    int AltaActivo(Activo activo);

    void ModificarActivo(Activo activo);

    void BajaActivo(int idActivo);
}
