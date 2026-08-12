using System.Data;
using System.Text.RegularExpressions;
using Operativ.BE;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;
using Operativ.SEC;

namespace Operativ.BLL
{
    public interface IClienteBLL
    {
        DataTable Listar(string filtro, int numeroPagina, int tamanioPagina);
        Cliente Obtener(int idCliente);
        int Alta(Cliente cliente);
        void Modificar(Cliente cliente);
        void Baja(int idCliente);
    }

    public class ClienteBLL : IClienteBLL
    {
        private static readonly Regex PatronCuit = new Regex(@"^\d{2}-\d{8}-\d{1}$");

        private readonly IClienteDAL _clienteDAL = FabricaDAL.Instancia.CrearClienteDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public DataTable Listar(string filtro, int numeroPagina, int tamanioPagina)
        {
            return _clienteDAL.Listar(filtro, numeroPagina, tamanioPagina);
        }

        public Cliente Obtener(int idCliente)
        {
            var cliente = _clienteDAL.ObtenerPorId(idCliente);
            if (cliente == null) throw new ExcepcionNegocio(CodigosError.ErrorRegistroNoEncontrado);
            return cliente;
        }

        public int Alta(Cliente cliente)
        {
            ValidarCliente(cliente);

            if (_clienteDAL.ObtenerPorCuit(cliente.Cuit) != null)
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCuitClienteDuplicado);
            }

            int idClienteNuevo = _clienteDAL.Insertar(cliente);
            _bitacoraBLL.Registrar("ALTA", "Cliente", idClienteNuevo.ToString(), $"Alta de cliente '{cliente.RazonSocial}'.", "ADVERTENCIA");

            string asunto = TextoHelper.Resolver("EmailAsuntoRegistroCliente", null);
            string cuerpo = string.Format(TextoHelper.Resolver("EmailCuerpoRegistroCliente", null), cliente.RazonSocial);
            EmailHelper.Enviar(cliente.CorreoElectronico, asunto, cuerpo);
            _bitacoraBLL.Registrar("ENVIO_EMAIL", "Cliente", idClienteNuevo.ToString(), $"Email de confirmación de registro enviado a '{cliente.CorreoElectronico}'.", "INFORMATIVA");

            return idClienteNuevo;
        }

        public void Modificar(Cliente cliente)
        {
            ValidarCliente(cliente);

            _clienteDAL.Modificar(cliente);
            _bitacoraBLL.Registrar("MODIFICACION", "Cliente", cliente.IdCliente.ToString(), "Modificación de datos de cliente.", "ADVERTENCIA");
        }

        public void Baja(int idCliente)
        {
            _clienteDAL.Baja(idCliente);
            _bitacoraBLL.Registrar("BAJA", "Cliente", idCliente.ToString(), "Baja lógica de cliente.", "GRAVE");
        }

        private static void ValidarCliente(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.RazonSocial)
                || string.IsNullOrWhiteSpace(cliente.CorreoElectronico))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);
            }
            if (string.IsNullOrWhiteSpace(cliente.Cuit)
                || !PatronCuit.IsMatch(cliente.Cuit))
            {
                throw new ExcepcionNegocio(CodigosError.ErrorFormatoDatoInvalido);
            }
        }
    }
}
