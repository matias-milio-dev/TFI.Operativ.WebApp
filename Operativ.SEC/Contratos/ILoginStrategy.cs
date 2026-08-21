using Operativ.BE.Modelos;

namespace Operativ.SEC.Contratos;
public interface ILoginStrategy
{
    ResultadoAutenticacion Autenticar(string nombreUsuario, string contrasena);
}
