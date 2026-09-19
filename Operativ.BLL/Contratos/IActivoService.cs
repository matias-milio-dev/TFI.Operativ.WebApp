using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos;
public interface IActivoService
{
    List<Activo> ListarActivos(string filtro, int numeroPagina, int tamanioPagina);

    int ContarActivos(string filtro);

    Activo ObtenerActivoPorId(int idActivo);

    int AltaActivo(Activo activo);

    void ModificarActivo(Activo activo);

    void BajaActivo(int idActivo);
}
