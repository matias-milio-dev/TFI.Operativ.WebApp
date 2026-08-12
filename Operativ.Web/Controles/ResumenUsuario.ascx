<%@ Control Codebehind="ResumenUsuario.ascx.cs" Inherits="Operativ.Web.Controles.ResumenUsuario" Language="C#" %>
<asp:Panel ID="pnlAutenticado" runat="server" CssClass="dropdown">
    <a class="btn btn-outline-light btn-sm dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
        <asp:Literal ID="litNombreYRol" runat="server" />
    </a>
    <ul class="dropdown-menu dropdown-menu-end">
        <li><asp:HyperLink ID="lnkPerfil" runat="server" CssClass="dropdown-item" NavigateUrl="~/Paginas/MiPerfil.aspx" /></li>
        <li><asp:HyperLink ID="lnkSuscripciones" runat="server" CssClass="dropdown-item" NavigateUrl="~/Paginas/GestionSuscripcionesCliente.aspx" /></li>
        <li><hr class="dropdown-divider" /></li>
        <li><asp:LinkButton ID="btnCerrarSesion" runat="server" CssClass="dropdown-item" OnClick="btnCerrarSesion_Click" /></li>
    </ul>
</asp:Panel>
<asp:HyperLink ID="lnkIngresar" runat="server" CssClass="btn btn-outline-light btn-sm" NavigateUrl="~/Login.aspx" />
