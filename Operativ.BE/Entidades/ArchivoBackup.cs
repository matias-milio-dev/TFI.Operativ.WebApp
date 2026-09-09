using System;

namespace Operativ.BE.Entidades;
public class ArchivoBackup
{
    public string NombreArchivo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public long TamanioBytes { get; set; }
}
