using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos;
public interface IActivoService
{
    List<Activo> ListarActivos(string filtro, int? idCliente, int numeroPagina, int tamanioPagina);

    int ContarActivos(string filtro, int? idCliente);

    List<Activo> ListarActivosDeCliente(int idCliente);

    Activo ObtenerActivoPorId(int idActivo);

    int AltaActivo(Activo activo);

    void ModificarActivo(Activo activo);

    void BajaActivo(int idActivo);
}
