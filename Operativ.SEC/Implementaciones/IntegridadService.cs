using System.Collections.Generic;
using System.Text;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones
{
    public class IntegridadService : IIntegridadService
    {
        private readonly IIntegridadRepositorio integridadRepositorio;

        public IntegridadService()
        {
            FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
            integridadRepositorio = fabricaRepositorio.CrearIntegridadRepositorio();
        }

        public void InicializarDigitos()
        {
            if (!integridadRepositorio.ExisteLineaBase())
            {
                integridadRepositorio.RecalcularTodo();
            }
        }

        public List<ResultadoVerificacionTabla> VerificarIntegridad()
        {
            return integridadRepositorio.VerificarTodo();
        }

        public void RepararBaseDatos()
        {
            integridadRepositorio.RecalcularTodo();
        }

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
}
