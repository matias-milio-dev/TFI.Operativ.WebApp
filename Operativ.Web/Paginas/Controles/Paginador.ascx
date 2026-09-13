<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Paginador.ascx.cs" Inherits="Operativ.Web.Controles.Paginador" %>
<div class="paginado">
    <asp:Literal ID="litResumen" runat="server" />
    <div class="paginado-controles">
        <asp:Button ID="btnAnterior" runat="server" Text="<%$ Resources:Textos, BotonPaginaAnterior %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnAnterior_Click" />
        <span class="paginado-numero"><asp:Literal ID="litNumeroPagina" runat="server" /></span>
        <asp:Button ID="btnSiguiente" runat="server" Text="<%$ Resources:Textos, BotonPaginaSiguiente %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnSiguiente_Click" />
    </div>
</div>
