<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResumenUsuario.ascx.cs" Inherits="Operativ.Web.Controles.ResumenUsuario" %>
<%@ Register TagPrefix="uc" TagName="SelectorIdioma" Src="~/Controles/SelectorIdioma.ascx" %>
<div class="resumen-usuario">
    <uc:SelectorIdioma ID="ucSelectorIdioma" runat="server" />
    <asp:Label ID="lblBienvenida" runat="server" />
    <asp:LinkButton ID="lnkCerrarSesion" runat="server" CausesValidation="false" CssClass="boton-logout" Text="<%$ Resources:Textos, EnlaceCerrarSesion %>" OnClick="lnkCerrarSesion_Click" />
</div>
