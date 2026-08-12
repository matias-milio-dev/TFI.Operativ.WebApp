<%@ Page Codebehind="GestionSuscripcionesCliente.aspx.cs" Inherits="Operativ.Web.Paginas.GestionSuscripcionesCliente" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="card mb-4">
        <div class="card-body">
            <h2 class="h6">Nueva suscripción</h2>
            <asp:ValidationSummary ID="vsSuscripcion" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />
            <asp:Panel ID="pnlResumen" runat="server" Visible="false" CssClass="alert alert-info">
                <asp:Literal ID="litResumen" runat="server" />
            </asp:Panel>

            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Cliente</label>
                    <asp:DropDownList ID="ddlCliente" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCliente_SelectedIndexChanged" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Paquete</label>
                    <asp:DropDownList ID="ddlPaquete" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Meses</label>
                    <asp:TextBox ID="txtMeses" runat="server" CssClass="form-control" Text="1" />
                    <asp:RangeValidator ID="rvMeses" runat="server" ControlToValidate="txtMeses" Type="Integer"
                        MinimumValue="1" MaximumValue="60" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionFormatoInvalido %>" />
                </div>
            </div>

            <div class="mt-3">
                <asp:Button ID="btnGenerarResumen" runat="server" CssClass="btn btn-outline-primary" Text="Generar resumen" OnClick="btnGenerarResumen_Click" CausesValidation="true" />
                <asp:Button ID="btnConfirmarAlta" runat="server" CssClass="btn btn-primary" Text="Confirmar alta" OnClick="btnConfirmarAlta_Click" CausesValidation="true" />
            </div>
        </div>
    </div>

    <h2 class="h5">Suscripciones del cliente</h2>
    <asp:GridView ID="gvSuscripciones" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        DataKeyNames="IdSuscripcion" OnRowCommand="gvSuscripciones_RowCommand" EmptyDataText="Sin suscripciones para este cliente.">
        <Columns>
            <asp:BoundField DataField="NombrePaquete" HeaderText="Paquete" />
            <asp:BoundField DataField="CodigoEstado" HeaderText="Estado" />
            <asp:BoundField DataField="FechaInicio" HeaderText="Inicio" DataFormatString="{0:d}" />
            <asp:BoundField DataField="FechaVencimiento" HeaderText="Vencimiento" DataFormatString="{0:d}" />
            <asp:BoundField DataField="PrecioAcordado" HeaderText="Precio" DataFormatString="{0:C}" />
            <asp:BoundField DataField="EstrategiaAplicada" HeaderText="Estrategia de cotización" />
            <asp:ButtonField ButtonType="Link" CommandName="Pagar" Text="Registrar pago" />
            <asp:ButtonField ButtonType="Link" CommandName="Cancelar" Text="Cancelar" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlPago" runat="server" Visible="false" CssClass="card mt-3">
        <div class="card-body">
            <h2 class="h6">Registrar pago — suscripción #<asp:Literal ID="litIdSuscripcionPago" runat="server" /></h2>
            <asp:HiddenField ID="hdnIdSuscripcionPago" runat="server" />
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Medio de pago</label>
                    <asp:DropDownList ID="ddlMedioPago" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Tarjeta" Value="TARJETA" />
                        <asp:ListItem Text="Transferencia" Value="TRANSFERENCIA" />
                        <asp:ListItem Text="Efectivo" Value="EFECTIVO" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Monto</label>
                    <asp:TextBox ID="txtMontoPago" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-5">
                    <label class="form-label">Referencia (comprobante/pasarela)</label>
                    <asp:TextBox ID="txtReferenciaPago" runat="server" CssClass="form-control" MaxLength="100" />
                </div>
            </div>
            <div class="mt-3">
                <asp:Button ID="btnRegistrarPago" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonConfirmar %>" OnClick="btnRegistrarPago_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
