<%@ Page Codebehind="GestionPaquetes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionPaquetes" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="mb-3 text-end">
        <asp:Button ID="btnNuevo" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonNuevo %>" OnClick="btnNuevo_Click" />
    </div>

    <asp:GridView ID="gvPaquetes" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        DataKeyNames="IdPaquete" OnRowCommand="gvPaquetes_RowCommand" EmptyDataText="Sin resultados.">
        <Columns>
            <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
            <asp:BoundField DataField="PrecioBase" HeaderText="Precio base" DataFormatString="{0:C}" />
            <asp:BoundField DataField="CantidadActivosIncluidos" HeaderText="Activos incluidos" />
            <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
            <asp:ButtonField ButtonType="Link" CommandName="Editar" Text="Editar" />
            <asp:ButtonField ButtonType="Link" CommandName="Baja" Text="Dar de baja" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlFormulario" runat="server" Visible="false" CssClass="card mt-4">
        <div class="card-body">
            <h2 class="h5">Datos del paquete</h2>
            <asp:HiddenField ID="hdnIdPaquete" runat="server" Value="0" />
            <asp:ValidationSummary ID="vsPaquete" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Nombre</label>
                    <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Precio base</label>
                    <asp:TextBox ID="txtPrecioBase" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvPrecio" runat="server" ControlToValidate="txtPrecioBase" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                    <asp:RangeValidator ID="rvPrecio" runat="server" ControlToValidate="txtPrecioBase" Type="Currency"
                        MinimumValue="0.01" MaximumValue="999999999" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionFormatoInvalido %>" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Cantidad de activos incluidos</label>
                    <asp:TextBox ID="txtCantidadActivos" runat="server" CssClass="form-control" />
                    <asp:RangeValidator ID="rvCantidadActivos" runat="server" ControlToValidate="txtCantidadActivos" Type="Integer"
                        MinimumValue="0" MaximumValue="100000" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionFormatoInvalido %>" />
                </div>
                <div class="col-md-12">
                    <label class="form-label">Descripción</label>
                    <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" MaxLength="400" />
                </div>
            </div>

            <div class="mt-3">
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonGuardar %>" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-outline-secondary" Text="<%$ Resources:Textos,BotonCancelar %>" CausesValidation="false" OnClick="btnCancelar_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
