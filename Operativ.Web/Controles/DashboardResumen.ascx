<%@ Control Codebehind="DashboardResumen.ascx.cs" Inherits="Operativ.Web.Controles.DashboardResumen" Language="C#" %>
<div class="row g-3">
    <div class="col-6 col-md-3">
        <div class="card card-indicador p-3 text-center">
            <div class="valor text-primary"><asp:Literal ID="litActivos" runat="server">0</asp:Literal></div>
            <div class="text-muted small"><asp:Literal ID="litEtiquetaActivos" runat="server" /></div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="card card-indicador p-3 text-center">
            <div class="valor text-warning"><asp:Literal ID="litIncidentesAbiertos" runat="server">0</asp:Literal></div>
            <div class="text-muted small"><asp:Literal ID="litEtiquetaIncidentes" runat="server" /></div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="card card-indicador p-3 text-center">
            <div class="valor text-success"><asp:Literal ID="litSuscripcionesActivas" runat="server">0</asp:Literal></div>
            <div class="text-muted small"><asp:Literal ID="litEtiquetaSuscripciones" runat="server" /></div>
        </div>
    </div>
    <div class="col-6 col-md-3">
        <div class="card card-indicador p-3 text-center">
            <div class="valor text-danger"><asp:Literal ID="litAlertasUrgentes" runat="server">0</asp:Literal></div>
            <div class="text-muted small"><asp:Literal ID="litEtiquetaAlertas" runat="server" /></div>
        </div>
    </div>
</div>
