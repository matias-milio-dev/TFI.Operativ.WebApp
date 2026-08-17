<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectorIdioma.ascx.cs" Inherits="Operativ.Web.Controles.SelectorIdioma" %>
<div class="selector-idioma-pill">
    <asp:LinkButton ID="lnkEspanol" runat="server" Text="ES" CausesValidation="false" CssClass="selector-idioma-pill-opcion" OnClick="lnkEspanol_Click" />
    <asp:LinkButton ID="lnkIngles" runat="server" Text="EN" CausesValidation="false" CssClass="selector-idioma-pill-opcion" OnClick="lnkIngles_Click" />
</div>
