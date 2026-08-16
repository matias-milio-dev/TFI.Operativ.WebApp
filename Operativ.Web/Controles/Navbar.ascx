<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navbar.ascx.cs" Inherits="Operativ.Web.Controles.Navbar" %>
<div class="navbar">
    <span class="navbar-marca">Operativ<span class="navbar-marca-acento">.</span></span>
    <asp:HyperLink ID="lnkHome" runat="server" CssClass="navbar-link navbar-link-activo" Text="<%$ Resources:Textos, EnlaceInicio %>" />
</div>
