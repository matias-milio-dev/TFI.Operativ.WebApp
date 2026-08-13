<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecuperarContrasena.aspx.cs" Inherits="Operativ.Web.RecuperarContrasena" %>
<%@ Register TagPrefix="uc" TagName="Notificaciones" Src="~/Controles/Notificaciones.ascx" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Operativ - Recuperar contraseña</title>
    <link runat="server" rel="stylesheet" type="text/css" href="~/Estilos/Site.css" />
</head>
<body class="pagina-login">
    <form id="formRecuperar" runat="server">
        <uc:Notificaciones ID="ucNotificaciones" runat="server" />
        <div class="caja-login">
            <h1>Recuperar contraseña</h1>
            <div class="campo-formulario">
                <label for="<%= txtNombreUsuario.ClientID %>">Usuario</label>
                <asp:TextBox ID="txtNombreUsuario" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                    ErrorMessage="El usuario es obligatorio" CssClass="texto-validacion" Display="Dynamic" />
            </div>
            <asp:Button ID="btnEnviar" runat="server" Text="Enviar contraseña temporal" CssClass="boton-principal" OnClick="btnEnviar_Click" />
            <asp:HyperLink ID="lnkVolver" runat="server" NavigateUrl="~/Login.aspx" CssClass="enlace-secundario" Text="Volver a iniciar sesión" />
            <asp:ValidationSummary ID="vsRecuperar" runat="server" CssClass="texto-validacion" />
        </div>
    </form>
</body>
</html>
