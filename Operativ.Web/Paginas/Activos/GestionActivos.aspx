<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GestionActivos.aspx.cs" Inherits="Operativ.Web.Paginas.GestionActivos" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Enums" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentGestionActivos" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="2" y1="21" x2="22" y2="21"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloGestionActivos">Activos</h1>
                    <p runat="server" meta:resourcekey="DescripcionGestionActivos">Inventario de estaciones de trabajo y su paquete de configuración asignado.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroActivos %>" /></label>
                    <asp:TextBox ID="txtFiltro" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
            <asp:LinkButton ID="btnNuevoActivo" runat="server" CssClass="btn-primario" CausesValidation="false" data-patente="GestionarActivos" OnClick="btnNuevoActivo_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonNuevoActivo %>" />
            </asp:LinkButton>
        </div>

        <asp:Panel ID="pnlListado" runat="server">
            <div class="tabla-contenedor">
                <asp:GridView ID="gvActivos" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
                    DataKeyNames="IdActivo" OnRowCommand="gvActivos_RowCommand" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="Nombre" HeaderText="<%$ Resources:Textos, EtiquetaNombreActivo %>" />
                        <asp:BoundField DataField="Modelo" HeaderText="<%$ Resources:Textos, EtiquetaModeloActivo %>" />
                        <asp:BoundField DataField="NumeroSerie" HeaderText="<%$ Resources:Textos, EtiquetaNumeroSerie %>" />
                        <asp:BoundField DataField="RazonSocialCliente" HeaderText="<%$ Resources:Textos, EtiquetaEmpresaCliente %>" />
                        <asp:BoundField DataField="NombrePaquete" HeaderText="<%$ Resources:Textos, EtiquetaPaqueteActivo %>" />
                        <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaEstadoActivo %>">
                            <ItemTemplate>
                                <span class='badge <%# ObtenerClaseEstado((EstadoActivo)Eval("Estado")) %>'>
                                    <%# ObtenerTextoEstado((EstadoActivo)Eval("Estado")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <div class="acciones-fila">
                                    <asp:LinkButton ID="lnkEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IdActivo") %>'
                                        CssClass="btn-outline" CausesValidation="false" data-patente="GestionarActivos">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"></path></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonEditar %>" /></span>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkBaja" runat="server" CommandName="Baja" CommandArgument='<%# Eval("IdActivo") %>'
                                        CssClass="btn-outline-peligro" CausesValidation="false" data-patente="GestionarActivos"
                                        OnClientClick="return confirm('¿Confirma que desea dar de baja el activo?');">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDarBaja %>" /></span>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinActivos %>" />
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <uc:Paginador ID="ucPaginador" runat="server" ClaveResumen="MensajeResumenPaginadoActivos" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
        </asp:Panel>
    </div>

    <asp:Panel ID="pnlFormularioActivo" runat="server" CssClass="tarjeta" Visible="false">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="2" y1="21" x2="22" y2="21"></line></svg>
            </span>
            <h2 id="tituloFormulario" runat="server"></h2>
        </div>
        <asp:HiddenField ID="hidIdActivo" runat="server" Value="0" />

        <div class="fila-formulario">
            <div class="campo-formulario">
                <label for="<%= txtNombre.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaNombreActivo %>" /></label>
                <asp:TextBox ID="txtNombre" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionNombreActivoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Activo" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtModelo.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaModeloActivo %>" /></label>
                <asp:TextBox ID="txtModelo" runat="server" />
                <asp:RequiredFieldValidator ID="rfvModelo" runat="server" ControlToValidate="txtModelo"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionModeloObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Activo" />
            </div>

            <div class="campo-formulario">
                <label for="<%= txtNumeroSerie.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaNumeroSerie %>" /></label>
                <asp:TextBox ID="txtNumeroSerie" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNumeroSerie" runat="server" ControlToValidate="txtNumeroSerie"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionNumeroSerieObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Activo" />
            </div>
        </div>

        <div class="fila-formulario">
            <div class="campo-formulario">
                <label for="<%= ddlPaquete.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaPaqueteActivo %>" /></label>
                <asp:DropDownList ID="ddlPaquete" runat="server" />
                <asp:RequiredFieldValidator ID="rfvPaquete" runat="server" ControlToValidate="ddlPaquete" InitialValue=""
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionPaqueteObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Activo" />
            </div>

            <div class="campo-formulario">
                <label for="<%= ddlCliente.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEmpresaCliente %>" /></label>
                <asp:DropDownList ID="ddlCliente" runat="server" />
                <asp:RequiredFieldValidator ID="rfvCliente" runat="server" ControlToValidate="ddlCliente" InitialValue=""
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionEmpresaObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Activo" />
            </div>

            <div class="campo-formulario">
                <label for="<%= ddlEstado.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEstadoActivo %>" /></label>
                <asp:DropDownList ID="ddlEstado" runat="server" />
            </div>
        </div>

        <div class="campo-formulario">
            <label for="<%= txtEspecificaciones.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEspecificaciones %>" /></label>
            <asp:TextBox ID="txtEspecificaciones" runat="server" TextMode="MultiLine" Rows="3" CssClass="campo-mensaje" />
        </div>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" data-patente="GestionarActivos" ValidationGroup="Activo" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnCancelar" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCancelar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </asp:LinkButton>
        </div>
        <asp:ValidationSummary ID="vsActivo" runat="server" CssClass="texto-validacion" ValidationGroup="Activo" />
    </asp:Panel>
</asp:Content>
