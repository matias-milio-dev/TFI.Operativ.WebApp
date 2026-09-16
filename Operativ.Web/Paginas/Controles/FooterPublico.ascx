<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FooterPublico.ascx.cs" Inherits="Operativ.Web.Controles.FooterPublico" %>
<footer class="footer-publico">
    <div class="footer-publico-contenido">
        <span class="footer-publico-marca">Operativ<span class="footer-publico-marca-acento">.</span></span>
        <nav class="footer-publico-links">
            <asp:HyperLink ID="lnkFooterInicio" runat="server" NavigateUrl="~/Paginas/Publico/Servicios.aspx" Text="<%$ Resources:Textos, EnlacePublicoInicio %>" />
            <asp:HyperLink ID="lnkFooterQuienesSomos" runat="server" NavigateUrl="~/Paginas/Publico/QuienesSomos.aspx" Text="<%$ Resources:Textos, EnlacePublicoQuienesSomos %>" />
            <asp:HyperLink ID="lnkFooterServicios" runat="server" NavigateUrl="~/Paginas/Publico/Servicios.aspx" Text="<%$ Resources:Textos, EnlacePublicoServicios %>" />
            <asp:HyperLink ID="lnkFooterContacto" runat="server" NavigateUrl="~/Paginas/Publico/Contacto.aspx" Text="<%$ Resources:Textos, EnlacePublicoContacto %>" />
        </nav>
    </div>
    <p class="footer-publico-copyright"><asp:Literal ID="litCopyright" runat="server" Text="<%$ Resources:Textos, TextoCopyright %>" /></p>
</footer>
