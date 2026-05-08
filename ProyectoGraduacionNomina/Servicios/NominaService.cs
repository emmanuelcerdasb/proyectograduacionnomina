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
                .Include(e => e.Persona)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            if (empleado == null)
                throw new Exception("Empleado no encontrado.");

            if (empleado.Puesto == null)
                throw new Exception("El empleado no tiene puesto asignado.");

            if (empleado.Persona == null)
                throw new Exception("El empleado no tiene datos personales registrados.");

            // -------------------------------------------------
            // CONFIGURACIÓN BASE
            // -------------------------------------------------
            decimal salarioBaseMensual = empleado.Puesto.salario_base;
            decimal salarioPorHora     = empleado.Puesto.salario_por_hora;
            decimal salarioDiario      = salarioBaseMensual / 30m;

            decimal totalSalarioBase   = 0m;
            decimal totalIncapacidades = 0m;
            decimal totalHorasExtra    = 0m;

            int diasLaborados   = 0;
            int diasIncapacidad = 0;
            int diasPeriodo     = (int)(fechaFin - fechaInicio).TotalDays + 1;

            // -------------------------------------------------
            // RECORRER DÍAS DEL PERIODO
            // -------------------------------------------------
            for (DateTime fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
            {
                var incapacidad = _db.Incapacidad.FirstOrDefault(i =>
                    i.Empleado_idEmpleado == empleadoId &&
                    i.estado == "Aprobada" &&
                    fecha >= i.fecha_inicio &&
                    fecha <= i.fecha_fin);

                if (incapacidad != null)
                {
                    totalIncapacidades += salarioDiario * incapacidad.porcentaje_pago;
                    diasIncapacidad++;
                    continue;
                }

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
            var horasExtraList = _db.HoraExtra
                .Where(h =>
                    h.Empleado_idEmpleado == empleadoId &&
                    h.fecha >= fechaInicio &&
                    h.fecha <= fechaFin &&
                    h.aprobado)
                .ToList();

            decimal totalHorasExtraUnidades = horasExtraList.Sum(h => h.cantidad_horas);
            totalHorasExtra = totalHorasExtraUnidades * salarioPorHora;

            // -------------------------------------------------
            // SALARIO BRUTO
            // -------------------------------------------------
            decimal salarioBruto = totalSalarioBase + totalIncapacidades + totalHorasExtra;

            // -------------------------------------------------
            // DEDUCCIÓN CCSS (aporte del trabajador)
            // IMPORTANTE: la tabla AportesSeguridadSocial debe contener
            // únicamente los porcentajes correspondientes al trabajador.
            // En Costa Rica: SEM 5.50% + IVM 3.84% = 9.34% total.
            // Verificá los registros en esa tabla antes de usar en producción.
            // -------------------------------------------------
            decimal deduccionCCSS = CalcularCCSS(salarioBruto);

            // -------------------------------------------------
            // DEDUCCIÓN RENTA (impuesto sobre la renta CR)
            // Usa los tramos configurados en TramosParametrosRenta para el año.
            // -------------------------------------------------
            decimal deduccionRenta = CalcularRenta(salarioBruto, fechaInicio.Year);

            decimal totalDeducciones = deduccionCCSS + deduccionRenta;
            decimal salarioNeto      = salarioBruto - totalDeducciones;

            // -------------------------------------------------
            // RESULTADO FINAL
            // -------------------------------------------------
            return new ResultadoNomina
            {
                EmpleadoId      = empleadoId,
                NombreEmpleado  = $"{empleado.Persona.nombre} {empleado.Persona.apellido1} {empleado.Persona.apellido2}".Trim(),
                CedulaEmpleado  = empleado.Persona.cedula,
                FechaInicio     = fechaInicio,
                FechaFin        = fechaFin,
                Mes             = fechaInicio.Month,
                Anno            = fechaInicio.Year,

                SalarioBase         = salarioBaseMensual,
                SalarioBaseMensual  = salarioBaseMensual,
                SalarioDiario       = salarioDiario,
                SalarioPorHora      = salarioPorHora,

                DiasLaborablesMes = diasPeriodo,
                DiasLaborados     = diasLaborados,
                DiasIncapacidad   = diasIncapacidad,
                DiasAusencia      = diasPeriodo - diasLaborados - diasIncapacidad,

                HorasExtraAprobadas = totalHorasExtraUnidades,
                MontoSalarioBase    = totalSalarioBase,
                MontoIncapacidades  = totalIncapacidades,
                MontoHorasExtra     = totalHorasExtra,

                SalarioBruto   = salarioBruto,
                TotalDevengado = salarioBruto,

                DeduccionCCSS    = deduccionCCSS,
                DeduccionRenta   = deduccionRenta,
                TotalDeducciones = totalDeducciones,
                SalarioNeto      = salarioNeto
            };
        }

        // =====================================================
        // CCSS — suma los porcentajes de AportesSeguridadSocial
        // =====================================================
        private decimal CalcularCCSS(decimal salarioBruto)
        {
            var aportes = _db.AportesSeguridadSocial.ToList();

            if (!aportes.Any())
                return 0m;

            decimal totalPorcentaje = aportes.Sum(a => a.porcentaje);
            return Math.Round(salarioBruto * (totalPorcentaje / 100m), 2);
        }

        // =====================================================
        // RENTA — tramos progresivos (fórmula CR)
        // Fórmula por tramo: (salario - limite_inferior) * porcentaje% + exceso
        // =====================================================
        private decimal CalcularRenta(decimal salarioBruto, int anio)
        {
            var tramos = _db.TramosParametrosRenta
                .Where(t => t.anio == anio)
                .OrderBy(t => t.limite_inferior)
                .ToList();

            if (!tramos.Any())
                return 0m;

            foreach (var tramo in tramos)
            {
                if (salarioBruto >= tramo.limite_inferior &&
                    salarioBruto <= tramo.limite_superior)
                {
                    decimal impuesto =
                        (salarioBruto - tramo.limite_inferior) * (tramo.porcentaje / 100m)
                        + tramo.exceso;

                    return Math.Round(impuesto, 2);
                }
            }

            // Si el salario supera todos los tramos, aplica el último
            var ultimoTramo = tramos.Last();
            decimal impuestoUltimo =
                (salarioBruto - ultimoTramo.limite_inferior) * (ultimoTramo.porcentaje / 100m)
                + ultimoTramo.exceso;

            return Math.Round(impuestoUltimo, 2);
        }

        // =====================================================
        // GUARDAR NÓMINA — persiste el resultado en BD
        // =====================================================
        public void GuardarNomina(ResultadoNomina resultado)
        {
            int empleadoId = resultado.EmpleadoId;
            int mes        = resultado.Mes;
            int anno       = resultado.Anno;

            // 1. Find or create Nomina header for the period
            var nomina = _db.Nomina.FirstOrDefault(n => n.mes == (byte)mes && n.anio == anno);
            if (nomina == null)
            {
                nomina = new Nomina
                {
                    mes            = (byte)mes,
                    anio           = anno,
                    estado         = "Abierta",
                    fecha_creacion = DateTime.Now
                };
                _db.Nomina.Add(nomina);
                _db.SaveChanges();
            }

            // 2. Guard: duplicate detail for same employee + period
            bool existe = _db.DetalleNomina.Any(d =>
                d.Nomina_idNomina    == nomina.idNomina &&
                d.Empleado_idEmpleado == empleadoId);

            if (existe)
                throw new Exception(
                    $"Ya existe un cálculo guardado para este empleado en el período {mes}/{anno}.");

            // 3. SalarioBrutoMensual
            var sbm = _db.SalarioBrutoMensual.FirstOrDefault(s =>
                s.Empleado_idEmpleado == empleadoId &&
                s.mes  == (byte)mes &&
                s.anio == anno);

            if (sbm == null)
            {
                sbm = new SalarioBrutoMensual
                {
                    Empleado_idEmpleado = empleadoId,
                    mes           = (byte)mes,
                    anio          = anno,
                    salario_bruto = resultado.SalarioBruto,
                    fecha_calculo = DateTime.Now
                };
                _db.SalarioBrutoMensual.Add(sbm);
                _db.SaveChanges();
            }

            // 4. DetalleRenta (nullable — only created when renta > 0)
            int? detalleRentaId = null;
            if (resultado.DeduccionRenta > 0)
            {
                var tramo = _db.TramosParametrosRenta
                    .Where(t => t.anio == anno)
                    .OrderBy(t => t.limite_inferior)
                    .ToList()
                    .FirstOrDefault(t =>
                        resultado.SalarioBruto >= t.limite_inferior &&
                        resultado.SalarioBruto <= t.limite_superior)
                    ?? _db.TramosParametrosRenta
                        .Where(t => t.anio == anno)
                        .OrderByDescending(t => t.limite_inferior)
                        .FirstOrDefault();

                if (tramo != null)
                {
                    var dr = new DetalleRenta
                    {
                        TramosParametrosRenta_idTramosParametrosRenta = tramo.idTramosParametrosRenta,
                        base_calculo   = resultado.SalarioBruto,
                        monto_impuesto = resultado.DeduccionRenta,
                        observacion    = "Calculado automáticamente"
                    };
                    _db.DetalleRenta.Add(dr);
                    _db.SaveChanges();
                    detalleRentaId = dr.idDetalleRenta;
                }
            }

            // 5. DetalleNomina
            var detalle = new DetalleNomina
            {
                Nomina_idNomina                            = nomina.idNomina,
                Empleado_idEmpleado                        = empleadoId,
                SalarioBrutoMensual_idSalarioBrutoMensual  = sbm.idSalarioBrutoMensual,
                DetalleRenta_idDetalleRenta                = detalleRentaId,
                total_bonificaciones                       = 0m,
                total_horas_extra                          = resultado.MontoHorasExtra,
                total_deducciones                          = resultado.TotalDeducciones,
                total_percepciones                         = resultado.SalarioBruto,
                salario_neto                               = resultado.SalarioNeto,
                fecha_calculo                              = DateTime.Now
            };
            _db.DetalleNomina.Add(detalle);
            _db.SaveChanges();

            // 6. DetalleAporte — one row per CCSS aporte
            var aportes = _db.AportesSeguridadSocial.ToList();
            foreach (var aporte in aportes)
            {
                decimal monto = Math.Round(resultado.SalarioBruto * (aporte.porcentaje / 100m), 2);
                _db.DetalleAporte.Add(new DetalleAporte
                {
                    Aporte_idAporte               = aporte.idAporte,
                    DetalleNomina_idDetalleNomina  = detalle.idDetalleNomina,
                    monto                          = monto
                });
            }
            _db.SaveChanges();
        }
    }
}
