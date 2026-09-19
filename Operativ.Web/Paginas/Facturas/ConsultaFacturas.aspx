<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsultaFacturas.aspx.cs" Inherits="Operativ.Web.Paginas.ConsultaFacturas" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentConsultaFacturas" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloConsultaFacturas">Facturas</h1>
                    <p runat="server" meta:resourcekey="DescripcionConsultaFacturas">Facturas emitidas por las suscripciones contratadas.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroFacturas %>" /></label>
                    <asp:TextBox ID="txtFiltro" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
        </div>

        <div class="tabla-contenedor">
            <asp:GridView ID="gvFacturas" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
                DataKeyNames="IdFactura" OnRowCommand="gvFacturas_RowCommand" GridLines="None">
                <Columns>
                    <asp:BoundField DataField="NumeroFactura" HeaderText="<%$ Resources:Textos, EtiquetaNumeroFactura %>" />
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaEmision %>">
                        <ItemTemplate>
                            <%# ((DateTime)Eval("FechaEmision")).ToString("dd/MM/yyyy") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="RazonSocialCliente" HeaderText="<%$ Resources:Textos, EtiquetaEmpresaCliente %>" />
                    <asp:BoundField DataField="CuitCliente" HeaderText="<%$ Resources:Textos, EtiquetaCuit %>" />
                    <asp:BoundField DataField="NombrePlan" HeaderText="<%$ Resources:Textos, EtiquetaPlanSuscripcion %>" />
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaTotalFactura %>">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode((string)Eval("Moneda")) %> <%# ((decimal)Eval("Total")).ToString("N2") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <div class="acciones-fila">
                                <asp:LinkButton ID="lnkVer" runat="server" CommandName="Ver" CommandArgument='<%# Eval("IdFactura") %>'
                                    CssClass="btn-outline" CausesValidation="false">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                                    <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonVer %>" /></span>
                                </asp:LinkButton>
                                <asp:HyperLink ID="lnkDescargarPdf" runat="server" CssClass="btn-outline" Target="_blank"
                                    NavigateUrl='<%# Operativ.Web.Paginas.NavegacionHelper.ObtenerUrlExportacionFactura((int)Eval("IdFactura")) %>'>
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                                    <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDescargarPdf %>" /></span>
                                </asp:HyperLink>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinFacturas %>" />
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <uc:Paginador ID="ucPaginador" runat="server" ClaveResumen="MensajeResumenPaginadoFacturas" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
    </div>

    <asp:Panel ID="pnlDetalleFactura" runat="server" CssClass="tarjeta" Visible="false">
        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloDetalleFactura %>" /></h2>

        <asp:Literal ID="litFacturaHtml" runat="server" Mode="PassThrough" />

        <div class="acciones-formulario">
            <asp:HyperLink ID="lnkDescargarPdfDetalle" runat="server" CssClass="btn-primario" Target="_blank">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDescargarPdf %>" />
            </asp:HyperLink>
            <asp:LinkButton ID="btnCerrarDetalle" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonVolver %>" />
            </asp:LinkButton>
        </div>
    </asp:Panel>
</asp:Content>
