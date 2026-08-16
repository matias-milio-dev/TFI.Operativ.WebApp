<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectorIdioma.ascx.cs" Inherits="Operativ.Web.Controles.SelectorIdioma" %>
<div class="selector-idioma">
    <asp:LinkButton ID="lnkEspanol" runat="server" Text="ES" CausesValidation="false" CssClass="selector-idioma-opcion" OnClick="lnkEspanol_Click" />
    <span class="selector-idioma-separador">|</span>
    <asp:LinkButton ID="lnkIngles" runat="server" Text="EN" CausesValidation="false" CssClass="selector-idioma-opcion" OnClick="lnkIngles_Click" />
</div>
