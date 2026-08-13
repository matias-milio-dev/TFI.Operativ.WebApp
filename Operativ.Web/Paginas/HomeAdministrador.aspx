<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeAdministrador.aspx.cs" Inherits="Operativ.Web.Paginas.HomeAdministrador" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeAdministrador" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1>Panel de Administrador</h1>
        <p>Gestión de usuarios y permisos de la plataforma Operativ.</p>
    </div>
    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />
</asp:Content>
