<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsultarBitacora.aspx.cs" Inherits="Operativ.Web.Paginas.ConsultarBitacora" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Enums" %>
<asp:Content ID="ContentConsultarBitacora" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2"></path><rect x="9" y="3" width="6" height="4" rx="2" ry="2"></rect><line x1="9" y1="12" x2="15" y2="12"></line><line x1="9" y1="16" x2="15" y2="16"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloConsultarBitacora">Consultar bitácora</h1>
                    <p runat="server" meta:resourcekey="DescripcionConsultarBitacora">Registro de actividad del sistema.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFechaDesde.ClientID %>"><asp:Literal ID="litEtiquetaFechaDesde" runat="server" Text="<%$ Resources:Textos, EtiquetaFechaDesde %>" /></label>
                    <asp:TextBox ID="txtFechaDesde" runat="server" TextMode="Date" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= txtFechaHasta.ClientID %>"><asp:Literal ID="litEtiquetaFechaHasta" runat="server" Text="<%$ Resources:Textos, EtiquetaFechaHasta %>" /></label>
                    <asp:TextBox ID="txtFechaHasta" runat="server" TextMode="Date" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= txtFiltroUsuario.ClientID %>"><asp:Literal ID="litEtiquetaUsuario" runat="server" Text="<%$ Resources:Textos, EtiquetaUsuarioBitacora %>" /></label>
                    <asp:TextBox ID="txtFiltroUsuario" runat="server" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= ddlFiltroAccion.ClientID %>"><asp:Literal ID="litEtiquetaAccion" runat="server" Text="<%$ Resources:Textos, EtiquetaAccionBitacora %>" /></label>
                    <asp:DropDownList ID="ddlFiltroAccion" runat="server" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= ddlFiltroCriticidad.ClientID %>"><asp:Literal ID="litEtiquetaCriticidad" runat="server" Text="<%$ Resources:Textos, EtiquetaCriticidadBitacora %>" /></label>
                    <asp:DropDownList ID="ddlFiltroCriticidad" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
        </div>

        <div class="tabla-contenedor">
        <asp:GridView ID="gvBitacora" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
            DataKeyNames="IdBitacora" GridLines="None">
            <Columns>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaBitacora %>">
                    <ItemTemplate>
                        <%# ((DateTime)Eval("FechaHora")).ToString("dd/MM/yyyy HH:mm") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaUsuarioBitacora %>">
                    <ItemTemplate>
                        <%# string.IsNullOrEmpty((string)Eval("NombreUsuario"))
                            ? (string)GetGlobalResourceObject("Textos", "EtiquetaSistemaBitacora")
                            : Eval("NombreUsuario") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaDescripcionBitacora %>">
                    <ItemTemplate>
                        <span class="descripcion-bitacora" title='<%# Eval("Descripcion") %>'><%# Eval("Descripcion") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaCriticidadBitacora %>">
                    <ItemTemplate>
                        <span class='badge <%# ObtenerClaseCriticidad((CriticidadBitacora)Eval("Criticidad")) %>'>
                            <%# ObtenerTextoCriticidad((CriticidadBitacora)Eval("Criticidad")) %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinRegistrosBitacora %>" />
            </EmptyDataTemplate>
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
</asp:Content>
