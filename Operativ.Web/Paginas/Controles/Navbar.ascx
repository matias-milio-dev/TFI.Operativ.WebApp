<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navbar.ascx.cs" Inherits="Operativ.Web.Controles.Navbar" %>
<div class="navbar">
    <span class="navbar-marca">Operativ<span class="navbar-marca-acento">.</span></span>
    <asp:HyperLink ID="lnkHome" runat="server" CssClass="navbar-link navbar-link-activo" Text="<%$ Resources:Textos, EnlaceInicio %>" />
    <asp:HyperLink ID="lnkUsuarios" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Usuarios/GestionUsuarios.aspx" Text="<%$ Resources:Textos, EnlaceUsuarios %>" Visible="false" />
</div>
