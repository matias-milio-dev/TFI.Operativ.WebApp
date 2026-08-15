<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecuperarContrasena.aspx.cs" Inherits="Operativ.Web.RecuperarContrasena" %>
<%@ Register TagPrefix="uc" TagName="Notificaciones" Src="~/Controles/Notificaciones.ascx" %>
<%@ Register TagPrefix="uc" TagName="SelectorIdioma" Src="~/Controles/SelectorIdioma.ascx" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title runat="server" meta:resourcekey="TituloPagina">Operativ - Recuperar contraseña</title>
    <link runat="server" rel="icon" type="image/x-icon" href="~/favicon.ico" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&amp;display=swap" rel="stylesheet" />
    <link runat="server" rel="stylesheet" type="text/css" href="~/Estilos/operativ.css" />
</head>
<body class="pagina-login">
    <form id="formRecuperar" runat="server">
        <div class="selector-idioma-flotante">
            <uc:SelectorIdioma ID="ucSelectorIdioma" runat="server" />
        </div>
        <uc:Notificaciones ID="ucNotificaciones" runat="server" />
        <div class="caja-login">
            <h1 runat="server" meta:resourcekey="TituloRecuperarContrasena">Recuperar contraseña</h1>
            <div class="campo-formulario">
                <label for="<%= txtNombreUsuario.ClientID %>"><asp:Literal ID="litEtiquetaUsuario" runat="server" Text="<%$ Resources:Textos, EtiquetaNombreUsuario %>" /></label>
                <asp:TextBox ID="txtNombreUsuario" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionUsuarioObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
            </div>
            <asp:Button ID="btnEnviar" runat="server" Text="<%$ Resources:Textos, BotonEnviarContrasenaTemporal %>" CssClass="boton-principal" OnClick="btnEnviar_Click" />
            <asp:HyperLink ID="lnkVolver" runat="server" NavigateUrl="~/Login.aspx" CssClass="enlace-secundario" Text="<%$ Resources:Textos, EnlaceVolverIniciarSesion %>" />
            <asp:ValidationSummary ID="vsRecuperar" runat="server" CssClass="texto-validacion" />
        </div>
    </form>
</body>
</html>
