using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IPaqueteRepositorio
{
    List<Paquete> Listar(string filtro, int numeroPagina, int tamanioPagina);

    List<Paquete> ListarHabilitados(int? idPaqueteIncluir);

    int ContarPaquetes(string filtro);

    Paquete GetPorId(int idPaquete);

    List<Programa> GetProgramasDePaquete(int idPaquete);

    int Insertar(Paquete paquete);

    void Modificar(Paquete paquete);

    void BajaLogica(int idPaquete);

    void AsignarProgramas(int idPaquete, int[] idsPrograma);

    void QuitarProgramas(int idPaquete, int[] idsPrograma);

    bool ExisteNombre(string nombre, int? idPaqueteExcluir);
}