using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;
public class BitacoraService : IBitacoraService
{
    private const int LongitudMaximaDescripcion = 300;

    private readonly IBitacoraRepositorio bitacoraRepositorio;

    public BitacoraService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        bitacoraRepositorio = fabricaRepositorio.CrearBitacoraRepositorio();
    }

    public void Registrar(int? idUsuario, TipoAccionBitacora accion)
    {
        Registrar(idUsuario, accion, null);
    }

    public void Registrar(int? idUsuario, TipoAccionBitacora accion, string detalleAdicional)
    {
        AccionBitacora definicion = AccionBitacora.ObtenerPorTipo(accion);
        string descripcion = string.Format(definicion.Descripcion, ConfiguracionAplicacion.IntentosMaximosLogin);

        if (!string.IsNullOrEmpty(detalleAdicional))
        {
            descripcion = descripcion + ": " + detalleAdicional;

            if (descripcion.Length > LongitudMaximaDescripcion)
            {
                descripcion = descripcion.Substring(0, LongitudMaximaDescripcion);
            }
        }

        Bitacora entrada = new Bitacora
        {
            IdUsuario = idUsuario,
            Accion = accion,
            Criticidad = definicion.Criticidad,
            Descripcion = descripcion
        };

        bitacoraRepositorio.Registrar(entrada);
    }
}
