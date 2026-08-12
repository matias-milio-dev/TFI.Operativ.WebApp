<%@ Page Codebehind="ErrorGenerico.aspx.cs" Inherits="Operativ.Web.Paginas.ErrorGenerico" Language="C#" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Operativ - Error</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
</head>
<body class="bg-light">
    <form id="frmError" runat="server">
        <div class="container" style="max-width:480px; margin-top:10vh;">
            <div class="card shadow-sm">
                <div class="card-body p-4 text-center">
                    <h1 class="h5 text-danger mb-3">Ocurrió un error</h1>
                    <asp:Literal ID="litMensaje" runat="server" Text="Ocurrió un error inesperado en el sistema." />
                    <div class="mt-3">
                        <a class="btn btn-primary" href="~/Default.aspx" runat="server">Volver al inicio</a>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
