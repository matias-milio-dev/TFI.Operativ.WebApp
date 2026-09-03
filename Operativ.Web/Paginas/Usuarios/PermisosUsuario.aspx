<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PermisosUsuario.aspx.cs" Inherits="Operativ.Web.Paginas.PermisosUsuario" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentPermisosUsuario" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta tarjeta-permisos">
        <div class="permisos-encabezado">
            <span class="icono-cuadrado">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
            </span>
            <div>
                <h1 id="tituloPermisos" runat="server"></h1>
                <p runat="server" meta:resourcekey="DescripcionPermisosUsuario">Asigná los permisos que tendrá este usuario. También podés gestionarlos por familia.</p>
            </div>
        </div>

        <div class="permisos-herramientas">
            <div class="permisos-herramientas-izquierda">
                <div class="campo-busqueda-permisos">
                    <span class="icono-busqueda-permisos">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    </span>
                    <input id="txtBuscarPermiso" runat="server" type="text" class="campo-busqueda-permisos-input" autocomplete="off" />
                </div>
                <asp:DropDownList ID="ddlFiltroFamilia" runat="server" CssClass="select-filtro-familia-permisos" />
            </div>
            <div class="permisos-herramientas-derecha">
                <button type="button" class="btn-outline" onclick="Operativ.seleccionarTodosPermisos()">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonSeleccionarTodos %>" />
                </button>
                <button type="button" class="btn-primario" onclick="Operativ.quitarTodosPermisos()">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonQuitarTodos %>" />
                </button>
            </div>
        </div>

        <div id="listaPermisos" class="lista-permisos">
            <asp:Repeater ID="rptCategorias" runat="server" OnItemDataBound="rptCategorias_ItemDataBound">
                <ItemTemplate>
                    <div id="grupoPermiso" runat="server" class="grupo-permiso">
                        <div class="grupo-permiso-encabezado" onclick="Operativ.alternarGrupoPermiso(this)">
                            <div class="grupo-permiso-encabezado-izquierda">
                                <svg class="grupo-permiso-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"></polyline></svg>
                                <span id="spanTituloCategoria" runat="server" class="grupo-permiso-titulo"></span>
                                <span id="spanCantidadCategoria" runat="server" class="grupo-permiso-contador"></span>
                            </div>
                            <div class="grupo-permiso-encabezado-derecha">
                                <span id="spanSeleccionadosCategoria" runat="server" class="grupo-permiso-seleccionados"></span>
                                <button type="button" id="btnAlternarCategoria" runat="server" class="grupo-permiso-alternar" onclick="Operativ.alternarSeleccionGrupo(this, event)">
                                    <svg class="icono-grupo-mas" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                                    <svg class="icono-grupo-menos" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line></svg>
                                </button>
                            </div>
                        </div>
                        <div class="grupo-permiso-cuerpo">
                            <asp:Repeater ID="rptPermisos" runat="server" OnItemDataBound="rptPermisos_ItemDataBound">
                                <ItemTemplate>
                                    <div id="filaPermiso" runat="server" class="fila-permiso">
                                        <asp:HiddenField ID="hidIdPatente" runat="server" />
                                        <span class="fila-permiso-checkbox">
                                            <asp:CheckBox ID="chkSeleccionada" runat="server" CssClass="chk-permiso" />
                                        </span>
                                        <div class="fila-permiso-texto">
                                            <span class="fila-permiso-nombre-linea">
                                                <span id="spanNombrePatente" runat="server" class="fila-permiso-nombre"></span>
                                                <span id="iconoInfoPatente" runat="server" class="fila-permiso-info">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>
                                                </span>
                                            </span>
                                            <span id="spanDescripcionPatente" runat="server" class="fila-permiso-descripcion"></span>
                                        </div>
                                        <span id="spanBadgeHeredada" runat="server"></span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="permisos-pie">
            <asp:HyperLink ID="lnkVolver" runat="server" CssClass="btn-outline" Text="<%$ Resources:Textos, BotonCancelar %>" />
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardarCambios %>" />
            </asp:LinkButton>
        </div>
    </div>
</asp:Content>
