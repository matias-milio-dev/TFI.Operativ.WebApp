<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResumenUsuario.ascx.cs" Inherits="Operativ.Web.Controles.ResumenUsuario" %>
<%@ Register TagPrefix="uc" TagName="SelectorIdioma" Src="~/Paginas/Controles/SelectorIdioma.ascx" %>
<div class="resumen-usuario">
    <asp:Label ID="lblBienvenida" runat="server" />
    <div class="menu-usuario" id="menuUsuario">
        <button type="button" id="btnMenuUsuario" class="menu-usuario-boton" onclick="Operativ.alternarMenuUsuario(event)" aria-haspopup="true" aria-expanded="false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
            <svg class="menu-usuario-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
        </button>
        <div id="menuUsuarioDropdown" class="menu-usuario-dropdown">
            <div class="menu-usuario-item menu-usuario-idioma">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="2" y1="12" x2="22" y2="12"></line><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
                <span class="menu-usuario-item-texto"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaIdioma %>" /></span>
                <uc:SelectorIdioma ID="ucSelectorIdioma" runat="server" />
            </div>
            <div class="menu-usuario-separador"></div>
            <button type="button" class="menu-usuario-item menu-usuario-item-boton" onclick="Operativ.abrirModalCambiarClave(event)">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                <span class="menu-usuario-item-texto"><asp:Literal runat="server" Text="<%$ Resources:Textos, EnlaceCambiarContrasena %>" /></span>
            </button>
            <div class="menu-usuario-separador"></div>
            <asp:LinkButton ID="lnkCerrarSesion" runat="server" CausesValidation="false" CssClass="menu-usuario-item menu-usuario-item-boton" OnClick="lnkCerrarSesion_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                <span class="menu-usuario-item-texto"><asp:Literal runat="server" Text="<%$ Resources:Textos, EnlaceCerrarSesion %>" /></span>
            </asp:LinkButton>
        </div>
    </div>
</div>
