<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BackupRestore.aspx.cs" Inherits="Operativ.Web.Paginas.BackupRestore" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentBackupRestore" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><ellipse cx="12" cy="5" rx="9" ry="3"></ellipse><path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"></path><path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"></path></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloBackupRestore">Backup y restore de la base de datos</h1>
                    <p runat="server" meta:resourcekey="DescripcionBackupRestore">Generá un backup de la base bajo demanda o restaurala desde uno existente.</p>
                </div>
            </div>
            <asp:LinkButton ID="btnCrearBackup" runat="server" CssClass="btn-primario" CausesValidation="false" data-patente="RealizarBackup" OnClick="btnCrearBackup_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCrearBackup %>" />
            </asp:LinkButton>
        </div>

        <div class="tabla-contenedor">
        <asp:GridView ID="gvBackups" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
            DataKeyNames="NombreArchivo" OnRowCommand="gvBackups_RowCommand" GridLines="None">
            <Columns>
                <asp:BoundField DataField="NombreArchivo" HeaderText="<%$ Resources:Textos, EtiquetaArchivoBackup %>" />
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaBackup %>">
                    <ItemTemplate>
                        <%# ((DateTime)Eval("FechaCreacion")).ToString("dd/MM/yyyy HH:mm") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaTamanioBackup %>">
                    <ItemTemplate>
                        <%# ((long)Eval("TamanioBytes") / 1024) %> KB
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkDescargar" runat="server" CommandName="Descargar" CommandArgument='<%# Eval("NombreArchivo") %>'
                            CssClass="btn-outline" CausesValidation="false">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                            <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDescargarBackup %>" />
                        </asp:LinkButton>
                        <asp:LinkButton ID="lnkRestaurar" runat="server" CommandName="Restaurar" CommandArgument='<%# Eval("NombreArchivo") %>'
                            CssClass="btn-outline-peligro" CausesValidation="false" data-patente="RestaurarBackup"
                            OnClientClick='<%# "return confirm(\"¿Confirma que desea restaurar la base de datos desde " + Eval("NombreArchivo") + "? Esto reemplaza TODOS los datos actuales y no se puede deshacer.\");" %>'>
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="1 4 1 10 7 10"></polyline><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"></path></svg>
                            <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonRestaurarBackup %>" />
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinBackups %>" />
            </EmptyDataTemplate>
        </asp:GridView>
        </div>
    </div>

    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="17 8 12 3 7 8"></polyline><line x1="12" y1="3" x2="12" y2="15"></line></svg>
                </span>
                <div>
                    <h2 runat="server" meta:resourcekey="TituloRestaurarDesdeArchivo">Restaurar desde un archivo</h2>
                    <p runat="server" meta:resourcekey="DescripcionRestaurarDesdeArchivo">Subí un archivo .bak desde tu computadora para restaurar la base de datos.</p>
                </div>
            </div>
        </div>
        <div class="barra-busqueda">
            <div class="campo-formulario">
                <label for="<%= fileuploadRestaurar.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaArchivoRestaurar %>" /></label>
                <asp:FileUpload ID="fileuploadRestaurar" runat="server" />
            </div>
            <asp:LinkButton ID="btnRestaurarDesdeArchivo" runat="server" CssClass="btn-outline-peligro" CausesValidation="false" data-patente="RestaurarBackup"
                OnClick="btnRestaurarDesdeArchivo_Click"
                OnClientClick="return confirm('¿Confirma que desea restaurar la base de datos con el archivo seleccionado? Esto reemplaza TODOS los datos actuales y no se puede deshacer.');">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="1 4 1 10 7 10"></polyline><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"></path></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonRestaurarDesdeArchivo %>" />
            </asp:LinkButton>
        </div>
    </div>
</asp:Content>
