<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GestionPaquetes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionPaquetes" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Enums" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentGestionPaquetes" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <asp:Panel ID="pnlPaquetes" runat="server" CssClass="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path><polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline><line x1="12" y1="22.08" x2="12" y2="12"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloGestionPaquetes">Paquetes de configuración</h1>
                    <p runat="server" meta:resourcekey="DescripcionGestionPaquetes">Alta, baja y modificación de las imágenes de sistema disponibles para los activos.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroPaquetes %>" /></label>
                    <asp:TextBox ID="txtFiltro" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
            <asp:LinkButton ID="btnNuevoPaquete" runat="server" CssClass="btn-primario" CausesValidation="false" data-patente="GestionarCatalogo" OnClick="btnNuevoPaquete_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonNuevoPaquete %>" />
            </asp:LinkButton>
        </div>

        <asp:Panel ID="pnlListado" runat="server">
            <div class="tabla-contenedor">
                <asp:GridView ID="gvPaquetes" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
                    DataKeyNames="IdPaquete" OnRowCommand="gvPaquetes_RowCommand" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="Nombre" HeaderText="<%$ Resources:Textos, EtiquetaNombrePaquete %>" />
                        <asp:BoundField DataField="Descripcion" HeaderText="<%$ Resources:Textos, EtiquetaDescripcionPaquete %>" />
                        <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaTipoPermiso %>">
                            <ItemTemplate>
                                <span class='badge <%# ObtenerClaseTipoPermiso((TipoPermiso)Eval("TipoPermiso")) %>'>
                                    <%# ObtenerTextoTipoPermiso((TipoPermiso)Eval("TipoPermiso")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <div class="acciones-fila">
                                    <asp:LinkButton ID="lnkEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IdPaquete") %>'
                                        CssClass="btn-outline" CausesValidation="false" data-patente="GestionarCatalogo">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"></path></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonEditar %>" /></span>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkBaja" runat="server" CommandName="Baja" CommandArgument='<%# Eval("IdPaquete") %>'
                                        CssClass="btn-outline-peligro" CausesValidation="false" data-patente="GestionarCatalogo"
                                        OnClientClick="return confirm('¿Confirma que desea dar de baja el paquete?');">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        <span class="texto-accion"><asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDarBaja %>" /></span>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinPaquetes %>" />
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <uc:Paginador ID="ucPaginador" runat="server" ClaveResumen="MensajeResumenPaginadoPaquetes" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
        </asp:Panel>
    </asp:Panel>

    <asp:Panel ID="pnlFormularioPaquete" runat="server" CssClass="tarjeta formulario-paquete" Visible="false">
        <div class="tarjeta-encabezado-titulo encabezado-formulario-paquete">
            <span class="icono-circulo icono-circulo-grande">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path><polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline><line x1="12" y1="22.08" x2="12" y2="12"></line></svg>
            </span>
            <div>
                <h1 id="tituloFormulario" runat="server"></h1>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, SubtituloFormularioPaquete %>" /></p>
            </div>
        </div>
        <asp:HiddenField ID="hidIdPaquete" runat="server" Value="0" />

        <div class="fila-formulario">
            <div class="campo-formulario">
                <label for="<%= txtNombre.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaNombrePaquete %>" /></label>
                <asp:TextBox ID="txtNombre" runat="server" MaxLength="100" placeholder="<%$ Resources:Textos, PlaceholderNombrePaquete %>" />
                <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionNombrePaqueteObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Paquete" />
            </div>

            <div class="campo-formulario">
                <label for="<%= ddlTipoPermiso.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaTipoPermiso %>" /></label>
                <asp:DropDownList ID="ddlTipoPermiso" runat="server" CssClass="select-tipo-permiso" />
            </div>
        </div>

        <div class="campo-formulario">
            <label for="<%= txtDescripcion.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaDescripcionPaquete %>" /></label>
            <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="4" CssClass="campo-mensaje" placeholder="<%$ Resources:Textos, PlaceholderDescripcionPaquete %>" />
            <div class="contador-caracteres" data-contador-para="<%= txtDescripcion.ClientID %>" data-maximo="300">0/300</div>
            <asp:RequiredFieldValidator ID="rfvDescripcion" runat="server" ControlToValidate="txtDescripcion"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionDescripcionPaqueteObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Paquete" />
        </div>

        <div class="seccion-programas">
            <div class="seccion-programas-encabezado">
                <div class="tarjeta-encabezado-titulo">
                    <span class="icono-circulo">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="12" rx="2"></rect><line x1="2" y1="20" x2="22" y2="20"></line><line x1="9" y1="16" x2="9" y2="20"></line><line x1="15" y1="16" x2="15" y2="20"></line></svg>
                    </span>
                    <div>
                        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaProgramasIncluidos %>" /></h2>
                        <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionProgramasIncluidos %>" /></p>
                    </div>
                </div>
                <div class="campo-busqueda-programas">
                    <span class="icono-busqueda-permisos">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    </span>
                    <input type="text" class="campo-busqueda-programas-input" autocomplete="off" placeholder="<%= Operativ.Web.Idioma.TextoRecurso.Obtener("PlaceholderBuscarPrograma") %>" />
                </div>
            </div>

            <asp:CheckBoxList ID="chkProgramas" runat="server" CssClass="lista-programas" RepeatLayout="UnorderedList" />
            <p class="sin-resultados-programas oculto-filtro"><asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinProgramas %>" /></p>
        </div>

        <div class="acciones-formulario acciones-formulario-pie">
            <asp:LinkButton ID="btnCancelar" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCancelar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" data-patente="GestionarCatalogo" ValidationGroup="Paquete" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
        </div>
        <asp:ValidationSummary ID="vsPaquete" runat="server" CssClass="texto-validacion" ValidationGroup="Paquete" />
    </asp:Panel>
</asp:Content>