using System;

namespace ProyectoGraduacionNomina.Servicios
{
    public class ResultadoAsistenciaDiaria
    {
        public int EmpleadoId { get; set; }
        public DateTime Fecha { get; set; }

        // Propiedades del empleado
        public string NombreEmpleado { get; set; }
        public string CedulaEmpleado { get; set; }

        
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
        public double HorasTrabajadas { get; set; }
        public bool JornadaCompleta { get; set; }
        public bool JornadaIncompleta { get; set; }
        public bool SinMarcaciones { get; set; }
        public string Observacion { get; set; }
        public TimeSpan? HoraEsperadaEntrada { get; set; }
        public TimeSpan? HoraEsperadaSalida { get; set; }
        public int MinutosTardia { get; set; }
        public bool TieneTardia { get; set; }
        public bool EsAusencia { get; set; }
        public decimal HorasNetas { get; set; }
    }
}