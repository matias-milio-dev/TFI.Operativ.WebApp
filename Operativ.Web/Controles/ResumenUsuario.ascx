<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResumenUsuario.ascx.cs" Inherits="Operativ.Web.Controles.ResumenUsuario" %>
<div class="resumen-usuario">
    <asp:Label ID="lblBienvenida" runat="server" />
    <asp:LinkButton ID="lnkCerrarSesion" runat="server" Text="Cerrar sesión" OnClick="lnkCerrarSesion_Click" />
</div>
