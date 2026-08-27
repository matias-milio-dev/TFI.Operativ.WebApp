<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Notificaciones.ascx.cs" Inherits="Operativ.Web.Controles.Notificaciones" %>
<asp:Panel ID="pnlNotificacion" runat="server" CssClass="notificacion" Visible="false">
    <asp:Label ID="lblMensaje" runat="server" />
    <div class="notificacion-accion">
        <asp:HyperLink ID="lnkDesbloquearUsuario" runat="server" CssClass="btn-primario" Visible="false" Text="<%$ Resources:Textos, BotonDesbloquearUsuario %>" />
    </div>
</asp:Panel>
