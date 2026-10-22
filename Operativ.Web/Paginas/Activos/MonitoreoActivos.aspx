<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MonitoreoActivos.aspx.cs" Inherits="Operativ.Web.Paginas.MonitoreoActivos" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Enums" %>
<%@ Register TagPrefix="uc" TagName="Paginador" Src="~/Paginas/Controles/Paginador.ascx" %>
<asp:Content ID="ContentMonitoreoActivos" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <asp:Panel ID="pnlMonitoreo" runat="server">
        <div class="tarjeta encabezado-monitoreo">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo icono-circulo-grande">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="2" y1="21" x2="22" y2="21"></line></svg>
                </span>
                <div>
                    <p class="eyebrow"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEyebrowMonitoreo %>" /></p>
                    <h1><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloMonitoreoActivos %>" /></h1>
                    <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionMonitoreoActivos %>" /></p>
                </div>
            </div>
        </div>

        <div class="tarjeta">
            <div class="barra-busqueda" style="margin: 0;">
                <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                    <div class="campo-formulario campo-con-lupa">
                        <label for="<%= txtFiltro.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroActivos %>" /></label>
                        <svg class="icono-campo" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                        <asp:TextBox ID="txtFiltro" runat="server" MaxLength="100" />
                    </div>
                    <div class="campo-formulario">
                        <label for="<%= ddlFiltroEstado.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaEstadoActivo %>" /></label>
                        <asp:DropDownList ID="ddlFiltroEstado" runat="server" />
                    </div>
                    <div class="campo-formulario">
                        <label for="<%= ddlFiltroPaquete.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroPaqueteActivo %>" /></label>
                        <asp:DropDownList ID="ddlFiltroPaquete" runat="server" />
                    </div>
                    <asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnFiltrar_Click">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"></polygon></svg>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonFiltrar %>" />
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnLimpiar" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnLimpiar_Click">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonLimpiarFiltros %>" />
                    </asp:LinkButton>
                </asp:Panel>
            </div>
        </div>

        <div class="tarjeta franja-resumen">
            <span class="icono-circulo icono-circulo-grande">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="2" y1="21" x2="22" y2="21"></line></svg>
            </span>
            <div class="franja-resumen-item">
                <span class="franja-resumen-valor"><asp:Literal ID="litTotal" runat="server" /></span>
                <span class="franja-resumen-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaResumenActivosTotales %>" /></span>
            </div>
            <div class="franja-resumen-item">
                <span class="franja-resumen-valor"><i class="punto-estado punto-disponible"></i><asp:Literal ID="litDisponibles" runat="server" /></span>
                <span class="franja-resumen-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaResumenDisponibles %>" /></span>
            </div>
            <div class="franja-resumen-item">
                <span class="franja-resumen-valor"><i class="punto-estado punto-asignado"></i><asp:Literal ID="litAsignados" runat="server" /></span>
                <span class="franja-resumen-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaResumenAsignados %>" /></span>
            </div>
            <div class="franja-resumen-item">
                <span class="franja-resumen-valor"><i class="punto-estado punto-reparacion"></i><asp:Literal ID="litEnReparacion" runat="server" /></span>
                <span class="franja-resumen-etiqueta"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaResumenEnReparacion %>" /></span>
            </div>
        </div>

        <div class="tarjeta">
            <div class="encabezado-grilla">
                <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloGrillaMisActivos %>" /></h2>
                <uc:Paginador ID="ucPaginador" runat="server" ModoNumerado="true" ClaveResumen="MensajeResumenPaginadoActivos" OnPaginaCambiada="ucPaginador_PaginaCambiada" />
            </div>

            <div class="tabla-contenedor">
                <asp:GridView ID="gvActivos" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderStyle-CssClass="columna-icono" ItemStyle-CssClass="columna-icono">
                            <ItemTemplate>
                                <svg class="icono-fila" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="2" y1="21" x2="22" y2="21"></line></svg>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Nombre" HeaderText="<%$ Resources:Textos, EtiquetaNombreActivo %>" />
                        <asp:BoundField DataField="Modelo" HeaderText="<%$ Resources:Textos, EtiquetaModeloActivo %>" />
                        <asp:BoundField DataField="NumeroSerie" HeaderText="<%$ Resources:Textos, EtiquetaNumeroSerie %>" />
                        <asp:BoundField DataField="NombrePaquete" HeaderText="<%$ Resources:Textos, EtiquetaPaqueteActivo %>" />
                        <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaEstadoActivo %>">
                            <ItemTemplate>
                                <span class='badge badge-con-punto <%# ObtenerClaseEstado((EstadoActivo)Eval("Estado")) %>'>
                                    <i class="punto-estado"></i>
                                    <%# ObtenerTextoEstado((EstadoActivo)Eval("Estado")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinActivos %>" />
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <div class="tarjeta tarjeta-acceso-rapido">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="8" y1="13" x2="16" y2="13"></line><line x1="8" y1="17" x2="13" y2="17"></line></svg>
            </span>
            <div class="tarjeta-acceso-rapido-texto">
                <h3><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloAccesoRapidoIncidentes %>" /></h3>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionAccesoRapidoIncidentes %>" /></p>
            </div>
            <asp:HyperLink ID="lnkIrAIncidentes" runat="server" CssClass="btn-outline" NavigateUrl="~/Paginas/Incidentes/GestionIncidentes.aspx" Text="<%$ Resources:Textos, BotonIrAIncidentes %>" />
        </div>
    </asp:Panel>
</asp:Content>
