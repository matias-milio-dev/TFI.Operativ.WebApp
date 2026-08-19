<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeWebMaster.aspx.cs" Inherits="Operativ.Web.Paginas.HomeWebMaster" MasterPageFile="~/Master/Principal.Master" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Paginas/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeWebMaster" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel de Web Master</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Mantenimiento técnico de la plataforma Operativ.</p>
    </div>
    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />
    <asp:Panel ID="pnlReparacionExitosa" runat="server" CssClass="modal-overlay activo" Visible="false">
        <div class="modal-caja">
            <div class="modal-encabezado">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"></path></svg>
                </span>
                <div>
                    <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloReparacionExitosa %>" /></h2>
                    <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionReparacionExitosa %>" /></p>
                </div>
            </div>
            <div class="modal-acciones">
                <asp:Button ID="btnAceptarReparacion" runat="server" CssClass="btn-primario" CausesValidation="false"
                    Text="<%$ Resources:Textos, BotonAceptar %>" OnClick="btnAceptarReparacion_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
