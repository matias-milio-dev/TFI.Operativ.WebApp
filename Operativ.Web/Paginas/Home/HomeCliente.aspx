<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeCliente.aspx.cs" Inherits="Operativ.Web.Paginas.HomeCliente" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Paginas/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeCliente" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel de Cliente</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Suscripciones, activos, incidentes y facturas de su cuenta Operativ.</p>
    </div>
<%--    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />--%>
</asp:Content>
