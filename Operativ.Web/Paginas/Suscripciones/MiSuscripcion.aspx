<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MiSuscripcion.aspx.cs" Inherits="Operativ.Web.Paginas.MiSuscripcion" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentMiSuscripcion" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>
            </span>
            <div>
                <h1 runat="server" meta:resourcekey="TituloMiSuscripcion">Mi suscripción</h1>
                <p runat="server" meta:resourcekey="DescripcionMiSuscripcion">Estado de la suscripción de tu empresa y gestión del pago.</p>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlSinSuscripcion" runat="server" CssClass="tarjeta" Visible="false">
        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloContratarSuscripcion %>" /></h2>
        <p class="texto-ayuda-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, AyudaContratarSuscripcion %>" /></p>

        <asp:Repeater ID="rptPlanes" runat="server">
            <HeaderTemplate>
                <div class="grilla-planes">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="tarjeta-plan">
                    <h3><%# Eval("Nombre") %></h3>
                    <p class="tarjeta-plan-precio">USD <%# ((decimal)Eval("PrecioAnual")).ToString("N2") %><span class="tarjeta-plan-periodo"> / año</span></p>
                    <p class="tarjeta-plan-descripcion"><%# Eval("Descripcion") %></p>
                    <asp:LinkButton ID="lnkContratar" runat="server" CssClass="btn-primario" CausesValidation="false"
                        CommandName="Contratar" CommandArgument='<%# Eval("IdPlan") %>' data-patente="GestionarSuscripciones"
                        OnCommand="lnkContratar_Command">
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonContratarPlan %>" />
                    </asp:LinkButton>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>
    </asp:Panel>

    <asp:Panel ID="pnlEstadoSuscripcion" runat="server" CssClass="tarjeta" Visible="false">
        <div class="suscripcion-encabezado">
            <div>
                <h2><asp:Literal ID="litNombrePlan" runat="server" /></h2>
                <p class="suscripcion-precio">USD <asp:Literal ID="litPrecioAnual" runat="server" /><span class="tarjeta-plan-periodo"> / año</span></p>
            </div>
            <asp:Label ID="lblEstado" runat="server" CssClass="badge" />
        </div>

        <div class="detalle-incidente">
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFechaAltaSuscripcion %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litFechaAlta" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFinTrial %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litFinTrial" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaVencimientoSuscripcion %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litVencimiento" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaComprobantePago %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litComprobante" runat="server" /></span>
            </div>
        </div>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnVerResumen" runat="server" CssClass="btn-primario" CausesValidation="false" Visible="false"
                data-patente="GestionarSuscripciones" OnClick="btnVerResumen_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonIrAPagar %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnCancelarSuscripcion" runat="server" CssClass="btn-outline-peligro" CausesValidation="false"
                data-patente="GestionarSuscripciones" OnClick="btnCancelarSuscripcion_Click"
                OnClientClick="return confirm('¿Confirma que desea dar de baja la suscripción?');">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDarBajaSuscripcion %>" />
            </asp:LinkButton>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlResumen" runat="server" CssClass="tarjeta" Visible="false">
        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloResumenSuscripcion %>" /></h2>
        <p class="texto-ayuda-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, AyudaResumenGenerado %>" /></p>

        <asp:Literal ID="litResumenHtml" runat="server" Mode="PassThrough" />

        <p class="texto-ayuda-formulario"><asp:Literal ID="litArchivoXml" runat="server" /></p>

        <div class="campo-formulario">
            <label for="<%= ddlMedioPago.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaMedioPago %>" /></label>
            <asp:DropDownList ID="ddlMedioPago" runat="server" />
        </div>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnConfirmarPago" runat="server" CssClass="btn-primario" CausesValidation="false"
                data-patente="GestionarSuscripciones" OnClick="btnConfirmarPago_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonConfirmarPago %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnVolverEstado" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnVolverEstado_Click">
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonVolver %>" />
            </asp:LinkButton>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlPagoConfirmado" runat="server" CssClass="tarjeta" Visible="false">
        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloPagoConfirmado %>" /></h2>

        <asp:Literal ID="litComprobanteHtml" runat="server" Mode="PassThrough" />
        <asp:Literal ID="litFacturaHtml" runat="server" Mode="PassThrough" />

        <div class="acciones-formulario">
            <asp:HyperLink ID="lnkDescargarFactura" runat="server" CssClass="btn-primario" Target="_blank">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDescargarPdf %>" />
            </asp:HyperLink>
            <asp:LinkButton ID="btnVolverDesdeConfirmacion" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnVolverEstado_Click">
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonVolver %>" />
            </asp:LinkButton>
        </div>
    </asp:Panel>
</asp:Content>
