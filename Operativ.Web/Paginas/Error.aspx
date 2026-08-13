<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Operativ.Web.Paginas.Error" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentError" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1>Se produjo un error</h1>
        <p>Ocurrió un problema al procesar su solicitud. Intente nuevamente más tarde.</p>
        <asp:HyperLink ID="lnkVolverLogin" runat="server" NavigateUrl="~/Login.aspx" CssClass="enlace-secundario" Text="Volver al inicio" />
    </div>
</asp:Content>
