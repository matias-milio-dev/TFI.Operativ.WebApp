<%@ Control Codebehind="Footer.ascx.cs" Inherits="Operativ.Web.Controles.Footer" Language="C#" %>
<footer>
    <div class="container">
        <span>&copy; <%= DateTime.Now.Year %> Operativ — <asp:Literal ID="litDerechos" runat="server" /></span>
    </div>
</footer>
