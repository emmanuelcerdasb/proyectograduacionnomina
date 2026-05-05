using System;

namespace ProyectoGraduacionNomina.Servicios
{
    public class ResumenAsistenciaMensual
    {
        public int EmpleadoId { get; set; }
        public int Mes { get; set; }
        public int Anno { get; set; }

        // Propiedades del empleado
        public string NombreEmpleado { get; set; }
        public string CedulaEmpleado { get; set; }

        
        public int DiasLaborados { get; set; }
        public int DiasAusencia { get; set; }
        public int DiasConTardia { get; set; }
        public double TotalHorasTrabajadas { get; set; }
        public decimal TotalHorasNetas { get; set; }
        public double TotalHorasExtra { get; set; }
    }
}