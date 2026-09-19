using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IProgramaRepositorio
{
    List<Programa> ListarTodos();
}