<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsultaSuscripciones.aspx.cs" Inherits="Operativ.Web.Paginas.ConsultaSuscripciones" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Enums" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentConsultaSuscripciones" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect><line x1="1" y1="10" x2="23" y2="10"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloConsultaSuscripciones">Suscripciones</h1>
                    <p runat="server" meta:resourcekey="DescripcionConsultaSuscripciones">Suscripciones de todas las empresas cliente.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroSuscripciones %>" /></label>
                    <asp:TextBox ID="txtFiltro" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
        </div>

        <div class="tabla-contenedor">
            <asp:GridView ID="gvSuscripciones" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
                DataKeyNames="IdSuscripcion" GridLines="None">
                <Columns>
                    <asp:BoundField DataField="RazonSocialCliente" HeaderText="<%$ Resources:Textos, EtiquetaEmpresaCliente %>" />
                    <asp:BoundField DataField="CuitCliente" HeaderText="<%$ Resources:Textos, EtiquetaCuit %>" />
                    <asp:BoundField DataField="NombrePlan" HeaderText="<%$ Resources:Textos, EtiquetaPlanSuscripcion %>" />
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaPrecioAnual %>">
                        <ItemTemplate>
                            USD <%# ((decimal)Eval("PrecioAnual")).ToString("N2") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaEstadoSuscripcion %>">
                        <ItemTemplate>
                            <span class='badge <%# ObtenerClaseEstadoSuscripcion((EstadoSuscripcion)Eval("Estado")) %>'>
                                <%# ObtenerTextoEstadoSuscripcion((EstadoSuscripcion)Eval("Estado")) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaAltaSuscripcion %>">
                        <ItemTemplate>
                            <%# ((DateTime)Eval("FechaAlta")).ToString("dd/MM/yyyy") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaVencimientoSuscripcion %>">
                        <ItemTemplate>
                            <%# ObtenerTextoFecha((DateTime?)Eval("FechaVencimiento")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaComprobantePago %>">
                        <ItemTemplate>
                            <%# ObtenerTextoComprobante((string)Eval("CodigoComprobante")) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinSuscripciones %>" />
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <uc:Paginador ID="ucPaginador" runat="server" ClaveResumen="MensajeResumenPaginadoSuscripciones" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
    </div>
</asp:Content>
