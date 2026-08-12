<%@ Page Codebehind="Servicios.aspx.cs" Inherits="Operativ.Web.Paginas.Servicios" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-2 align-items-end mb-3">
        <div class="col-auto">
            <asp:TextBox ID="txtFiltro" runat="server" CssClass="form-control" placeholder="Buscar paquete por nombre" />
        </div>
        <div class="col-auto">
            <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonBuscar %>" OnClick="btnBuscar_Click" />
        </div>
    </div>

    <asp:GridView ID="gvCatalogo" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        EmptyDataText="Sin resultados.">
        <Columns>
            <asp:BoundField DataField="Nombre" HeaderText="Paquete" />
            <asp:BoundField DataField="PrecioBase" HeaderText="Precio" DataFormatString="{0:C}" />
            <asp:BoundField DataField="CantidadActivosIncluidos" HeaderText="Activos incluidos" />
        </Columns>
    </asp:GridView>
    <p class="text-muted small">Este listado se genera a través de CatalogoService (Operativ.WebServices), que persiste catalogo_paquetes.xml.</p>
</asp:Content>
