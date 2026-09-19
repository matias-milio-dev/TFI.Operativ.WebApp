<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GestionClientes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionClientes" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentGestionClientes" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 21h18"></path><path d="M5 21V7l8-4v18"></path><path d="M19 21V11l-6-4"></path><line x1="9" y1="9" x2="9" y2="9.01"></line><line x1="9" y1="13" x2="9" y2="13.01"></line><line x1="9" y1="17" x2="9" y2="17.01"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloGestionClientes">Empresas cliente</h1>
                    <p runat="server" meta:resourcekey="DescripcionGestionClientes">Alta, baja y modificación de las empresas que contratan el servicio.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroClientes %>" /></label>
                    <asp:TextBox ID="txtFiltro" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
            <asp:LinkButton ID="btnNuevaEmpresa" runat="server" CssClass="btn-primario" CausesValidation="false" data-patente="GestionarClientes" OnClick="btnNuevaEmpresa_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonNuevaEmpresa %>" />
            </asp:LinkButton>
        </div>

        <asp:Panel ID="pnlListado" runat="server">
            <div class="tabla-contenedor">
                <asp:GridView ID="gvClientes" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
                    DataKeyNames="IdCliente" OnRowCommand="gvClientes_RowCommand" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="RazonSocial" HeaderText="<%$ Resources:Textos, EtiquetaRazonSocial %>" />
                        <asp:BoundField DataField="Cuit" HeaderText="<%$ Resources:Textos, EtiquetaCuit %>" />
                        <asp:BoundField DataField="Email" HeaderText="<%$ Resources:Textos, EtiquetaEmailEmpresa %>" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <div class="acciones-fila">
                                    <asp:LinkButton ID="lnkEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IdCliente") %>'
                                        CssClass="btn-outline" CausesValidation="false" data-patente="GestionarClientes">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"></path></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonEditar %>" /></span>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkBaja" runat="server" CommandName="Baja" CommandArgument='<%# Eval("IdCliente") %>'
                                        CssClass="btn-outline-peligro" CausesValidation="false" data-patente="GestionarClientes"
                                        OnClientClick="return confirm('¿Confirma que desea dar de baja la empresa?');">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDarBaja %>" /></span>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinClientes %>" />
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <uc:Paginador ID="ucPaginador" runat="server" ClaveResumen="MensajeResumenPaginadoClientes" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
        </asp:Panel>
    </div>

    <asp:Panel ID="pnlFormularioCliente" runat="server" CssClass="tarjeta" Visible="false">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 21h18"></path><path d="M5 21V7l8-4v18"></path><path d="M19 21V11l-6-4"></path></svg>
            </span>
            <h2 id="tituloFormulario" runat="server"></h2>
        </div>
        <asp:HiddenField ID="hidIdCliente" runat="server" Value="0" />

        <div class="fila-formulario">
            <div class="campo-formulario">
                <label for="<%= txtRazonSocial.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaRazonSocial %>" /></label>
                <asp:TextBox ID="txtRazonSocial" runat="server" />
                <asp:RequiredFieldValidator ID="rfvRazonSocial" runat="server" ControlToValidate="txtRazonSocial"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionRazonSocialObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cliente" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtCuit.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCuit %>" /></label>
                <asp:TextBox ID="txtCuit" runat="server" placeholder="30-12345678-9" />
                <asp:RequiredFieldValidator ID="rfvCuit" runat="server" ControlToValidate="txtCuit"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionCuitObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cliente" />
                <asp:RegularExpressionValidator ID="revCuit" runat="server" ControlToValidate="txtCuit"
                    ValidationExpression="^\d{2}-\d{8}-\d$"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionCuitFormato %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cliente" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtEmailEmpresa.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEmailEmpresa %>" /></label>
                <asp:TextBox ID="txtEmailEmpresa" runat="server" />
                <asp:RequiredFieldValidator ID="rfvEmailEmpresa" runat="server" ControlToValidate="txtEmailEmpresa"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cliente" />
                <asp:RegularExpressionValidator ID="revEmailEmpresa" runat="server" ControlToValidate="txtEmailEmpresa"
                    ValidationExpression="^[\w\.\-]+@[\w\-]+\.[\w\.\-]+$"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoFormato %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cliente" />
            </div>
        </div>

        <asp:Panel ID="pnlUsuarioInicial" runat="server">
            <h3 class="subtitulo-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloUsuarioInicial %>" /></h3>
            <p class="texto-ayuda-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, AyudaUsuarioInicial %>" /></p>

            <div class="campo-formulario">
                <label for="<%= ddlUsuarioCliente.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaUsuarioCliente %>" /></label>
                <asp:DropDownList ID="ddlUsuarioCliente" runat="server" />
                <asp:RequiredFieldValidator ID="rfvUsuarioCliente" runat="server" ControlToValidate="ddlUsuarioCliente" InitialValue=""
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionUsuarioClienteObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cliente" />
            </div>
        </asp:Panel>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" data-patente="GestionarClientes" ValidationGroup="Cliente" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnCancelar" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCancelar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </asp:LinkButton>
        </div>
        <asp:ValidationSummary ID="vsCliente" runat="server" CssClass="texto-validacion" ValidationGroup="Cliente" />
    </asp:Panel>
</asp:Content>
