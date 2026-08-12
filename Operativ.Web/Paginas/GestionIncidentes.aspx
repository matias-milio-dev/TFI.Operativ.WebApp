<%@ Page Codebehind="GestionIncidentes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionIncidentes" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-2 align-items-end mb-3">
        <div class="col-auto">
            <label class="form-label small mb-0">Cliente</label>
            <asp:DropDownList ID="ddlCliente" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCliente_SelectedIndexChanged" />
        </div>
        <div class="col-auto">
            <label class="form-label small mb-0">Activo</label>
            <asp:DropDownList ID="ddlActivo" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlActivo_SelectedIndexChanged" />
        </div>
    </div>

    <asp:GridView ID="gvIncidentes" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        DataKeyNames="IdIncidente" OnRowCommand="gvIncidentes_RowCommand" EmptyDataText="Sin incidentes para este activo.">
        <Columns>
            <asp:BoundField DataField="CodigoCategoria" HeaderText="Categoría" />
            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
            <asp:BoundField DataField="Prioridad" HeaderText="Prioridad" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:BoundField DataField="FechaAlta" HeaderText="Fecha" DataFormatString="{0:g}" />
            <asp:ButtonField ButtonType="Link" CommandName="Cerrar" Text="Cerrar" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlFormulario" runat="server" Visible="false" CssClass="card mt-4">
        <div class="card-body">
            <h2 class="h6">Nuevo incidente</h2>
            <asp:ValidationSummary ID="vsIncidente" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />
            <div class="row g-3">
                <div class="col-md-3">
                    <label class="form-label">Categoría</label>
                    <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Hardware" Value="HARDWARE" />
                        <asp:ListItem Text="Software" Value="SOFTWARE" />
                        <asp:ListItem Text="Red" Value="RED" />
                        <asp:ListItem Text="Seguridad" Value="SEGURIDAD" />
                        <asp:ListItem Text="Otro" Value="OTRO" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Prioridad</label>
                    <asp:DropDownList ID="ddlPrioridad" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Baja" Value="BAJA" />
                        <asp:ListItem Text="Media" Value="MEDIA" Selected="True" />
                        <asp:ListItem Text="Alta" Value="ALTA" />
                        <asp:ListItem Text="Urgente" Value="URGENTE" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <label class="form-label">Descripción</label>
                    <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" MaxLength="500" />
                    <asp:RequiredFieldValidator ID="rfvDescripcion" runat="server" ControlToValidate="txtDescripcion" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                </div>
            </div>
            <div class="mt-3">
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonGuardar %>" OnClick="btnGuardar_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
