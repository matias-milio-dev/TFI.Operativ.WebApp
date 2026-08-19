<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ModalCambiarClave.ascx.cs" Inherits="Operativ.Web.Controles.ModalCambiarClave" %>
<div id="modalCambiarClave" class="modal-overlay" onclick="Operativ.clicOverlayModal(event)">
    <div class="modal-caja">
        <button type="button" class="modal-cerrar" onclick="Operativ.cerrarModalCambiarClave(event)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
        </button>

        <div class="modal-encabezado">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
            </span>
            <div>
                <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloCambiarContrasena %>" /></h2>
                <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionCambiarContrasena %>" /></p>
            </div>
        </div>

        <div class="campo-formulario">
            <label for="<%= txtContrasenaActual.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaContrasenaActual %>" /></label>
            <div class="campo-contrasena-envoltorio">
                <asp:TextBox ID="txtContrasenaActual" runat="server" TextMode="Password" CssClass="campo-contrasena" />
                <button type="button" class="boton-mostrar-contrasena" onclick="Operativ.alternarVisibilidadContrasena(this)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                </button>
            </div>
            <asp:RequiredFieldValidator ID="rfvContrasenaActual" runat="server" ControlToValidate="txtContrasenaActual"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionContrasenaActualObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="CambiarClave" />
        </div>

        <div class="campo-formulario">
            <label for="<%= txtContrasenaNueva.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaContrasenaNueva %>" /></label>
            <div class="campo-contrasena-envoltorio">
                <asp:TextBox ID="txtContrasenaNueva" runat="server" TextMode="Password" CssClass="campo-contrasena" />
                <button type="button" class="boton-mostrar-contrasena" onclick="Operativ.alternarVisibilidadContrasena(this)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                </button>
            </div>
            <asp:RequiredFieldValidator ID="rfvContrasenaNueva" runat="server" ControlToValidate="txtContrasenaNueva"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionContrasenaNuevaObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="CambiarClave" />
            <asp:RegularExpressionValidator ID="revContrasenaNueva" runat="server" ControlToValidate="txtContrasenaNueva"
                ValidationExpression="^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionContrasenaNuevaFormato %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="CambiarClave" />
        </div>

        <div class="campo-formulario">
            <label for="<%= txtContrasenaConfirmar.ClientID %>"><asp:Literal runat="server" Text="<%$ Resources:Textos, EtiquetaContrasenaConfirmar %>" /></label>
            <div class="campo-contrasena-envoltorio">
                <asp:TextBox ID="txtContrasenaConfirmar" runat="server" TextMode="Password" CssClass="campo-contrasena" />
                <button type="button" class="boton-mostrar-contrasena" onclick="Operativ.alternarVisibilidadContrasena(this)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                </button>
            </div>
            <asp:RequiredFieldValidator ID="rfvContrasenaConfirmar" runat="server" ControlToValidate="txtContrasenaConfirmar"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionContrasenaConfirmarObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="CambiarClave" />
            <asp:CompareValidator ID="cvContrasenaConfirmar" runat="server" ControlToValidate="txtContrasenaConfirmar" ControlToCompare="txtContrasenaNueva"
                ErrorMessage="<%$ Resources:Textos, MensajeValidacionContrasenaConfirmarCoincide %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="CambiarClave" />
        </div>

        <div class="caja-info-requisitos">
            <div class="caja-info-requisitos-titulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, TituloRequisitosContrasena %>" />
            </div>
            <ul>
                <li><asp:Literal runat="server" Text="<%$ Resources:Textos, RequisitoContrasenaLongitud %>" /></li>
                <li><asp:Literal runat="server" Text="<%$ Resources:Textos, RequisitoContrasenaMayuscula %>" /></li>
                <li><asp:Literal runat="server" Text="<%$ Resources:Textos, RequisitoContrasenaMinuscula %>" /></li>
                <li><asp:Literal runat="server" Text="<%$ Resources:Textos, RequisitoContrasenaNumero %>" /></li>
            </ul>
        </div>

        <asp:ValidationSummary ID="vsCambiarClave" runat="server" CssClass="texto-validacion" ValidationGroup="CambiarClave" />

        <div class="modal-acciones">
            <button type="button" class="btn-outline" onclick="Operativ.cerrarModalCambiarClave(event)">
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </button>
            <asp:LinkButton ID="btnGuardarClave" runat="server" CssClass="btn-primario" ValidationGroup="CambiarClave" OnClick="btnGuardarClave_Click">
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
        </div>
    </div>
</div>
