<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NoAutorizado.aspx.cs" Inherits="Operativ.Web.Paginas.NoAutorizado" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentNoAutorizado" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloNoAutorizado">Acceso no autorizado</h1>
        <p runat="server" meta:resourcekey="DescripcionNoAutorizado">No tiene permisos para acceder a la página solicitada.</p>
        <asp:HyperLink ID="lnkVolverHome" runat="server" CssClass="enlace-secundario" meta:resourcekey="lnkVolverHome" Text="Volver a mi página principal" />
    </div>
</asp:Content>
