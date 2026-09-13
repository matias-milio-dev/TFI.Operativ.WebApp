using System;
using System.Web.UI;
using Operativ.Web.Idioma;

namespace Operativ.Web.Controles;
public partial class Paginador : UserControl
{
    private const int TamanioPaginaPorDefecto = 10;

    public event EventHandler PaginaCambiada;

    public string ClaveResumen { get; set; }

    public int TamanioPagina
    {
        get { return ViewState["TamanioPagina"] == null ? TamanioPaginaPorDefecto : (int)ViewState["TamanioPagina"]; }
        set { ViewState["TamanioPagina"] = value; }
    }

    public int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    public void Reiniciar()
    {
        NumeroPagina = 1;
    }

    public void Actualizar(int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((NumeroPagina - 1) * TamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        litResumen.Text = TextoRecurso.Formato(ClaveResumen, desde, hasta, total);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnAnterior.Enabled = NumeroPagina > 1;
        btnSiguiente.Enabled = (NumeroPagina * TamanioPagina) < total;
    }

    protected void btnAnterior_Click(object sender, EventArgs e)
    {
        if (NumeroPagina > 1)
        {
            NumeroPagina--;
        }

        PaginaCambiada?.Invoke(this, EventArgs.Empty);
    }

    protected void btnSiguiente_Click(object sender, EventArgs e)
    {
        NumeroPagina++;
        PaginaCambiada?.Invoke(this, EventArgs.Empty);
    }
}
