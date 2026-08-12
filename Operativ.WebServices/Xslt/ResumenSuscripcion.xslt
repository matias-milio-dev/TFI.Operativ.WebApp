<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />
  <xsl:template match="/ResumenSuscripcion">
    <div class="resumen-suscripcion">
      <p><strong>Paquete:</strong> <xsl:value-of select="NombrePaquete" /> (<xsl:value-of select="format-number(PrecioBase, '#,##0.00')" />)</p>
      <p><strong>Cliente:</strong> <xsl:value-of select="RazonSocial" /> — CUIT <xsl:value-of select="Cuit" /></p>
      <p><strong>Correo de contacto:</strong> <xsl:value-of select="CorreoElectronico" /></p>
      <p><strong>Generado:</strong> <xsl:value-of select="FechaGeneracion" /></p>
    </div>
  </xsl:template>
</xsl:stylesheet>
