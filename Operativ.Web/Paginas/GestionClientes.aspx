<%@ Page Codebehind="GestionClientes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionClientes" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-2 align-items-end mb-3">
        <div class="col-auto">
            <asp:TextBox ID="txtFiltro" runat="server" CssClass="form-control" placeholder="Buscar por razón social o CUIT" />
        </div>
        <div class="col-auto">
            <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-outline-secondary" Text="<%$ Resources:Textos,BotonBuscar %>" OnClick="btnBuscar_Click" />
        </div>
        <div class="col-auto ms-auto">
            <asp:Button ID="btnNuevo" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonNuevo %>" OnClick="btnNuevo_Click" />
        </div>
    </div>

    <asp:GridView ID="gvClientes" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        DataKeyNames="IdCliente" OnRowCommand="gvClientes_RowCommand" EmptyDataText="Sin resultados.">
        <Columns>
            <asp:BoundField DataField="Cuit" HeaderText="CUIT" />
            <asp:BoundField DataField="RazonSocial" HeaderText="Razón social" />
            <asp:BoundField DataField="CorreoElectronico" HeaderText="Correo" />
            <asp:BoundField DataField="Telefono" HeaderText="Teléfono" />
            <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
            <asp:ButtonField ButtonType="Link" CommandName="Editar" Text="Editar" />
            <asp:ButtonField ButtonType="Link" CommandName="Baja" Text="Dar de baja" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlFormulario" runat="server" Visible="false" CssClass="card mt-4">
        <div class="card-body">
            <h2 class="h5">Datos del cliente</h2>
            <asp:HiddenField ID="hdnIdCliente" runat="server" Value="0" />
            <asp:ValidationSummary ID="vsCliente" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">CUIT</label>
                    <asp:TextBox ID="txtCuit" runat="server" CssClass="form-control" MaxLength="13" placeholder="20-12345678-9" />
                    <asp:RequiredFieldValidator ID="rfvCuit" runat="server" ControlToValidate="txtCuit" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                    <asp:RegularExpressionValidator ID="revCuit" runat="server" ControlToValidate="txtCuit"
                        ValidationExpression="^\d{2}-\d{8}-\d{1}$" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionFormatoCuit %>" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Razón social</label>
                    <asp:TextBox ID="txtRazonSocial" runat="server" CssClass="form-control" MaxLength="150" />
                    <asp:RequiredFieldValidator ID="rfvRazonSocial" runat="server" ControlToValidate="txtRazonSocial" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Correo electrónico</label>
                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" MaxLength="150" />
                    <asp:RequiredFieldValidator ID="rfvCorreo" runat="server" ControlToValidate="txtCorreo" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                    <asp:RegularExpressionValidator ID="revCorreo" runat="server" ControlToValidate="txtCorreo"
                        ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionFormatoCorreo %>" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Teléfono</label>
                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" MaxLength="30" />
                </div>
                <div class="col-md-8">
                    <label class="form-label">Dirección</label>
                    <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
            </div>

            <div class="mt-3">
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonGuardar %>" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-outline-secondary" Text="<%$ Resources:Textos,BotonCancelar %>" CausesValidation="false" OnClick="btnCancelar_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
