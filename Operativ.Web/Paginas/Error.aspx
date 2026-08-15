<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Operativ.Web.Paginas.Error" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentError" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloError">Se produjo un error</h1>
        <p runat="server" meta:resourcekey="DescripcionError">Ocurrió un problema al procesar su solicitud. Intente nuevamente más tarde.</p>
        <asp:HyperLink ID="lnkVolverLogin" runat="server" NavigateUrl="~/Login.aspx" CssClass="enlace-secundario" meta:resourcekey="lnkVolverLogin" Text="Volver al inicio" />
    </div>
</asp:Content>
