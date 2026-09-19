namespace Operativ.BE.Entidades;
public class Cliente
{
    public int IdCliente { get; set; }

    public string RazonSocial { get; set; }

    public string Cuit { get; set; }

    public string Email { get; set; }

    public bool Activo { get; set; }
}
