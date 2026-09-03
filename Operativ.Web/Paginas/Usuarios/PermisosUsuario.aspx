<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PermisosUsuario.aspx.cs" Inherits="Operativ.Web.Paginas.PermisosUsuario" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentPermisosUsuario" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
            </span>
            <div>
                <h1 id="tituloPermisos" runat="server"></h1>
                <p runat="server" meta:resourcekey="DescripcionPermisosUsuario">Marcá las patentes que querés asignarle individualmente a este usuario, además de las que ya tiene por su familia.</p>
            </div>
        </div>

        <asp:CheckBoxList ID="chkPatentes" runat="server" CssClass="lista-patentes" RepeatDirection="Vertical" RepeatLayout="Flow" />

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
            <asp:HyperLink ID="lnkVolver" runat="server" CssClass="btn-outline" Text="<%$ Resources:Textos, EnlaceVolverGestionUsuarios %>" />
        </div>
    </div>
</asp:Content>
