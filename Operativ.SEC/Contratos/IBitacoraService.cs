using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.SEC.Contratos;
public interface IBitacoraService
{
    void Registrar(int? idUsuario, TipoAccionBitacora accion);

    void Registrar(int? idUsuario, TipoAccionBitacora accion, string detalleAdicional);

    List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina);

    int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta);
}
