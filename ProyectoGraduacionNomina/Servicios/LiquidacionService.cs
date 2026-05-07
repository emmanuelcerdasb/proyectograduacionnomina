using System;
using System.Data.Entity;
using System.Linq;

namespace ProyectoGraduacionNomina.Servicios
{
    public class LiquidacionService
    {
        private readonly BD_NominaEntities _db;

        public LiquidacionService(BD_NominaEntities context)
        {
            _db = context;
        }

        // =====================================================
        // CALCULAR LIQUIDACION
        // CT CR: preaviso (art.28), cesantia (art.29),
        //        aguinaldo proporcional, vacaciones proporcionales
        // =====================================================
        public ResultadoLiquidacion CalcularLiquidacion(
            int empleadoId, int tipoLiquidacionId, DateTime fechaLiquidacion, string observaciones)
        {
            var empleado = _db.Empleado
                .Include(e => e.Persona)
                .Include(e => e.Puesto)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            if (empleado == null)
                throw new Exception("Empleado no encontrado.");

            var tipo = _db.TipoLiquidacion
                .FirstOrDefault(t => t.idTipoLiquidacion == tipoLiquidacionId);

            if (tipo == null)
                throw new Exception("Tipo de liquidacion no encontrado.");

            bool yaGuardado = _db.Liquidacion.Any(l =>
                l.Empleado_idEmpleado == empleadoId &&
                l.TipoLiquidacion_idTipoLiquidacion == tipoLiquidacionId);

            // -------------------------------------------------
            // Tiempo de servicio
            // -------------------------------------------------
            DateTime fechaIngreso = empleado.fecha_ingreso;
            int diasTotal = (int)(fechaLiquidacion - fechaIngreso).TotalDays;
            int anios = 0;
            int meses = 0;
            {
                DateTime tmp = fechaIngreso;
                while (tmp.AddYears(anios + 1) <= fechaLiquidacion) anios++;
                tmp = tmp.AddYears(anios);
                while (tmp.AddMonths(meses + 1) <= fechaLiquidacion) meses++;
            }
            int totalMeses = anios * 12 + meses;

            // -------------------------------------------------
            // Salario de referencia: promedio ultimos 6 meses
            //   con fallback a salario_base del puesto
            // -------------------------------------------------
            decimal salarioMensual = ObtenerSalarioReferencia(empleadoId, fechaLiquidacion, empleado.Puesto.salario_base);
            decimal salarioDiario = Math.Round(salarioMensual / 30m, 2);

            // -------------------------------------------------
            // PREAVISO (art. 28 CT)
            // -------------------------------------------------
            int diasPreaviso = 0;
            if (tipo.aplica_preaviso)
            {
                if (totalMeses < 3) diasPreaviso = 0;
                else if (totalMeses < 6) diasPreaviso = 7;
                else if (totalMeses < 12) diasPreaviso = 14;
                else diasPreaviso = 30;
            }
            decimal montoPreaviso = Math.Round(salarioDiario * diasPreaviso, 2);

            // -------------------------------------------------
            // CESANTIA (art. 29 CT)
            // Escala deslizante: 1-3 anios = 20 dias/anio,
            //   4-6 = 21, 7+ = 22, tope 8 anios
            // -------------------------------------------------
            int diasCesantia = 0;
            if (tipo.aplica_cesantia)
            {
                if (totalMeses < 3)
                    diasCesantia = 0;
                else if (totalMeses < 6)
                    diasCesantia = 7;
                else if (totalMeses < 12)
                    diasCesantia = 14;
                else
                {
                    int aniosTope = Math.Min(anios, 8);
                    int diasPorAnio;
                    if (aniosTope <= 3) diasPorAnio = 20;
                    else if (aniosTope <= 6) diasPorAnio = 21;
                    else diasPorAnio = 22;
                    diasCesantia = aniosTope * diasPorAnio;
                }
            }
            decimal montoCesantia = Math.Round(salarioDiario * diasCesantia, 2);

            // -------------------------------------------------
            // AGUINALDO PROPORCIONAL
            // Periodo: 1 dic año anterior -> fecha liquidacion
            // Sumatoria salarios del periodo / 12
            // -------------------------------------------------
            int anioAguinaldo = fechaLiquidacion.Month >= 12
                ? fechaLiquidacion.Year + 1
                : fechaLiquidacion.Year;
            DateTime inicioPeriodoAg = new DateTime(anioAguinaldo - 1, 12, 1);
            if (fechaIngreso > inicioPeriodoAg)
                inicioPeriodoAg = fechaIngreso;

            int claveInicioAg = inicioPeriodoAg.Year * 100 + inicioPeriodoAg.Month;
            int claveFinAg = fechaLiquidacion.Year * 100 + fechaLiquidacion.Month;

            var salariosAg = _db.SalarioBrutoMensual
                .Where(s => s.Empleado_idEmpleado == empleadoId)
                .ToList()
                .Where(s =>
                {
                    int c = s.anio * 100 + s.mes;
                    return c >= claveInicioAg && c <= claveFinAg;
                })
                .ToList();

            int mesesAguinaldo = salariosAg.Count;
            decimal sumaAg = salariosAg.Sum(s => s.salario_bruto);
            decimal montoAguinaldoProp = Math.Round(sumaAg / 12m, 2);

            // -------------------------------------------------
            // VACACIONES PROPORCIONALES
            // 14 dias habiles por cada 50 semanas (1 anio)
            // Proporcional = (semanas_trabajadas / 50) * 14
            // -------------------------------------------------
            decimal semanasServicio = diasTotal / 7m;
            decimal diasVacProp = Math.Round((semanasServicio / 50m) * 14m, 2);
            decimal montoVacProp = Math.Round(salarioDiario * diasVacProp, 2);

            // -------------------------------------------------
            // TOTALES (deducciones = 0 en liquidacion tipica CR)
            // -------------------------------------------------
            decimal totalPercepciones = montoPreaviso + montoCesantia + montoAguinaldoProp + montoVacProp;
            decimal totalDeducciones = 0m;
            decimal netoPagar = totalPercepciones - totalDeducciones;

            return new ResultadoLiquidacion
            {
                EmpleadoId            = empleadoId,
                NombreEmpleado        = $"{empleado.Persona.nombre} {empleado.Persona.apellido1} {empleado.Persona.apellido2}".Trim(),
                CedulaEmpleado        = empleado.Persona.cedula,
                NombrePuesto          = empleado.Puesto.nombre,
                FechaIngreso          = fechaIngreso,
                FechaLiquidacion      = fechaLiquidacion,
                TipoLiquidacionId     = tipoLiquidacionId,
                NombreTipoLiquidacion = tipo.nombre,
                AplicaCesantia        = tipo.aplica_cesantia,
                AplicaPreaviso        = tipo.aplica_preaviso,
                AniosServicio         = anios,
                MesesServicio         = meses,
                DiasServicioTotal     = diasTotal,
                SalarioMensual        = salarioMensual,
                SalarioDiario         = salarioDiario,
                DiasPreaviso          = diasPreaviso,
                MontoPreaviso         = montoPreaviso,
                DiasCesantia          = diasCesantia,
                MontoCesantia         = montoCesantia,
                MesesAguinaldo        = mesesAguinaldo,
                MontoAguinaldoProporcional  = montoAguinaldoProp,
                DiasVacacionesProporcionales = diasVacProp,
                MontoVacacionesProporcionales = montoVacProp,
                TotalPercepciones     = totalPercepciones,
                TotalDeducciones      = totalDeducciones,
                NetoPagar             = netoPagar,
                YaGuardado            = yaGuardado,
                Observaciones         = observaciones
            };
        }

        // =====================================================
        // GUARDAR LIQUIDACION
        // =====================================================
        public void GuardarLiquidacion(ResultadoLiquidacion resultado)
        {
            bool existe = _db.Liquidacion.Any(l =>
                l.Empleado_idEmpleado == resultado.EmpleadoId &&
                l.TipoLiquidacion_idTipoLiquidacion == resultado.TipoLiquidacionId);

            if (existe)
                throw new Exception("Ya existe una liquidacion de este tipo para el empleado.");

            _db.Liquidacion.Add(new Liquidacion
            {
                Empleado_idEmpleado              = resultado.EmpleadoId,
                TipoLiquidacion_idTipoLiquidacion = resultado.TipoLiquidacionId,
                fecha                            = resultado.FechaLiquidacion,
                total_percepciones               = resultado.TotalPercepciones,
                total_deducciones                = resultado.TotalDeducciones,
                neto_pagar                       = resultado.NetoPagar,
                observaciones                    = resultado.Observaciones
            });

            _db.SaveChanges();
        }

        // =====================================================
        // OBTENER TODAS LAS LIQUIDACIONES
        // =====================================================
        public System.Collections.Generic.List<Liquidacion> ObtenerTodas()
        {
            return _db.Liquidacion
                .Include(l => l.Empleado.Persona)
                .Include(l => l.TipoLiquidacion)
                .OrderByDescending(l => l.fecha)
                .ToList();
        }

        // =====================================================
        // SALARIO DE REFERENCIA
        // Promedio ultimos 6 meses de SalarioBrutoMensual;
        // si no hay registros usa salario_base del puesto.
        // =====================================================
        private decimal ObtenerSalarioReferencia(int empleadoId, DateTime fechaRef, decimal salarioBase)
        {
            int claveRef = fechaRef.Year * 100 + fechaRef.Month;
            DateTime hace6Meses = fechaRef.AddMonths(-6);
            int claveDesde = hace6Meses.Year * 100 + hace6Meses.Month;

            var registros = _db.SalarioBrutoMensual
                .Where(s => s.Empleado_idEmpleado == empleadoId)
                .ToList()
                .Where(s =>
                {
                    int c = s.anio * 100 + s.mes;
                    return c >= claveDesde && c <= claveRef;
                })
                .ToList();

            if (!registros.Any())
                return salarioBase;

            return Math.Round(registros.Average(s => s.salario_bruto), 2);
        }
    }
}
