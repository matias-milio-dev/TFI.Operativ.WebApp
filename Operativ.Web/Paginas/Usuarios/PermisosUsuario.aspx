<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PermisosUsuario.aspx.cs" Inherits="Operativ.Web.Paginas.PermisosUsuario" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentPermisosUsuario" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
            </span>
            <div>
                <h1 id="tituloPermisos" runat="server"></h1>
                <p runat="server" meta:resourcekey="DescripcionPermisosUsuario">Asigná los permisos que tendrá este usuario, además de los que ya tiene por su familia.</p>
            </div>
        </div>

        <div class="barra-busqueda">
            <div class="campo-formulario campo-busqueda-permisos">
                <span class="icono-busqueda-permisos">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                </span>
                <input id="txtBuscarPermiso" runat="server" type="text" class="campo-busqueda-permisos-input" autocomplete="off" />
            </div>
            <div class="campo-formulario">
                <asp:DropDownList ID="ddlFiltroFamilia" runat="server" CssClass="select-filtro-familia-permisos" />
            </div>
        </div>

        <div class="acciones-permisos-masivas">
            <button type="button" class="btn-outline" onclick="Operativ.seleccionarTodosPermisos()">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonSeleccionarTodos %>" />
            </button>
            <button type="button" class="btn-primario" onclick="Operativ.quitarTodosPermisos()">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonQuitarTodos %>" />
            </button>
        </div>

        <div id="listaPermisos" class="lista-permisos">
            <asp:CheckBoxList ID="chkPatentes" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" />
        </div>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
            <asp:HyperLink ID="lnkVolver" runat="server" CssClass="btn-outline" Text="<%$ Resources:Textos, EnlaceVolverGestionUsuarios %>" />
        </div>
    </div>
</asp:Content>
