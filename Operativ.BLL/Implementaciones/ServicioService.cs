using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BLL.Contratos;

namespace Operativ.BLL.Implementaciones;
public class ServicioService : IServicioService
{
    public List<Servicio> ListarServicios()
    {
        return new List<Servicio>
        {
            new Servicio
            {
                IdServicio = 1,
                Nombre = "Device as a Service",
                Descripcion = "Accedé a tecnología de última generación sin inversión inicial, con una cuota mensual fija.",
                ClaveIcono = "DeviceAsAService"
            },
            new Servicio
            {
                IdServicio = 2,
                Nombre = "Gestión de activos",
                Descripcion = "Control total de tus dispositivos, licencias y periféricos en un solo lugar.",
                ClaveIcono = "GestionDeActivos"
            },
            new Servicio
            {
                IdServicio = 3,
                Nombre = "Soporte técnico",
                Descripcion = "Asistencia especializada para que tu operación nunca se detenga.",
                ClaveIcono = "SoporteTecnico"
            },
            new Servicio
            {
                IdServicio = 4,
                Nombre = "Renovación tecnológica",
                Descripcion = "Planificá la renovación de tus equipos de forma simple y sin interrupciones.",
                ClaveIcono = "RenovacionTecnologica"
            },
            new Servicio
            {
                IdServicio = 5,
                Nombre = "Consultoría",
                Descripcion = "Te ayudamos a optimizar tu infraestructura tecnológica según tus objetivos de negocio.",
                ClaveIcono = "Consultoria"
            },
            new Servicio
            {
                IdServicio = 6,
                Nombre = "Gestión de licencias",
                Descripcion = "Administración y control de licencias de software para un uso eficiente y seguro.",
                ClaveIcono = "GestionDeLicencias"
            }
        };
    }
}
