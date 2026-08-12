<%@ Page Codebehind="Login.aspx.cs" Inherits="Operativ.Web.Login" Language="C#" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Operativ - Iniciar sesión</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="Content/Site.css" />
</head>
<body class="bg-light">
    <form id="frmLogin" runat="server">
        <div class="container" style="max-width:420px; margin-top:8vh;">
            <div class="card shadow-sm">
                <div class="card-body p-4">
                    <h1 class="h4 mb-3 text-center"><asp:Literal ID="litTitulo" runat="server" /></h1>

                    <asp:ValidationSummary ID="vsLogin" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />
                    <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert alert-warning">
                        <asp:Literal ID="litMensaje" runat="server" />
                    </asp:Panel>

                    <div class="mb-3">
                        <asp:Label ID="lblUsuario" runat="server" AssociatedControlID="txtUsuario" CssClass="form-label" />
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50" />
                        <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario"
                            CssClass="text-danger small" Display="Dynamic" />
                    </div>

                    <div class="mb-3">
                        <asp:Label ID="lblClave" runat="server" AssociatedControlID="txtClave" CssClass="form-label" />
                        <asp:TextBox ID="txtClave" runat="server" CssClass="form-control" TextMode="Password" MaxLength="100" />
                        <asp:RequiredFieldValidator ID="rfvClave" runat="server" ControlToValidate="txtClave"
                            CssClass="text-danger small" Display="Dynamic" />
                    </div>

                    <asp:Button ID="btnIngresar" runat="server" CssClass="btn btn-primary w-100" OnClick="btnIngresar_Click" />

                    <div class="text-center mt-3">
                        <asp:HyperLink ID="lnkRecuperarClave" runat="server" NavigateUrl="~/RecuperarClave.aspx" />
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
