<%@ Page Codebehind="Administracion.aspx.cs" Inherits="Operativ.Web.Paginas.Administracion" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-4">
        <div class="col-md-6">
            <div class="card">
                <div class="card-body">
                    <h2 class="h6">Integridad de base de datos (DVH/DVV)</h2>
                    <p class="text-muted small">CU-001-002 Reparar Base de Datos.</p>
                    <asp:Button ID="btnVerificar" runat="server" CssClass="btn btn-outline-primary btn-sm" Text="<%$ Resources:Textos,BotonVerificar %>" OnClick="btnVerificar_Click" />
                    <asp:Button ID="btnReparar" runat="server" CssClass="btn btn-primary btn-sm" Text="<%$ Resources:Textos,BotonReparar %>" OnClick="btnReparar_Click" />

                    <asp:GridView ID="gvIntegridad" runat="server" CssClass="table table-sm mt-3" AutoGenerateColumns="false" EmptyDataText="Sin datos aún.">
                        <Columns>
                            <asp:BoundField DataField="NombreTabla" HeaderText="Tabla" />
                            <asp:BoundField DataField="Integro" HeaderText="Íntegra" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="card">
                <div class="card-body">
                    <h2 class="h6">Backup / Restore</h2>
                    <div class="mb-2">
                        <label class="form-label small">Ruta del archivo .bak</label>
                        <asp:TextBox ID="txtRutaBackup" runat="server" CssClass="form-control" placeholder="C:\Backups\Operativ.bak" />
                    </div>
                    <asp:Button ID="btnBackup" runat="server" CssClass="btn btn-outline-primary btn-sm" Text="<%$ Resources:Textos,BotonBackup %>" OnClick="btnBackup_Click" />
                    <asp:Button ID="btnRestore" runat="server" CssClass="btn btn-danger btn-sm" Text="<%$ Resources:Textos,BotonRestore %>" OnClick="btnRestore_Click"
                        OnClientClick="return confirm('Esta acción reemplaza la base de datos actual. ¿Confirma?');" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
