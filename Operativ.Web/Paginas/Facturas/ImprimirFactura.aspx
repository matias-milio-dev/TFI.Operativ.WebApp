<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImprimirFactura.aspx.cs" Inherits="Operativ.Web.Paginas.ImprimirFactura" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentImprimirFactura" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= Operativ.Web.Paginas.RecursoEstatico.ObtenerUrl("~/Estilos/impresion-factura.css") %>" />

    <asp:Panel ID="pnlBarraImpresion" runat="server" CssClass="tarjeta barra-impresion" Visible="false">
        <p class="texto-ayuda-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, AyudaDescargarFactura %>" /></p>
        <button type="button" class="btn-primario" onclick="Operativ.imprimirDocumento()">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
            <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDescargarPdf %>" />
        </button>
    </asp:Panel>

    <asp:Literal ID="litFacturaHtml" runat="server" Mode="PassThrough" />
</asp:Content>
