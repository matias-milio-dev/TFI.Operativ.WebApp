<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navbar.ascx.cs" Inherits="Operativ.Web.Controles.Navbar" %>
<div class="navbar">
    <span class="navbar-marca">Operativ<span class="navbar-marca-acento">.</span></span>
    <asp:HyperLink ID="lnkHome" runat="server" CssClass="navbar-link navbar-link-activo" Text="<%$ Resources:Textos, EnlaceInicio %>" />
    <asp:HyperLink ID="lnkUsuarios" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Usuarios/GestionUsuarios.aspx" Text="<%$ Resources:Textos, EnlaceUsuarios %>" Visible="false" />
    <asp:HyperLink ID="lnkClientes" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Clientes/GestionClientes.aspx" Text="<%$ Resources:Textos, EnlaceClientes %>" Visible="false" />
    <asp:HyperLink ID="lnkPaquetes" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Paquetes/GestionPaquetes.aspx" Text="<%$ Resources:Textos, EnlacePaquetes %>" Visible="false" />
    <asp:HyperLink ID="lnkActivos" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Activos/GestionActivos.aspx" Text="<%$ Resources:Textos, EnlaceActivos %>" Visible="false" />
    <asp:HyperLink ID="lnkBackup" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Sistema/BackupRestore.aspx" Text="<%$ Resources:Textos, EnlaceBackup %>" Visible="false" />
    <asp:HyperLink ID="lnkBitacora" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Sistema/ConsultarBitacora.aspx" Text="<%$ Resources:Textos, EnlaceBitacora %>" Visible="false" />
</div>
