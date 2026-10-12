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
            <asp:DropDownList ID="ddlMedioPago" runat="server" data-medio-pago="true" />
        </div>

        <asp:Panel ID="pnlDatosTarjeta" runat="server" CssClass="pago-tarjeta" data-pago-tarjeta="true" data-marca="">
            <h3><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloDatosTarjeta %>" /></h3>
            <p class="texto-ayuda-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, AyudaDatosTarjeta %>" /></p>

            <div class="pago-tarjeta-contenido">
                <div class="tarjeta-visual" aria-hidden="true">
                    <div class="tarjeta-visual-encabezado">
                        <span class="tarjeta-visual-chip"></span>
                        <span class="tarjeta-visual-marca">
                            <svg class="marca-visa" viewBox="0 0 48 30"><text x="24" y="22" text-anchor="middle" font-size="20" font-weight="800" font-style="italic" font-family="Arial, sans-serif" fill="#ffffff">VISA</text></svg>
                            <svg class="marca-mastercard" viewBox="0 0 48 30"><circle cx="18" cy="15" r="11" fill="#EB001B"></circle><circle cx="30" cy="15" r="11" fill="#F79E1B"></circle><path d="M24 6.2a11 11 0 0 1 0 17.6 11 11 0 0 1 0-17.6z" fill="#FF5F00"></path></svg>
                            <svg class="marca-amex" viewBox="0 0 48 30"><rect x="2" y="3" width="44" height="24" rx="3" fill="#2E77BC"></rect><text x="24" y="20" text-anchor="middle" font-size="11" font-weight="800" font-family="Arial, sans-serif" fill="#ffffff">AMEX</text></svg>
                        </span>
                    </div>
                    <div class="tarjeta-visual-numero" data-vista="numero">&bull;&bull;&bull;&bull; &bull;&bull;&bull;&bull; &bull;&bull;&bull;&bull; &bull;&bull;&bull;&bull;</div>
                    <div class="tarjeta-visual-pie">
                        <div>
                            <span class="tarjeta-visual-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaTitularTarjeta %>" /></span>
                            <span class="tarjeta-visual-valor" data-vista="titular"><asp:Literal runat="server" Text="<%$ Resources:Textos, VistaTitularTarjeta %>" /></span>
                        </div>
                        <div>
                            <span class="tarjeta-visual-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaVencimientoTarjeta %>" /></span>
                            <span class="tarjeta-visual-valor" data-vista="vencimiento"><asp:Literal runat="server" Text="<%$ Resources:Textos, PlaceholderVencimientoTarjeta %>" /></span>
                        </div>
                    </div>
                </div>

                <div class="pago-tarjeta-campos">
                    <div class="campo-formulario">
                        <label for="<%= txtNumeroTarjeta.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaNumeroTarjeta %>" /></label>
                        <div class="campo-con-marca">
                            <asp:TextBox ID="txtNumeroTarjeta" runat="server" MaxLength="23" autocomplete="cc-number" inputmode="numeric"
                                placeholder="<%$ Resources:Textos, PlaceholderNumeroTarjeta %>" data-campo="numero" />
                            <span class="campo-marca" aria-hidden="true">
                                <svg class="marca-visa" viewBox="0 0 48 30"><rect width="48" height="30" rx="4" fill="#1A1F71"></rect><text x="24" y="21" text-anchor="middle" font-size="15" font-weight="800" font-style="italic" font-family="Arial, sans-serif" fill="#ffffff">VISA</text></svg>
                                <svg class="marca-mastercard" viewBox="0 0 48 30"><rect width="48" height="30" rx="4" fill="#ffffff" stroke="#d1d5db"></rect><circle cx="19" cy="15" r="9" fill="#EB001B"></circle><circle cx="29" cy="15" r="9" fill="#F79E1B"></circle><path d="M24 7.7a9 9 0 0 1 0 14.6 9 9 0 0 1 0-14.6z" fill="#FF5F00"></path></svg>
                                <svg class="marca-amex" viewBox="0 0 48 30"><rect width="48" height="30" rx="4" fill="#2E77BC"></rect><text x="24" y="19" text-anchor="middle" font-size="9" font-weight="800" font-family="Arial, sans-serif" fill="#ffffff">AMEX</text></svg>
                            </span>
                        </div>
                        <asp:CustomValidator ID="cvNumeroTarjeta" runat="server" ControlToValidate="txtNumeroTarjeta" ValidateEmptyText="true"
                            ClientValidationFunction="Operativ.validarNumeroTarjeta" OnServerValidate="cvNumeroTarjeta_ServerValidate"
                            ErrorMessage="<%$ Resources:Textos, MensajeValidacionNumeroTarjeta %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="PagoSuscripcion" />
                    </div>

                    <div class="campo-formulario">
                        <label for="<%= txtTitularTarjeta.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaTitularTarjeta %>" /></label>
                        <asp:TextBox ID="txtTitularTarjeta" runat="server" MaxLength="60" autocomplete="cc-name"
                            placeholder="<%$ Resources:Textos, PlaceholderTitularTarjeta %>" data-campo="titular" />
                        <asp:CustomValidator ID="cvTitularTarjeta" runat="server" ControlToValidate="txtTitularTarjeta" ValidateEmptyText="true"
                            ClientValidationFunction="Operativ.validarTitularTarjeta" OnServerValidate="cvTitularTarjeta_ServerValidate"
                            ErrorMessage="<%$ Resources:Textos, MensajeValidacionTitularTarjeta %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="PagoSuscripcion" />
                    </div>

                    <div class="fila-formulario">
                        <div class="campo-formulario">
                            <label for="<%= txtVencimientoTarjeta.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaVencimientoTarjeta %>" /></label>
                            <asp:TextBox ID="txtVencimientoTarjeta" runat="server" MaxLength="5" autocomplete="cc-exp" inputmode="numeric"
                                placeholder="<%$ Resources:Textos, PlaceholderVencimientoTarjeta %>" data-campo="vencimiento" />
                            <asp:CustomValidator ID="cvVencimientoTarjeta" runat="server" ControlToValidate="txtVencimientoTarjeta" ValidateEmptyText="true"
                                ClientValidationFunction="Operativ.validarVencimientoTarjeta" OnServerValidate="cvVencimientoTarjeta_ServerValidate"
                                ErrorMessage="<%$ Resources:Textos, MensajeValidacionVencimientoTarjeta %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="PagoSuscripcion" />
                        </div>

                        <div class="campo-formulario">
                            <label for="<%= txtCodigoSeguridad.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCodigoSeguridad %>" /></label>
                            <asp:TextBox ID="txtCodigoSeguridad" runat="server" TextMode="Password" MaxLength="4" autocomplete="cc-csc" inputmode="numeric"
                                placeholder="<%$ Resources:Textos, PlaceholderCodigoSeguridad %>" data-campo="codigo" />
                            <asp:CustomValidator ID="cvCodigoSeguridad" runat="server" ControlToValidate="txtCodigoSeguridad" ValidateEmptyText="true"
                                ClientValidationFunction="Operativ.validarCodigoSeguridad" OnServerValidate="cvCodigoSeguridad_ServerValidate"
                                ErrorMessage="<%$ Resources:Textos, MensajeValidacionCodigoSeguridad %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="PagoSuscripcion" />
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnConfirmarPago" runat="server" CssClass="btn-primario" CausesValidation="true" ValidationGroup="PagoSuscripcion"
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
