using System;

namespace ProyectoGraduacionNomina.Servicios
{
    public class ResultadoNomina
    {
        // ===============================
        // Identificación
        // ===============================
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; }
        public string CedulaEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public int Mes { get; set; }
        public int Anno { get; set; }

        // ===============================
        // Salarios base (desde Puesto)
        // ===============================
        public decimal SalarioBase { get; set; }
        public decimal SalarioDiario { get; set; }
        public decimal SalarioBaseMensual { get; set; }
        public decimal SalarioPorHora { get; set; }

        // ===============================
        // Asistencia
        // ===============================
        public int DiasLaborablesMes { get; set; }
        public int DiasLaborados { get; set; }
        public int DiasAusencia { get; set; }

        // ===============================
        // Incapacidades
        // ===============================
        public int DiasIncapacidad { get; set; }

        // ===============================
        // MONTOS
        // ===============================
        public decimal MontoSalarioBase { get; set; }
        public decimal MontoIncapacidades { get; set; }
        public decimal MontoHorasExtra { get; set; }

        // ===============================
        // Horas extra
        // ===============================
        public decimal HorasExtraAprobadas { get; set; }

        // ===============================
        // Totales devengados
        // ===============================
        public decimal SalarioBruto { get; set; }
        public decimal TotalDevengado { get; set; }

        // ===============================
        // Deducciones
        // ===============================
        public decimal DeduccionCCSS { get; set; }
        public decimal DeduccionRenta { get; set; }
        public decimal TotalDeducciones { get; set; }

        // ===============================
        // Neto a pagar
        // ===============================
        public decimal SalarioNeto { get; set; }
    }
}
