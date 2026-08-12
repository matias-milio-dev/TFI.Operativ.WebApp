<%@ Page Codebehind="GestionUsuarios.aspx.cs" Inherits="Operativ.Web.Paginas.GestionUsuarios" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4"><asp:Literal ID="litTitulo" runat="server" /></h1>

    <div class="row g-2 align-items-end mb-3">
        <div class="col-auto">
            <asp:TextBox ID="txtFiltro" runat="server" CssClass="form-control" placeholder="Buscar por usuario o nombre" />
        </div>
        <div class="col-auto">
            <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-outline-secondary" Text="<%$ Resources:Textos,BotonBuscar %>" OnClick="btnBuscar_Click" />
        </div>
        <div class="col-auto ms-auto">
            <asp:Button ID="btnNuevo" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonNuevo %>" OnClick="btnNuevo_Click" />
        </div>
    </div>

    <asp:GridView ID="gvUsuarios" runat="server" CssClass="table table-striped table-hover tabla-operativ" AutoGenerateColumns="false"
        DataKeyNames="IdUsuario" OnRowCommand="gvUsuarios_RowCommand" EmptyDataText="Sin resultados.">
        <Columns>
            <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
            <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre" />
            <asp:BoundField DataField="CorreoElectronico" HeaderText="Correo" />
            <asp:BoundField DataField="NombrePerfil" HeaderText="Perfil" />
            <asp:CheckBoxField DataField="Bloqueado" HeaderText="Bloqueado" />
            <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
            <asp:ButtonField ButtonType="Link" CommandName="Editar" Text="Editar" />
            <asp:ButtonField ButtonType="Link" CommandName="Baja" Text="Dar de baja" />
            <asp:ButtonField ButtonType="Link" CommandName="Desbloquear" Text="Desbloquear" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlFormulario" runat="server" Visible="false" CssClass="card mt-4">
        <div class="card-body">
            <h2 class="h5" ><asp:Literal ID="litSubtitulo" runat="server" Text="Alta de usuario" /></h2>
            <asp:HiddenField ID="hdnIdUsuario" runat="server" Value="0" />
            <asp:ValidationSummary ID="vsUsuario" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-md-4">
                    <asp:Label ID="lblNombreUsuario" runat="server" Text="Nombre de usuario" CssClass="form-label" AssociatedControlID="txtNombreUsuario" />
                    <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50" />
                    <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblNombreCompleto" runat="server" Text="Nombre completo" CssClass="form-label" AssociatedControlID="txtNombreCompleto" />
                    <asp:TextBox ID="txtNombreCompleto" runat="server" CssClass="form-control" MaxLength="150" />
                    <asp:RequiredFieldValidator ID="rfvNombreCompleto" runat="server" ControlToValidate="txtNombreCompleto" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblCorreo" runat="server" Text="Correo electrónico" CssClass="form-label" AssociatedControlID="txtCorreo" />
                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" MaxLength="150" />
                    <asp:RequiredFieldValidator ID="rfvCorreo" runat="server" ControlToValidate="txtCorreo" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionCampoObligatorio %>" />
                    <asp:RegularExpressionValidator ID="revCorreo" runat="server" ControlToValidate="txtCorreo"
                        ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" CssClass="text-danger small" Display="Dynamic" ErrorMessage="<%$ Resources:Textos,ValidacionFormatoCorreo %>" />
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblPerfil" runat="server" Text="Perfil" CssClass="form-label" AssociatedControlID="ddlPerfil" />
                    <asp:DropDownList ID="ddlPerfil" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Web Master" Value="WEBMASTER" />
                        <asp:ListItem Text="Administrador" Value="ADMINISTRADOR" />
                        <asp:ListItem Text="Comercial" Value="COMERCIAL" />
                        <asp:ListItem Text="Cliente" Value="CLIENTE" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblIdioma" runat="server" Text="Idioma preferido" CssClass="form-label" AssociatedControlID="ddlIdioma" />
                    <asp:DropDownList ID="ddlIdioma" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Español" Value="es" />
                        <asp:ListItem Text="English" Value="en" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="mt-3">
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" Text="<%$ Resources:Textos,BotonGuardar %>" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-outline-secondary" Text="<%$ Resources:Textos,BotonCancelar %>" CausesValidation="false" OnClick="btnCancelar_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
