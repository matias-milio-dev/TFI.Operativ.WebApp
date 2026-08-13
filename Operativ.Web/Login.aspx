<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Operativ.Web.Login" %>
<%@ Register TagPrefix="uc" TagName="Notificaciones" Src="~/Controles/Notificaciones.ascx" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Operativ - Iniciar sesión</title>
    <link runat="server" rel="stylesheet" type="text/css" href="~/Estilos/Site.css" />
</head>
<body class="pagina-login">
    <form id="formLogin" runat="server">
        <uc:Notificaciones ID="ucNotificaciones" runat="server" />
        <div class="caja-login">
            <h1>Operativ</h1>
            <div class="campo-formulario">
                <label for="<%= txtNombreUsuario.ClientID %>">Usuario</label>
                <asp:TextBox ID="txtNombreUsuario" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                    ErrorMessage="El usuario es obligatorio" CssClass="texto-validacion" Display="Dynamic" />
            </div>
            <div class="campo-formulario">
                <label for="<%= txtContrasena.ClientID %>">Contraseña</label>
                <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena"
                    ErrorMessage="La contraseña es obligatoria" CssClass="texto-validacion" Display="Dynamic" />
            </div>
            <asp:Button ID="btnIngresar" runat="server" Text="Ingresar" CssClass="boton-principal" OnClick="btnIngresar_Click" />
            <asp:HyperLink ID="lnkRecuperarContrasena" runat="server" NavigateUrl="~/RecuperarContrasena.aspx" CssClass="enlace-secundario" Text="¿Olvidó su contraseña?" />
            <asp:ValidationSummary ID="vsLogin" runat="server" CssClass="texto-validacion" />
        </div>
    </form>
</body>
</html>
