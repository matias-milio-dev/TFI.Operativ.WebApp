<%@ Page Codebehind="Monitoreo.aspx.cs" Inherits="Operativ.Web.Paginas.Monitoreo" Language="C#" MasterPageFile="~/Master/Site.master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Controles/DashboardResumen.ascx" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>
    <uc:DashboardResumen ID="ucDashboard" runat="server" />
</asp:Content>
