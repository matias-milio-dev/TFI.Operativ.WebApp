<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

  <xsl:template match="/ComprobantePago">
    <div class="comprobante">
      <div class="comprobante-encabezado">
        <span class="comprobante-titulo">Comprobante de pago</span>
        <span class="comprobante-numero"><xsl:value-of select="@codigo" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Empresa</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Cliente/RazonSocial" /> — CUIT <xsl:value-of select="Cliente/Cuit" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Plan</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Pago/Plan" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Medio de pago</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Pago/MedioPago" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Fecha de pago</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Pago/FechaPago" /></span>
      </div>
      <div class="resumen-suscripcion-fila">
        <span class="resumen-suscripcion-etiqueta">Vigente hasta</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Pago/FechaVencimiento" /></span>
      </div>
      <div class="resumen-suscripcion-fila resumen-suscripcion-fila-total">
        <span class="resumen-suscripcion-etiqueta">Importe abonado</span>
        <span class="resumen-suscripcion-valor"><xsl:value-of select="Pago/Moneda" /><xsl:text> </xsl:text><xsl:value-of select="format-number(Pago/Importe, '#,##0.00')" /></span>
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>
