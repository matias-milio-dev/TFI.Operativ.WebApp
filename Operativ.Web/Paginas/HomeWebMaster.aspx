<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeWebMaster.aspx.cs" Inherits="Operativ.Web.Paginas.HomeWebMaster" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeWebMaster" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel de Web Master</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Mantenimiento técnico de la plataforma Operativ.</p>
    </div>
    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />
</asp:Content>
