using System;

namespace ProyectoGraduacionNomina.Servicios
{
    public class ResultadoLiquidacion
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; }
        public string CedulaEmpleado { get; set; }
        public string NombrePuesto { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime FechaLiquidacion { get; set; }
        public int TipoLiquidacionId { get; set; }
        public string NombreTipoLiquidacion { get; set; }
        public bool AplicaCesantia { get; set; }
        public bool AplicaPreaviso { get; set; }

        // Tiempo de servicio
        public int AniosServicio { get; set; }
        public int MesesServicio { get; set; }
        public int DiasServicioTotal { get; set; }

        // Salario de referencia (promedio ultimos 6 meses o salario base)
        public decimal SalarioMensual { get; set; }
        public decimal SalarioDiario { get; set; }

        // Preaviso
        public int DiasPreaviso { get; set; }
        public decimal MontoPreaviso { get; set; }

        // Cesantia
        public int DiasCesantia { get; set; }
        public decimal MontoCesantia { get; set; }

        // Aguinaldo proporcional
        public decimal MontoAguinaldoProporcional { get; set; }
        public int MesesAguinaldo { get; set; }

        // Vacaciones proporcionales
        public decimal DiasVacacionesProporcionales { get; set; }
        public decimal MontoVacacionesProporcionales { get; set; }

        // Totales
        public decimal TotalPercepciones { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal NetoPagar { get; set; }

        public bool YaGuardado { get; set; }
        public string Observaciones { get; set; }
    }
}
