using System.Data;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;

namespace Operativ.DAL.Implementaciones;
public class ProgramaRepositorio : IProgramaRepositorio
{
    private readonly AccesoDatos accesoDatos;

    public ProgramaRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Programa> ListarTodos()
    {
        string consulta = "SELECT IdPrograma, Nombre, Descripcion FROM Programa ORDER BY Nombre";

        DataTable tabla = accesoDatos.EjecutarReader(consulta, null);

        return tabla.ToListaProgramas();
    }
}