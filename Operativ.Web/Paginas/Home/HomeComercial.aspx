<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeComercial.aspx.cs" Inherits="Operativ.Web.Paginas.HomeComercial" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Paginas/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeComercial" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel Comercial</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Gestión de clientes y catálogo de la plataforma Operativ.</p>
    </div>
<%--    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />--%>
</asp:Content>
