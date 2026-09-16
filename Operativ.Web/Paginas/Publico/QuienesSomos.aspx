<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="QuienesSomos.aspx.cs" Inherits="Operativ.Web.Paginas.QuienesSomos" MasterPageFile="~/Master/Publico.Master" %>
<asp:Content ID="ContentQuienesSomos" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <section class="hero-publico">
        <div class="hero-publico-texto">
            <span class="eyebrow"><span class="eyebrow-linea"></span><asp:Literal runat="server" Text="<%$ Resources:Textos, EyebrowQuienesSomos %>" /></span>
            <h1><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloQuienesSomos %>" /></h1>
            <p class="hero-publico-bajada"><asp:Literal runat="server" Text="<%$ Resources:Textos, BajadaQuienesSomos %>" /></p>
        </div>
        <div class="hero-publico-imagen">
            <asp:Image ID="imgHero" runat="server" ImageUrl="~/Imagenes/hero-quienes-somos.svg" AlternateText="<%$ Resources:Textos, AltHeroQuienesSomos %>" />
        </div>
    </section>

    <section class="bloque-publico">
        <span class="eyebrow-linea"></span>
        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloNuestroProposito %>" /></h2>
        <p class="bloque-publico-texto"><asp:Literal runat="server" Text="<%$ Resources:Textos, TextoNuestroProposito %>" /></p>
    </section>

    <section class="bloque-publico">
        <span class="eyebrow-linea"></span>
        <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloNuestrosValores %>" /></h2>
        <div class="grilla-valores">
            <div class="tarjeta-valor">
                <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m11 17 2 2a1 1 0 1 0 3-3"></path><path d="m14 14 2.5 2.5a1 1 0 1 0 3-3l-3.88-3.88a3 3 0 0 0-4.24 0l-.88.88a1 1 0 1 1-3-3l2.81-2.81a5.79 5.79 0 0 1 7.06-.87l.47.28a2 2 0 0 0 1.42.25L21 4"></path><path d="m21 3 1 11h-2"></path><path d="M3 3 2 14l6.5 6.5a1 1 0 1 0 3-3"></path><path d="M3 4h8"></path></svg></span>
                <h3><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloValorCompromiso %>" /></h3>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, TextoValorCompromiso %>" /></p>
            </div>
            <div class="tarjeta-valor">
                <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 14c.2-1 .7-1.7 1.5-2.5 1-.9 1.5-2.2 1.5-3.5a6 6 0 0 0-12 0c0 1.3.5 2.6 1.5 3.5.8.8 1.3 1.5 1.5 2.5"></path><path d="M9 18h6"></path><path d="M10 22h4"></path></svg></span>
                <h3><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloValorInnovacion %>" /></h3>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, TextoValorInnovacion %>" /></p>
            </div>
            <div class="tarjeta-valor">
                <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg></span>
                <h3><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloValorTransparencia %>" /></h3>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, TextoValorTransparencia %>" /></p>
            </div>
            <div class="tarjeta-valor">
                <span class="icono-circulo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg></span>
                <h3><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloValorOrientacionCliente %>" /></h3>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, TextoValorOrientacionCliente %>" /></p>
            </div>
        </div>
    </section>
</asp:Content>
