<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeWebMaster.aspx.cs" Inherits="Operativ.Web.Paginas.HomeWebMaster" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Modelos" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Paginas/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeWebMaster" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel de Web Master</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Mantenimiento técnico de la plataforma Operativ.</p>
    </div>
    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />
    <asp:Panel ID="pnlIntegridadCorrupta" runat="server" CssClass="modal-overlay activo" Visible="false">
        <div class="modal-caja">
            <div class="modal-encabezado">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>
                </span>
                <div>
                    <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloIntegridadCorrupta %>" /></h2>
                    <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionIntegridadCorrupta %>" /></p>
                </div>
            </div>

            <div class="detalle-integridad">
                <asp:Repeater ID="rptFallasIntegridad" runat="server">
                    <ItemTemplate>
                        <div class="detalle-integridad-tabla">
                            <span class="detalle-integridad-nombre"><%# Eval("NombreTabla") %></span>
                            <span class="detalle-integridad-filas"><%# ObtenerDetalleFalla((ResultadoVerificacionTabla)Container.DataItem) %></span>
                            <span class="detalle-integridad-dvv"><%# ObtenerDetalleDvv((ResultadoVerificacionTabla)Container.DataItem) %></span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="modal-acciones">
                <asp:Button ID="btnRecalcular" runat="server" CssClass="btn-primario" CausesValidation="false"
                    Text="<%$ Resources:Textos, BotonRecalcularDigitos %>" OnClick="btnRecalcular_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
