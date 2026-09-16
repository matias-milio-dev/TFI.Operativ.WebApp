<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Contacto.aspx.cs" Inherits="Operativ.Web.Paginas.Contacto" MasterPageFile="~/Master/Publico.Master" %>
<asp:Content ID="ContentContacto" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <section class="hero-publico">
        <div class="hero-publico-texto">
            <span class="eyebrow"><span class="eyebrow-linea"></span><asp:Literal runat="server" Text="<%$ Resources:Textos, EyebrowContacto %>" /></span>
            <h1><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloContacto %>" /></h1>
            <p class="hero-publico-bajada"><asp:Literal runat="server" Text="<%$ Resources:Textos, BajadaContacto %>" /></p>
        </div>
        <div class="hero-publico-imagen">
            <asp:Image ID="imgHero" runat="server" ImageUrl="~/Imagenes/hero-contacto.svg" AlternateText="<%$ Resources:Textos, AltHeroContacto %>" />
        </div>
    </section>

    <section class="grilla-contacto">
        <div class="tarjeta-publica">
            <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloEnviaConsulta %>" /></h2>

            <div class="campo-formulario">
                <label for="<%= txtNombreCompleto.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaNombreCompleto %>" /></label>
                <asp:TextBox ID="txtNombreCompleto" runat="server" placeholder="<%$ Resources:Textos, PlaceholderNombreContacto %>" />
                <asp:RequiredFieldValidator ID="rfvNombreCompleto" runat="server" ControlToValidate="txtNombreCompleto"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionNombreCompletoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Contacto" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtEmail.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCorreoElectronico %>" /></label>
                <asp:TextBox ID="txtEmail" runat="server" placeholder="<%$ Resources:Textos, PlaceholderEmailContacto %>" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Contacto" />
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                    ValidationExpression="^[\w\.\-]+@[\w\-]+\.[\w\.\-]+$"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoFormato %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Contacto" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtEmpresa.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEmpresa %>" /></label>
                <asp:TextBox ID="txtEmpresa" runat="server" placeholder="<%$ Resources:Textos, PlaceholderEmpresaContacto %>" />
            </div>

            <div class="campo-formulario">
                <label for="<%= ddlAsunto.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaAsunto %>" /></label>
                <asp:DropDownList ID="ddlAsunto" runat="server" />
                <asp:RequiredFieldValidator ID="rfvAsunto" runat="server" ControlToValidate="ddlAsunto" InitialValue=""
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionAsuntoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Contacto" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtMensaje.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaMensaje %>" /></label>
                <asp:TextBox ID="txtMensaje" runat="server" TextMode="MultiLine" Rows="4" CssClass="campo-mensaje" placeholder="<%$ Resources:Textos, PlaceholderMensajeContacto %>" />
                <asp:RequiredFieldValidator ID="rfvMensaje" runat="server" ControlToValidate="txtMensaje"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionMensajeObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Contacto" />
            </div>

            <asp:LinkButton ID="btnEnviar" runat="server" CssClass="btn-primario" ValidationGroup="Contacto" OnClick="btnEnviar_Click">
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonEnviarConsulta %>" />
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line><polyline points="12 5 19 12 12 19"></polyline></svg>
            </asp:LinkButton>
        </div>

        <div class="tarjeta-publica">
            <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloNuestrosCanales %>" /></h2>
            <div class="lista-canales">
                <div class="canal">
                    <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22 6 12 13 2 6"></polyline></svg></span>
                    <div class="canal-texto">
                        <span class="canal-titulo"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCanalEmail %>" /></span>
                        <span class="canal-valor"><asp:Literal runat="server" Text="<%$ Resources:Textos, ValorCanalEmail %>" /></span>
                    </div>
                </div>
                <div class="canal">
                    <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path></svg></span>
                    <div class="canal-texto">
                        <span class="canal-titulo"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCanalTelefono %>" /></span>
                        <span class="canal-valor"><asp:Literal runat="server" Text="<%$ Resources:Textos, ValorCanalTelefono %>" /></span>
                    </div>
                </div>
                <div class="canal">
                    <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg></span>
                    <div class="canal-texto">
                        <span class="canal-titulo"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCanalOficinas %>" /></span>
                        <span class="canal-valor"><asp:Literal runat="server" Text="<%$ Resources:Textos, ValorCanalOficinas %>" /></span>
                    </div>
                </div>
                <div class="canal">
                    <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg></span>
                    <div class="canal-texto">
                        <span class="canal-titulo"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCanalHorarios %>" /></span>
                        <span class="canal-valor"><asp:Literal runat="server" Text="<%$ Resources:Textos, ValorCanalHorarios %>" /></span>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
