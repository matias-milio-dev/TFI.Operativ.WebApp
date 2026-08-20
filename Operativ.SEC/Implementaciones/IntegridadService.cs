using System.Collections.Generic;
using System.Text;
using Operativ.BE.Modelos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;

//Implementacion de la interfaz IIintegridadService
public class IntegridadService : IIntegridadService
{
    //Miembros de clase privados
    private readonly IIntegridadRepositorio integridadRepositorio;

    //Inicializacion con Factory de seguridad de integridadRepositorio
    public IntegridadService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        integridadRepositorio = fabricaRepositorio.CrearIntegridadRepositorio();
    }
    
    //Metodo para iniciar la carga de digitos verificadores en caso de existir.
    public void InicializarDigitos()
    {
        if (!integridadRepositorio.ExisteTablaDigitosVerticiales())
        {
            integridadRepositorio.RecalcularTodo();
        }
    }

    //Verifica la integridad de la base de datos llamando al metodo de DAL en repositorio de integridad
    public List<ResultadoVerificacionTabla> VerificarIntegridad()
    {
        return integridadRepositorio.VerificarTodo();
    }

    //Repara la integridad de la base de datos llamando al metodo de DAL en repositorio de integridad
    public void RepararBaseDatos()
    {
        integridadRepositorio.RecalcularTodo();
    }

    //Convierte con un string builder la lista de filas comprometidas en un mensaje presentable a la UI
    public string FormatearResumenFallas(List<ResultadoVerificacionTabla> resultados)
    {
        StringBuilder resumen = new StringBuilder();

        foreach (ResultadoVerificacionTabla resultado in resultados)
        {
            if (resumen.Length > 0)
            {
                resumen.Append("; ");
            }

            resumen.Append(resultado.NombreTabla);

            if (resultado.ClavesFilasInvalidas.Count > 0)
            {
                resumen.Append(" (registros alterados: ");
                resumen.Append(string.Join(", ", resultado.ClavesFilasInvalidas));
                resumen.Append(")");
            }
            else
            {
                resumen.Append(" (no coincide la cantidad de registros; posible alta o baja fuera del sistema)");
            }
        }

        return resumen.ToString();
    }
}
