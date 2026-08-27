<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Notificaciones.ascx.cs" Inherits="Operativ.Web.Controles.Notificaciones" %>
<asp:Panel ID="pnlNotificacion" runat="server" CssClass="notificacion" Visible="false">
    <asp:Label ID="lblMensaje" runat="server" />
    <asp:HyperLink ID="lnkDesbloquearUsuario" runat="server" CssClass="notificacion-accion" Visible="false" Text="<%$ Resources:Textos, EnlaceDesbloquearAhora %>" />
</asp:Panel>
