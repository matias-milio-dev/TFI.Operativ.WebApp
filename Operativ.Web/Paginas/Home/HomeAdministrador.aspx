<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeAdministrador.aspx.cs" Inherits="Operativ.Web.Paginas.HomeAdministrador" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Paginas/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeAdministrador" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel de Administrador</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Gestión de usuarios y permisos de la plataforma Operativ.</p>
    </div>
   <%-- <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />--%>
</asp:Content>
