<%@ Page Codebehind="GestionActivos.aspx.cs" Inherits="Operativ.Web.Paginas.GestionActivos" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-2 align-items-end mb-3">
        <div class="col-auto">
            <label class="form-label small mb-0">Cliente</label>
            <asp:DropDownList ID="ddlCliente" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCliente_SelectedIndexChanged" />
        </div>
    </div>

    <asp:GridView ID="gvActivos" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        DataKeyNames="IdActivo" OnRowCommand="gvActivos_RowCommand" EmptyDataText="Este cliente no tiene activos.">
        <Columns>
            <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
            <asp:BoundField DataField="TipoActivo" HeaderText="Tipo" />
            <asp:BoundField DataField="Identificador" HeaderText="Identificador" />
            <asp:CheckBoxField DataField="EstaActivo" HeaderText="Activo" />
            <asp:ButtonField ButtonType="Link" CommandName="Baja" Text="Dar de baja" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="card mt-4">
        <div class="card-body">
            <h2 class="h6">Alta de activo</h2>
            <p class="text-muted small">Requiere una suscripción activa del cliente (#ERR17).</p>
            <asp:ValidationSummary ID="vsActivo" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />
            <div class="row g-3">
                <div class="col-md-3">
                    <label class="form-label">Suscripción activa</label>
                    <asp:DropDownList ID="ddlSuscripcion" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Nombre</label>
                    <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Tipo</label>
                    <asp:DropDownList ID="ddlTipoActivo" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Servidor" Value="SERVIDOR" />
                        <asp:ListItem Text="PC" Value="PC" />
                        <asp:ListItem Text="Red" Value="RED" />
                        <asp:ListItem Text="Aplicación" Value="APLICACION" />
                        <asp:ListItem Text="Otro" Value="OTRO" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Identificador</label>
                    <asp:TextBox ID="txtIdentificador" runat="server" CssClass="form-control" MaxLength="100" placeholder="Serie / hostname / IP" />
                </div>
            </div>
            <div class="mt-3">
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonGuardar %>" OnClick="btnGuardar_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
