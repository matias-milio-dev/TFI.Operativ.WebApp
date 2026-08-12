<%@ Page Codebehind="ConsultaBitacora.aspx.cs" Inherits="Operativ.Web.Paginas.ConsultaBitacora" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-2 align-items-end mb-3">
        <div class="col-auto">
            <label class="form-label small mb-0">Desde</label>
            <asp:TextBox ID="txtFechaDesde" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="col-auto">
            <label class="form-label small mb-0">Hasta</label>
            <asp:TextBox ID="txtFechaHasta" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="col-auto">
            <label class="form-label small mb-0">Acción</label>
            <asp:TextBox ID="txtAccion" runat="server" CssClass="form-control" placeholder="LOGIN, ALTA, BAJA..." />
        </div>
        <div class="col-auto">
            <label class="form-label small mb-0">Criticidad</label>
            <asp:DropDownList ID="ddlCriticidad" runat="server" CssClass="form-select">
                <asp:ListItem Text="(Todas)" Value="" />
                <asp:ListItem Text="Informativa" Value="INFORMATIVA" />
                <asp:ListItem Text="Advertencia" Value="ADVERTENCIA" />
                <asp:ListItem Text="Grave" Value="GRAVE" />
                <asp:ListItem Text="Crítica" Value="CRITICA" />
            </asp:DropDownList>
        </div>
        <div class="col-auto">
            <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonBuscar %>" OnClick="btnBuscar_Click" />
        </div>
    </div>

    <asp:GridView ID="gvBitacora" runat="server" CssClass="table table-striped table-sm tabla-operativ" AutoGenerateColumns="false"
        EmptyDataText="Sin resultados.">
        <Columns>
            <asp:BoundField DataField="FechaHora" HeaderText="Fecha/Hora" DataFormatString="{0:g}" />
            <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
            <asp:BoundField DataField="Accion" HeaderText="Acción" />
            <asp:BoundField DataField="EntidadAfectada" HeaderText="Entidad" />
            <asp:BoundField DataField="IdEntidadAfectada" HeaderText="ID" />
            <asp:BoundField DataField="CodigoCriticidad" HeaderText="Criticidad" />
            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
        </Columns>
    </asp:GridView>

    <div class="d-flex justify-content-between align-items-center">
        <asp:Button ID="btnAnterior" runat="server" CssClass="btn btn-outline-secondary btn-sm" Text="Anterior" OnClick="btnAnterior_Click" />
        <asp:Literal ID="litPagina" runat="server" />
        <asp:Button ID="btnSiguiente" runat="server" CssClass="btn btn-outline-secondary btn-sm" Text="Siguiente" OnClick="btnSiguiente_Click" />
    </div>
</asp:Content>
