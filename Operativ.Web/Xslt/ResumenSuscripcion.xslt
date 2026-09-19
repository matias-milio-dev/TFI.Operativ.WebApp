<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

  <xsl:template match="/ResumenSuscripcion">
    <div class="resumen-suscripcion">
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Empresa</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Cliente/RazonSocial" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">CUIT</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Cliente/Cuit" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Correo de contacto</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Cliente/Email" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Plan</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Plan/Nombre" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Incluye</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Plan/Descripcion" /></span>
      </div>
      <div class="resumen-suscripcion-fila resumen-suscripcion-fila-total">
        <span class="resumen-suscripcion-etiqueta">Total anual</span>
        <span class="resumen-suscripcion-valor">USD <xsl:value-of select="format-number(Plan/PrecioAnual, '#,##0.00')" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Días de prueba restantes</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Condiciones/DiasTrialRestantes" /></span>
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>
