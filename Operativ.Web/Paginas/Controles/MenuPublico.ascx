<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MenuPublico.ascx.cs" Inherits="Operativ.Web.Controles.MenuPublico" %>
<nav class="menu-publico">
    <asp:HyperLink ID="lnkInicio" runat="server" CssClass="menu-publico-link" Text="<%$ Resources:Textos, EnlacePublicoInicio %>" />
    <asp:HyperLink ID="lnkQuienesSomos" runat="server" CssClass="menu-publico-link" NavigateUrl="~/Paginas/Publico/QuienesSomos.aspx" Text="<%$ Resources:Textos, EnlacePublicoQuienesSomos %>" />
    <asp:HyperLink ID="lnkServicios" runat="server" CssClass="menu-publico-link" NavigateUrl="~/Paginas/Publico/Servicios.aspx" Text="<%$ Resources:Textos, EnlacePublicoServicios %>" />
    <asp:HyperLink ID="lnkContacto" runat="server" CssClass="menu-publico-link" NavigateUrl="~/Paginas/Publico/Contacto.aspx" Text="<%$ Resources:Textos, EnlacePublicoContacto %>" />
</nav>
