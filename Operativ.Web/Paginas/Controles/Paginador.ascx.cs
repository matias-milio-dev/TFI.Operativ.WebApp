using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.Web.Idioma;

namespace Operativ.Web.Controles;
public partial class Paginador : UserControl
{
    private const int TamanioPaginaPorDefecto = 10;
    private const string ComandoIrAPagina = "IrAPagina";
    private const string ClasePagina = "paginado-pagina";
    private const string ClasePaginaActiva = "paginado-pagina paginado-pagina-activa";

    public event EventHandler PaginaCambiada;

    public string ClaveResumen { get; set; }

    // No va en ViewState: se declara en el markup de la pagina y se re-aplica en cada request.
    public bool ModoNumerado { get; set; }

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

        pnlControlesTexto.Visible = !ModoNumerado;
        pnlControlesNumerados.Visible = ModoNumerado;

        if (ModoNumerado)
        {
            ActualizarNumerado(total);
        }
        else
        {
            litNumeroPagina.Text = NumeroPagina.ToString();

            btnAnterior.Enabled = NumeroPagina > 1;
            btnSiguiente.Enabled = (NumeroPagina * TamanioPagina) < total;
        }
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

    protected void btnPagina_Command(object sender, CommandEventArgs e)
    {
        if (e.CommandName != ComandoIrAPagina)
        {
            return;
        }

        int paginaDestino = Convert.ToInt32(e.CommandArgument);

        if (paginaDestino == NumeroPagina)
        {
            return;
        }

        NumeroPagina = paginaDestino;
        PaginaCambiada?.Invoke(this, EventArgs.Empty);
    }

    // Los botones de pagina son fijos (no un Repeater): la pagina vuelve a enlazar el paginador en cada
    // Page_Load, y un Repeater recreado en ese momento deja huerfano al boton que origino el postback.
    private Button[] ObtenerBotonesPagina()
    {
        return new[] { btnPagina1, btnPagina2, btnPagina3, btnPagina4, btnPagina5, btnPagina6, btnPagina7 };
    }

    private void ActualizarNumerado(int total)
    {
        Button[] botones = ObtenerBotonesPagina();
        int cantidadPaginas = CalcularCantidadPaginas(total);

        btnAnteriorNumerado.Enabled = NumeroPagina > 1;
        btnSiguienteNumerado.Enabled = NumeroPagina < cantidadPaginas;

        List<int> paginas = ArmarVentanaPaginas(cantidadPaginas, botones.Length);

        for (int indice = 0; indice < botones.Length; indice++)
        {
            Button boton = botones[indice];
            boton.Visible = indice < paginas.Count;

            if (boton.Visible)
            {
                int numero = paginas[indice];
                boton.Text = numero.ToString();
                boton.CommandArgument = numero.ToString();
                boton.CssClass = numero == NumeroPagina ? ClasePaginaActiva : ClasePagina;
            }
        }
    }

    private int CalcularCantidadPaginas(int total)
    {
        if (total == 0 || TamanioPagina <= 0)
        {
            return 1;
        }

        return (total + TamanioPagina - 1) / TamanioPagina;
    }

    private List<int> ArmarVentanaPaginas(int cantidadPaginas, int maximoVisibles)
    {
        int primera = 1;
        int ultima = cantidadPaginas;

        if (cantidadPaginas > maximoVisibles)
        {
            primera = NumeroPagina - (maximoVisibles / 2);

            if (primera < 1)
            {
                primera = 1;
            }

            ultima = primera + maximoVisibles - 1;

            if (ultima > cantidadPaginas)
            {
                ultima = cantidadPaginas;
                primera = ultima - maximoVisibles + 1;
            }
        }

        List<int> paginas = new List<int>();

        for (int numero = primera; numero <= ultima; numero++)
        {
            paginas.Add(numero);
        }

        return paginas;
    }
}
