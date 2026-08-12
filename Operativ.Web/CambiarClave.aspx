<%@ Page Codebehind="CambiarClave.aspx.cs" Inherits="Operativ.Web.CambiarClave" Language="C#" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Operativ - Cambiar contraseña</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="Content/Site.css" />
</head>
<body class="bg-light">
    <form id="frmCambiarClave" runat="server">
        <div class="container" style="max-width:460px; margin-top:8vh;">
            <div class="card shadow-sm">
                <div class="card-body p-4">
                    <h1 class="h5 mb-3">Cambiar contraseña</h1>
                    <asp:Panel ID="pnlAviso" runat="server" CssClass="alert alert-warning">
                        <asp:Literal ID="litAviso" runat="server" />
                    </asp:Panel>

                    <asp:ValidationSummary ID="vsCambiarClave" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" />

                    <div class="mb-3">
                        <asp:Label ID="lblClaveActual" runat="server" AssociatedControlID="txtClaveActual" CssClass="form-label" />
                        <asp:TextBox ID="txtClaveActual" runat="server" CssClass="form-control" TextMode="Password" />
                        <asp:RequiredFieldValidator ID="rfvClaveActual" runat="server" ControlToValidate="txtClaveActual" CssClass="text-danger small" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <asp:Label ID="lblClaveNueva" runat="server" AssociatedControlID="txtClaveNueva" CssClass="form-label" />
                        <asp:TextBox ID="txtClaveNueva" runat="server" CssClass="form-control" TextMode="Password" />
                        <asp:RequiredFieldValidator ID="rfvClaveNueva" runat="server" ControlToValidate="txtClaveNueva" CssClass="text-danger small" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="revClaveNueva" runat="server" ControlToValidate="txtClaveNueva"
                            ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$" CssClass="text-danger small" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <asp:Label ID="lblConfirmarClave" runat="server" AssociatedControlID="txtConfirmarClave" CssClass="form-label" />
                        <asp:TextBox ID="txtConfirmarClave" runat="server" CssClass="form-control" TextMode="Password" />
                        <asp:CompareValidator ID="cvConfirmarClave" runat="server" ControlToValidate="txtConfirmarClave"
                            ControlToCompare="txtClaveNueva" CssClass="text-danger small" Display="Dynamic" />
                    </div>

                    <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-primary w-100" OnClick="btnConfirmar_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>
