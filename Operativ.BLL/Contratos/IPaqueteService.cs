using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.BLL.Contratos;
public interface IPaqueteService
{
    List<Paquete> ListarPaquetes(string filtro, int numeroPagina, int tamanioPagina);

    int ContarPaquetes(string filtro);

    Paquete ObtenerPaquetePorId(int idPaquete);

    List<Programa> ListarProgramas();

    int AltaPaquete(Paquete paquete, int[] idsPrograma);

    void ModificarPaquete(Paquete paquete, int[] idsPrograma);

    void BajaPaquete(int idPaquete);
}