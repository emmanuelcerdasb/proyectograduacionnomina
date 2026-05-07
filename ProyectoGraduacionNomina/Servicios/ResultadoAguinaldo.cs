using System;
using System.Collections.Generic;

namespace ProyectoGraduacionNomina.Servicios
{
    public class DetalleAguinaldoMes
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal SalarioBruto { get; set; }
    }

    public class ResultadoAguinaldo
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; }
        public string CedulaEmpleado { get; set; }
        public int Anio { get; set; }
        public DateTime FechaInicioPeriodo { get; set; }
        public DateTime FechaFinPeriodo { get; set; }
        public List<DetalleAguinaldoMes> Meses { get; set; } = new List<DetalleAguinaldoMes>();
        public decimal SumatoriaSalarios { get; set; }
        public int MesesConsiderados { get; set; }
        public decimal MontoAguinaldo { get; set; }
        public bool YaGuardado { get; set; }
    }
}
