<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GestionUsuarios.aspx.cs" Inherits="Operativ.Web.Paginas.GestionUsuarios" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentGestionUsuarios" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                        <circle cx="9" cy="7" r="4"></circle>
                        <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                        <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                    </svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloGestionUsuarios">Gestión de usuarios</h1>
                    <p runat="server" meta:resourcekey="DescripcionGestionUsuarios">Alta, baja y modificación de usuarios de la plataforma Operativ.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <div class="campo-formulario">
                <label for="<%= txtFiltro.ClientID %>"><asp:Literal ID="litEtiquetaFiltro" runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroUsuarios %>" /></label>
                <asp:TextBox ID="txtFiltro" runat="server" />
            </div>
            <div class="campo-formulario">
                <label for="<%= ddlFiltroFamilia.ClientID %>"><asp:Literal ID="litEtiquetaFiltroFamilia" runat="server" Text="<%$ Resources:Textos, EtiquetaFamilia %>" /></label>
                <asp:DropDownList ID="ddlFiltroFamilia" runat="server" />
            </div>
            <asp:LinkButton ID="btnNuevoUsuario" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnNuevoUsuario_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="8.5" cy="7" r="4"></circle><line x1="20" y1="8" x2="20" y2="14"></line><line x1="23" y1="11" x2="17" y2="11"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonNuevoUsuario %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
            </asp:LinkButton>
        </div>

        <div class="tabla-contenedor">
        <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
            DataKeyNames="IdUsuario" OnRowCommand="gvUsuarios_RowCommand" GridLines="None">
            <Columns>
                <asp:BoundField DataField="NombreUsuario" HeaderText="<%$ Resources:Textos, EtiquetaNombreUsuario %>" />
                <asp:BoundField DataField="NombreCompleto" HeaderText="<%$ Resources:Textos, EtiquetaNombreCompleto %>" />
                <asp:BoundField DataField="Email" HeaderText="<%$ Resources:Textos, EtiquetaCorreoElectronico %>" />
                <asp:BoundField DataField="NombreFamilia" HeaderText="<%$ Resources:Textos, EtiquetaFamilia %>" />
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaEstado %>">
                    <ItemTemplate>
                        <span class='badge <%# (bool)Eval("Bloqueado") ? "badge-bloqueado" : "badge-activo" %>'>
                            <%# (bool)Eval("Bloqueado")
                                ? (string)GetGlobalResourceObject("Textos", "EstadoBloqueado")
                                : (string)GetGlobalResourceObject("Textos", "EstadoActivo") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <div class="acciones-fila">
                            <asp:LinkButton ID="lnkEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IdUsuario") %>'
                                CssClass="btn-outline" CausesValidation="false">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"></path></svg>
                                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonEditar %>" />
                            </asp:LinkButton>
                            <asp:LinkButton ID="lnkBaja" runat="server" CommandName="Baja" CommandArgument='<%# Eval("IdUsuario") %>'
                                CssClass="btn-outline-peligro" CausesValidation="false"
                                OnClientClick="return confirm('¿Confirma que desea dar de baja al usuario?');">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDarBaja %>" />
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </div>

        <div class="paginado">
            <asp:Literal ID="litResumenPaginado" runat="server" />
            <div class="paginado-controles">
                <asp:Button ID="btnPaginaAnterior" runat="server" Text="<%$ Resources:Textos, BotonPaginaAnterior %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnPaginaAnterior_Click" />
                <span class="paginado-numero"><asp:Literal ID="litNumeroPagina" runat="server" /></span>
                <asp:Button ID="btnPaginaSiguiente" runat="server" Text="<%$ Resources:Textos, BotonPaginaSiguiente %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnPaginaSiguiente_Click" />
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlFormularioUsuario" runat="server" CssClass="tarjeta">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="8.5" cy="7" r="4"></circle>
                    <line x1="20" y1="8" x2="20" y2="14"></line>
                    <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
            </span>
            <h2 id="tituloFormulario" runat="server"></h2>
        </div>
        <asp:HiddenField ID="hidIdUsuario" runat="server" Value="0" />

        <asp:Panel ID="pnlDesbloqueo" runat="server" Visible="false">
            <p><asp:Literal ID="litMensajeBloqueado" runat="server" /></p>
            <div class="acciones-formulario">
                <asp:LinkButton ID="btnDesbloquear" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnDesbloquear_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 9.9-1"></path></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDesbloquearUsuario %>" />
                </asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCamposEdicion" runat="server">
            <div class="fila-formulario">
                <div class="campo-formulario">
                    <label for="<%= txtNombreUsuarioAlta.ClientID %>"><asp:Literal ID="litEtiquetaUsuario" runat="server" Text="<%$ Resources:Textos, EtiquetaNombreUsuario %>" /></label>
                    <asp:TextBox ID="txtNombreUsuarioAlta" runat="server" autocomplete="off" />
                    <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuarioAlta"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionUsuarioObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>

                <div class="campo-formulario">
                    <label for="<%= txtNombreCompleto.ClientID %>"><asp:Literal ID="litEtiquetaNombreCompleto" runat="server" Text="<%$ Resources:Textos, EtiquetaNombreCompleto %>" /></label>
                    <asp:TextBox ID="txtNombreCompleto" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvNombreCompleto" runat="server" ControlToValidate="txtNombreCompleto"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionNombreCompletoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>

                <div class="campo-formulario">
                    <label for="<%= txtEmail.ClientID %>"><asp:Literal ID="litEtiquetaEmail" runat="server" Text="<%$ Resources:Textos, EtiquetaCorreoElectronico %>" /></label>
                    <asp:TextBox ID="txtEmail" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        ValidationExpression="^[\w\.\-]+@[\w\-]+\.[\w\.\-]+$"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoFormato %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>

                <div class="campo-formulario">
                    <label for="<%= ddlFamilia.ClientID %>"><asp:Literal ID="litEtiquetaFamilia" runat="server" Text="<%$ Resources:Textos, EtiquetaFamilia %>" /></label>
                    <asp:DropDownList ID="ddlFamilia" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvFamilia" runat="server" ControlToValidate="ddlFamilia" InitialValue=""
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionFamiliaObligatoria %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>
            </div>

            <div class="acciones-formulario">
                <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" OnClick="btnGuardar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
                </asp:LinkButton>
            </div>
        </asp:Panel>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnCancelar" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCancelar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </asp:LinkButton>
        </div>
        <asp:ValidationSummary ID="vsGestionUsuarios" runat="server" CssClass="texto-validacion" />
    </asp:Panel>
</asp:Content>
