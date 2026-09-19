<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GestionIncidentes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionIncidentes" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Enums" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentGestionIncidentes" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloGestionIncidentes">Incidentes</h1>
                    <p runat="server" meta:resourcekey="DescripcionGestionIncidentes">Incidentes reportados sobre los activos y su estado de resolución.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroIncidentes %>" /></label>
                    <asp:TextBox ID="txtFiltro" runat="server" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= ddlFiltroEstado.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEstadoIncidente %>" /></label>
                    <asp:DropDownList ID="ddlFiltroEstado" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
            <asp:LinkButton ID="btnNuevoIncidente" runat="server" CssClass="btn-primario" CausesValidation="false" data-patente="ReportarIncidentes" OnClick="btnNuevoIncidente_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonNuevoIncidente %>" />
            </asp:LinkButton>
        </div>

        <asp:Panel ID="pnlListado" runat="server">
            <div class="tabla-contenedor">
                <asp:GridView ID="gvIncidentes" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
                    DataKeyNames="IdIncidente" OnRowCommand="gvIncidentes_RowCommand" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="NumeroIncidente" HeaderText="<%$ Resources:Textos, EtiquetaNumeroIncidente %>" />
                        <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaAltaIncidente %>">
                            <ItemTemplate>
                                <%# ((DateTime)Eval("FechaAlta")).ToString("dd/MM/yyyy HH:mm") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="NombreActivo" HeaderText="<%$ Resources:Textos, EtiquetaActivoIncidente %>" />
                        <asp:BoundField DataField="RazonSocialCliente" HeaderText="<%$ Resources:Textos, EtiquetaEmpresaCliente %>" />
                        <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaPrioridadIncidente %>">
                            <ItemTemplate>
                                <span class='badge <%# ObtenerClasePrioridad((PrioridadIncidente)Eval("Prioridad")) %>'>
                                    <%# ObtenerTextoPrioridad((PrioridadIncidente)Eval("Prioridad")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaEstadoIncidente %>">
                            <ItemTemplate>
                                <span class='badge <%# ObtenerClaseEstadoIncidente((EstadoIncidente)Eval("Estado")) %>'>
                                    <%# ObtenerTextoEstadoIncidente((EstadoIncidente)Eval("Estado")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <div class="acciones-fila">
                                    <asp:LinkButton ID="lnkVer" runat="server" CommandName="Ver" CommandArgument='<%# Eval("IdIncidente") %>'
                                        CssClass="btn-outline" CausesValidation="false">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonVer %>" /></span>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkCerrar" runat="server" CommandName="Cerrar" CommandArgument='<%# Eval("IdIncidente") %>'
                                        CssClass="btn-outline" CausesValidation="false" data-patente="CerrarIncidente"
                                        Visible='<%# !(bool)Eval("EstaCerrado") %>'>
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCerrarIncidente %>" /></span>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinIncidentes %>" />
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <uc:Paginador ID="ucPaginador" runat="server" ClaveResumen="MensajeResumenPaginadoIncidentes" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
        </asp:Panel>
    </div>

    <asp:Panel ID="pnlFormularioIncidente" runat="server" CssClass="tarjeta" Visible="false">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line></svg>
            </span>
            <h2 runat="server" meta:resourcekey="TituloFormularioAltaIncidente">Nuevo incidente</h2>
        </div>

        <div class="fila-formulario">
            <div class="campo-formulario">
                <label for="<%= ddlActivo.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaActivoIncidente %>" /></label>
                <asp:DropDownList ID="ddlActivo" runat="server" />
                <asp:RequiredFieldValidator ID="rfvActivo" runat="server" ControlToValidate="ddlActivo" InitialValue=""
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionActivoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Incidente" />
            </div>

            <div class="campo-formulario">
                <label for="<%= ddlCategoria.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCategoriaIncidente %>" /></label>
                <asp:DropDownList ID="ddlCategoria" runat="server" />
            </div>

            <div class="campo-formulario">
                <label for="<%= ddlPrioridad.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaPrioridadIncidente %>" /></label>
                <asp:DropDownList ID="ddlPrioridad" runat="server" />
            </div>
        </div>

        <div class="campo-formulario">
            <label for="<%= txtDescripcion.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaDescripcionIncidente %>" /></label>
            <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="4" CssClass="campo-mensaje" />
            <asp:RequiredFieldValidator ID="rfvDescripcion" runat="server" ControlToValidate="txtDescripcion"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionDescripcionIncidenteObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Incidente" />
        </div>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" data-patente="ReportarIncidentes" ValidationGroup="Incidente" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnCancelarAlta" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCancelarAlta_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </asp:LinkButton>
        </div>
        <asp:ValidationSummary ID="vsIncidente" runat="server" CssClass="texto-validacion" ValidationGroup="Incidente" />
    </asp:Panel>

    <asp:Panel ID="pnlDetalleIncidente" runat="server" CssClass="tarjeta" Visible="false">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>
            </span>
            <h2 id="tituloDetalle" runat="server"></h2>
        </div>
        <asp:HiddenField ID="hidIdIncidente" runat="server" Value="0" />

        <div class="detalle-incidente">
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaActivoIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litActivo" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEmpresaCliente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litCliente" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaCategoriaIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litCategoria" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaPrioridadIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litPrioridad" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEstadoIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litEstado" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFechaAltaIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litFechaAlta" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo detalle-incidente-campo-ancho">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaDescripcionIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litDescripcion" runat="server" /></span>
            </div>
        </div>

        <asp:Panel ID="pnlResolucion" runat="server" Visible="false" CssClass="detalle-incidente-resolucion">
            <div class="detalle-incidente-campo detalle-incidente-campo-ancho">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFechaCierreIncidente %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litFechaCierre" runat="server" /></span>
            </div>
            <div class="detalle-incidente-campo detalle-incidente-campo-ancho">
                <span class="detalle-incidente-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaComentarioResolucion %>" /></span>
                <span class="detalle-incidente-valor"><asp:Literal ID="litComentarioResolucion" runat="server" /></span>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCierre" runat="server" Visible="false">
            <h3 class="subtitulo-formulario"><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloCerrarIncidente %>" /></h3>
            <div class="campo-formulario">
                <label for="<%= txtComentarioResolucion.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaComentarioResolucion %>" /></label>
                <asp:TextBox ID="txtComentarioResolucion" runat="server" TextMode="MultiLine" Rows="3" CssClass="campo-mensaje" />
                <asp:RequiredFieldValidator ID="rfvComentarioResolucion" runat="server" ControlToValidate="txtComentarioResolucion" Enabled="false"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionComentarioResolucionObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Cierre" />
            </div>
            <div class="acciones-formulario">
                <asp:LinkButton ID="btnConfirmarCierre" runat="server" CssClass="btn-primario" data-patente="CerrarIncidente" ValidationGroup="Cierre" OnClick="btnConfirmarCierre_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonConfirmarCierre %>" />
                </asp:LinkButton>
            </div>
            <asp:ValidationSummary ID="vsCierre" runat="server" CssClass="texto-validacion" ValidationGroup="Cierre" />
        </asp:Panel>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnCerrarDetalle" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonVolver %>" />
            </asp:LinkButton>
        </div>
    </asp:Panel>
</asp:Content>
