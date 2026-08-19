<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DashboardResumen.ascx.cs" Inherits="Operativ.Web.Controles.DashboardResumen" %>
<div class="dashboard-resumen">
    <div class="indicador">
        <span class="indicador-valor">0</span>
        <asp:Label ID="lblEtiquetaSuscripciones" runat="server" CssClass="indicador-etiqueta" Text="<%$ Resources:Textos, EtiquetaSuscripcionesActivas %>" />
    </div>
    <div class="indicador">
        <span class="indicador-valor">0</span>
        <asp:Label ID="lblEtiquetaIncidentes" runat="server" CssClass="indicador-etiqueta" Text="<%$ Resources:Textos, EtiquetaIncidentesAbiertos %>" />
    </div>
    <div class="indicador">
        <span class="indicador-valor">0</span>
        <asp:Label ID="lblEtiquetaActivos" runat="server" CssClass="indicador-etiqueta" Text="<%$ Resources:Textos, EtiquetaActivosRegistrados %>" />
    </div>
    <div class="indicador">
        <span class="indicador-valor">0</span>
        <asp:Label ID="lblEtiquetaFacturas" runat="server" CssClass="indicador-etiqueta" Text="<%$ Resources:Textos, EtiquetaFacturasPendientes %>" />
    </div>
</div>
