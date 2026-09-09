using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Contratos;
public interface IBitacoraRepositorio
{
    void Registrar(Bitacora entrada);

    List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina);

    int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta);
}
