<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Servicios.aspx.cs" Inherits="Operativ.Web.Paginas.Servicios" MasterPageFile="~/Master/Publico.Master" %>
<asp:Content ID="ContentServicios" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <section class="hero-publico">
        <div class="hero-publico-texto">
            <span class="eyebrow"><span class="eyebrow-linea"></span><asp:Literal runat="server" Text="<%$ Resources:Textos, EyebrowServicios %>" /></span>
            <h1><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloServicios %>" /></h1>
            <p class="hero-publico-bajada"><asp:Literal runat="server" Text="<%$ Resources:Textos, BajadaServicios %>" /></p>
        </div>
        <div class="hero-publico-imagen">
            <asp:Image ID="imgHero" runat="server" ImageUrl="~/Imagenes/hero-servicios.svg" AlternateText="<%$ Resources:Textos, AltHeroServicios %>" />
        </div>
    </section>

    <asp:Repeater ID="rptServicios" runat="server">
        <HeaderTemplate>
            <section class="grilla-servicios">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="tarjeta-servicio">
                <span class="icono-circulo"><asp:Literal runat="server" Mode="PassThrough" Text='<%# ObtenerIconoSvg((string)Eval("ClaveIcono")) %>' /></span>
                <h3><%# Eval("Nombre") %></h3>
                <p><%# Eval("Descripcion") %></p>
                <asp:HyperLink runat="server" CssClass="link-texto" NavigateUrl="~/Paginas/Publico/Contacto.aspx">
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, EnlaceSaberMas %>" />
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line><polyline points="12 5 19 12 12 19"></polyline></svg>
                </asp:HyperLink>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </section>
        </FooterTemplate>
    </asp:Repeater>
</asp:Content>
