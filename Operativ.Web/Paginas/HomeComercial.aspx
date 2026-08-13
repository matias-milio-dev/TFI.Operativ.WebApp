<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeComercial.aspx.cs" Inherits="Operativ.Web.Paginas.HomeComercial" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeComercial" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1>Panel Comercial</h1>
        <p>Gestión de clientes y catálogo de la plataforma Operativ.</p>
    </div>
    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />
</asp:Content>
