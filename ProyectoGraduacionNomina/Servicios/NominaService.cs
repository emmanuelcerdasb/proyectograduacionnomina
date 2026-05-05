using System;
using System.Data.Entity;
using System.Linq;

namespace ProyectoGraduacionNomina.Servicios
{
    public class NominaService
    {
        private readonly BD_NominaEntities _db;

        public NominaService(BD_NominaEntities context)
        {
            _db = context;
        }

        // =====================================================
        // CÁLCULO DE NÓMINA POR EMPLEADO
        // =====================================================
        public ResultadoNomina CalcularNominaEmpleado(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            fechaInicio = fechaInicio.Date;
            fechaFin = fechaFin.Date;

            var empleado = _db.Empleado
                .Include(e => e.Puesto)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            if (empleado == null)
                throw new Exception("Empleado no encontrado.");

            if (empleado.Puesto == null)
                throw new Exception("El empleado no tiene puesto asignado.");

            // -------------------------------------------------
            // CONFIGURACIÓN BASE
            // -------------------------------------------------
            decimal salarioBaseMensual = empleado.Puesto.salario_base;
            decimal salarioPorHora = empleado.Puesto.salario_por_hora;
            decimal salarioDiario = salarioBaseMensual / 30m;

            decimal totalSalarioBase = 0m;
            decimal totalIncapacidades = 0m;
            decimal totalHorasExtra = 0m;

            int diasLaborados = 0;
            int diasIncapacidad = 0;

            // -------------------------------------------------
            // RECORRER DÍAS DEL PERIODO
            // -------------------------------------------------
            for (DateTime fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
            {
                // -----------------------------
                // INCAPACIDAD
                // -----------------------------
                var incapacidad = _db.Incapacidad.FirstOrDefault(i =>
                    i.Empleado_idEmpleado == empleadoId &&
                    i.estado == "Aprobada" &&
                    fecha >= i.fecha_inicio &&
                    fecha <= i.fecha_fin);

                if (incapacidad != null)
                {
                    decimal pagoDia = salarioDiario * incapacidad.porcentaje_pago;
                    totalIncapacidades += pagoDia;
                    diasIncapacidad++;
                    continue;
                }

                // -----------------------------
                // ASISTENCIA REAL
                // -----------------------------
                var asistencia = _db.Asistencia.FirstOrDefault(a =>
                    a.Empleado_idEmpleado == empleadoId &&
                    a.fecha == fecha &&
                    a.hora_entrada != null);

                if (asistencia != null)
                {
                    totalSalarioBase += salarioDiario;
                    diasLaborados++;
                }
            }

            // -------------------------------------------------
            // HORAS EXTRA APROBADAS
            // -------------------------------------------------
            decimal horasExtra = _db.HoraExtra
                .Where(h =>
                    h.Empleado_idEmpleado == empleadoId &&
                    h.fecha >= fechaInicio &&
                    h.fecha <= fechaFin &&
                    h.aprobado)
                .Select(h => h.cantidad_horas)
                .DefaultIfEmpty(0)
                .Sum();

            totalHorasExtra = horasExtra * salarioPorHora;

            // -------------------------------------------------
            // RESULTADO FINAL
            // -------------------------------------------------
            return new ResultadoNomina
            {
                EmpleadoId = empleadoId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,

                SalarioBase = salarioBaseMensual,
                SalarioDiario = salarioDiario,

                DiasLaborados = diasLaborados,
                DiasIncapacidad = diasIncapacidad,

                MontoSalarioBase = totalSalarioBase,
                MontoIncapacidades = totalIncapacidades,
                MontoHorasExtra = totalHorasExtra,

                TotalDevengado =
                    totalSalarioBase +
                    totalIncapacidades +
                    totalHorasExtra
            };
        }
    }
}
