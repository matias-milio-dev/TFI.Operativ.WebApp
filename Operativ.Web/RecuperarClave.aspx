<%@ Page Codebehind="RecuperarClave.aspx.cs" Inherits="Operativ.Web.RecuperarClave" Language="C#" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Operativ - Recuperar contraseña</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="Content/Site.css" />
</head>
<body class="bg-light">
    <form id="frmRecuperar" runat="server">
        <div class="container" style="max-width:420px; margin-top:8vh;">
            <div class="card shadow-sm">
                <div class="card-body p-4">
                    <h1 class="h5 mb-3"><asp:Literal ID="litTitulo" runat="server" Text="Recuperar contraseña" /></h1>

                    <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert alert-info">
                        <asp:Literal ID="litMensaje" runat="server" />
                    </asp:Panel>

                    <div class="mb-3">
                        <asp:Label ID="lblUsuario" runat="server" AssociatedControlID="txtUsuario" CssClass="form-label" />
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50" />
                        <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario"
                            CssClass="text-danger small" Display="Dynamic" />
                    </div>

                    <asp:Button ID="btnEnviar" runat="server" CssClass="btn btn-primary w-100" Text="Enviar" OnClick="btnEnviar_Click" />

                    <div class="text-center mt-3">
                        <a href="Login.aspx">Volver a inicio de sesión</a>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
