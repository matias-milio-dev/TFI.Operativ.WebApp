<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

  <xsl:template match="/Factura">
    <div class="factura-documento">
      <div class="factura-cabecera">
        <div class="factura-cabecera-emisor">
          <span class="factura-marca">Operativ<span class="factura-marca-acento">.</span></span>
          <span class="factura-eslogan">Gestión de activos tecnológicos</span>
        </div>
        <div class="factura-cabecera-tipo">
          <span class="factura-tipo">FACTURA A</span>
          <span class="factura-cabecera-numero"><xsl:value-of select="@numero" /></span>
        </div>
      </div>

      <div class="factura-cuerpo">
        <div class="factura-seccion-encabezado">
          <h3 class="factura-seccion-titulo">Cliente</h3>
          <div class="factura-numero-bloque">
            <span class="factura-etiqueta">Número de factura</span>
            <span class="factura-numero"><xsl:value-of select="@numero" /></span>
            <span class="factura-fecha">
              <xsl:text>Emitida el </xsl:text>
              <xsl:value-of select="concat(substring(@emitida, 9, 2), '/', substring(@emitida, 6, 2), '/', substring(@emitida, 1, 4))" />
            </span>
          </div>
        </div>

        <div class="factura-cliente">
          <div class="factura-cliente-fila">
            <span class="factura-etiqueta">Razón social</span>
            <span class="factura-cliente-valor factura-destacado"><xsl:value-of select="Receptor/RazonSocial" /></span>
          </div>
          <div class="factura-cliente-fila">
            <span class="factura-etiqueta">CUIT</span>
            <span class="factura-cliente-valor"><xsl:value-of select="Receptor/Cuit" /></span>
          </div>
        </div>

        <h3 class="factura-seccion-titulo">Detalle de facturación</h3>
        <table class="factura-detalle">
          <thead>
            <tr>
              <th>Descripción</th>
              <th class="factura-columna-cantidad">Cantidad</th>
              <th class="factura-columna-importe">Importe</th>
            </tr>
          </thead>
          <tbody>
            <xsl:for-each select="Detalle/Item">
              <tr>
                <td><xsl:value-of select="Descripcion" /></td>
                <td class="factura-columna-cantidad"><xsl:value-of select="Cantidad" /></td>
                <td class="factura-columna-importe"><xsl:value-of select="format-number(Importe, '#,##0.00')" /></td>
              </tr>
            </xsl:for-each>
          </tbody>
        </table>

        <div class="factura-totales">
          <div class="factura-totales-moneda">
            <span class="factura-etiqueta-suave">Moneda</span>
            <strong><xsl:value-of select="Totales/Moneda" /></strong>
          </div>
          <div class="factura-totales-total">
            <span class="factura-etiqueta-suave">TOTAL</span>
            <span class="factura-total"><xsl:value-of select="Totales/Moneda" /><xsl:text> </xsl:text><xsl:value-of select="format-number(Totales/Total, '#,##0.00')" /></span>
          </div>
        </div>

        <div class="factura-comprobante">
          <div>
            <span class="factura-comprobante-titulo">Comprobante de pago</span>
            <span class="factura-comprobante-ayuda">Referencia asociada a la factura.</span>
          </div>
          <span class="factura-comprobante-codigo"><xsl:value-of select="ComprobantePago" /></span>
        </div>

        <div class="factura-pie">
          <span>Documento de exportación - Operativ</span>
          <span>Factura A | <xsl:value-of select="@numero" /></span>
        </div>
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>
