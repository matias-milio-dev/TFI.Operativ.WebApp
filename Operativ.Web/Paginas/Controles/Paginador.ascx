<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Paginador.ascx.cs" Inherits="Operativ.Web.Controles.Paginador" %>
<div class="paginado">
    <asp:Literal ID="litResumen" runat="server" />
    <asp:Panel ID="pnlControlesTexto" runat="server" CssClass="paginado-controles">
        <asp:Button ID="btnAnterior" runat="server" Text="<%$ Resources:Textos, BotonPaginaAnterior %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnAnterior_Click" />
        <span class="paginado-numero"><asp:Literal ID="litNumeroPagina" runat="server" /></span>
        <asp:Button ID="btnSiguiente" runat="server" Text="<%$ Resources:Textos, BotonPaginaSiguiente %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnSiguiente_Click" />
    </asp:Panel>
    <asp:Panel ID="pnlControlesNumerados" runat="server" CssClass="paginado-numerado" Visible="false">
        <asp:Button ID="btnAnteriorNumerado" runat="server" Text="&lsaquo;" CssClass="paginado-flecha" CausesValidation="false" OnClick="btnAnterior_Click" />
        <asp:Button ID="btnPagina1" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnPagina2" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnPagina3" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnPagina4" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnPagina5" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnPagina6" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnPagina7" runat="server" CssClass="paginado-pagina" CausesValidation="false" CommandName="IrAPagina" OnCommand="btnPagina_Command" />
        <asp:Button ID="btnSiguienteNumerado" runat="server" Text="&rsaquo;" CssClass="paginado-flecha" CausesValidation="false" OnClick="btnSiguiente_Click" />
    </asp:Panel>
</div>
