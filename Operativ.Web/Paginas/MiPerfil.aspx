<%@ Page Codebehind="MiPerfil.aspx.cs" Inherits="Operativ.Web.Paginas.MiPerfil" Language="C#" MasterPageFile="~/Master/Site.master" %>
<asp:Content ID="cntContenido" ContentPlaceHolderID="cphContenido" runat="server">
    <h1 class="h3 mb-4">Mi perfil</h1>
    <div class="card" style="max-width:480px;">
        <div class="card-body">
            <dl class="row mb-0">
                <dt class="col-sm-4">Usuario</dt><dd class="col-sm-8"><asp:Literal ID="litUsuario" runat="server" /></dd>
                <dt class="col-sm-4">Nombre completo</dt><dd class="col-sm-8"><asp:Literal ID="litNombreCompleto" runat="server" /></dd>
                <dt class="col-sm-4">Correo</dt><dd class="col-sm-8"><asp:Literal ID="litCorreo" runat="server" /></dd>
                <dt class="col-sm-4">Perfil</dt><dd class="col-sm-8"><asp:Literal ID="litPerfil" runat="server" /></dd>
            </dl>
            <a class="btn btn-outline-primary mt-3" href="~/CambiarClave.aspx" runat="server">Cambiar contraseña</a>
        </div>
    </div>
</asp:Content>
