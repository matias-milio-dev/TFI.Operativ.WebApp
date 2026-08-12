<%@ Control Codebehind="Navbar.ascx.cs" Inherits="Operativ.Web.Controles.Navbar" Language="C#" %>
<%@ Register TagPrefix="uc" TagName="ResumenUsuario" Src="~/Controles/ResumenUsuario.ascx" %>
<nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
        <a class="navbar-brand" href="~/Default.aspx" runat="server">Operativ</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navOperativ">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navOperativ">
            <ul class="navbar-nav me-auto mb-2 mb-lg-0">
                <li class="nav-item"><asp:HyperLink ID="lnkInicio" runat="server" CssClass="nav-link" NavigateUrl="~/Default.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkUsuarios" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionUsuarios.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkFamilias" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionFamiliasPatentes.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkBitacora" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/ConsultaBitacora.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkClientes" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionClientes.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkPaquetes" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionPaquetes.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkSuscripciones" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionSuscripcionesCliente.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkActivos" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionActivos.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkIncidentes" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/GestionIncidentes.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkServicios" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/Servicios.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkMonitoreo" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/Monitoreo.aspx" /></li>
                <li class="nav-item"><asp:HyperLink ID="lnkAdministracion" runat="server" CssClass="nav-link" NavigateUrl="~/Paginas/Administracion.aspx" /></li>
            </ul>
            <div class="d-flex align-items-center">
                <div class="btn-group btn-group-sm me-3" role="group">
                    <asp:LinkButton ID="btnIdiomaEs" runat="server" CssClass="btn btn-outline-light" Text="ES" OnClick="btnIdiomaEs_Click" />
                    <asp:LinkButton ID="btnIdiomaEn" runat="server" CssClass="btn btn-outline-light" Text="EN" OnClick="btnIdiomaEn_Click" />
                </div>
                <uc:ResumenUsuario ID="ucResumenUsuario" runat="server" />
            </div>
        </div>
    </div>
</nav>
