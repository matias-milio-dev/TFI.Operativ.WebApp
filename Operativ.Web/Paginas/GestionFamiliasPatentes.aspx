<%@ Page Codebehind="GestionFamiliasPatentes.aspx.cs" Inherits="Operativ.Web.Paginas.GestionFamiliasPatentes" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row">
        <div class="col-md-4">
            <div class="card mb-3">
                <div class="card-body">
                    <h2 class="h6">Nueva familia</h2>
                    <asp:ValidationSummary ID="vsFamilia" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />
                    <div class="mb-2">
                        <asp:TextBox ID="txtNombreFamilia" runat="server" CssClass="form-control" placeholder="Nombre" />
                        <asp:RequiredFieldValidator ID="rfvNombreFamilia" runat="server" ControlToValidate="txtNombreFamilia"
                            CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                    </div>
                    <div class="mb-2">
                        <asp:TextBox ID="txtDescripcionFamilia" runat="server" CssClass="form-control" placeholder="Descripción" TextMode="MultiLine" Rows="2" />
                    </div>
                    <asp:Button ID="btnCrearFamilia" runat="server" CssClass="btn btn-primary btn-sm" Text="<%$ Resources:Textos,BotonNuevo %>" OnClick="btnCrearFamilia_Click" />
                </div>
            </div>

            <div class="list-group">
                <asp:Repeater ID="rptFamilias" runat="server" OnItemCommand="rptFamilias_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnSeleccionar" runat="server" CssClass="list-group-item list-group-item-action"
                            CommandName="Seleccionar" CommandArgument='<%# Eval("IdFamilia") %>' Text='<%# Eval("Nombre") %>' />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="col-md-8">
            <asp:Panel ID="pnlPatentes" runat="server" Visible="false" CssClass="card">
                <div class="card-body">
                    <h2 class="h6"><asp:Literal ID="litFamiliaSeleccionada" runat="server" /></h2>
                    <p class="text-muted small">Patentes asignadas a esta familia (patrón Composite):</p>
                    <asp:CheckBoxList ID="cblPatentes" runat="server" />
                    <asp:HiddenField ID="hdnIdFamiliaSeleccionada" runat="server" />
                    <asp:Button ID="btnGuardarPatentes" runat="server" CssClass="btn btn-primary mt-2" Text="<%$ Resources:Textos,BotonGuardar %>" OnClick="btnGuardarPatentes_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
