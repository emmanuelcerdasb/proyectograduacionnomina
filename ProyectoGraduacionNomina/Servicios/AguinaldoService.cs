using System;
using System.Data.Entity;
using System.Linq;

namespace ProyectoGraduacionNomina.Servicios
{
    public class AguinaldoService
    {
        private readonly BD_NominaEntities _db;

        public AguinaldoService(BD_NominaEntities context)
        {
            _db = context;
        }

        // =====================================================
        // CALCULAR AGUINALDO
        // Período CR: 1 dic año anterior → 30 nov año actual
        // Fórmula: sumatoria salarios brutos del período / 12
        // =====================================================
        public ResultadoAguinaldo CalcularAguinaldo(int empleadoId, int anio)
        {
            var empleado = _db.Empleado
                .Include(e => e.Persona)
                .Include(e => e.Puesto)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            if (empleado == null)
                throw new Exception("Empleado no encontrado.");

            if (empleado.Persona == null)
                throw new Exception("El empleado no tiene datos personales registrados.");

            DateTime fechaInicioPeriodo = new DateTime(anio - 1, 12, 1);
            DateTime fechaFinPeriodo    = new DateTime(anio, 11, 30);

            // Si ingresó después del inicio del período, ajustar
            if (empleado.fecha_ingreso > fechaInicioPeriodo)
                fechaInicioPeriodo = empleado.fecha_ingreso;

            // Claves comparables: año*100+mes
            int claveInicio = fechaInicioPeriodo.Year * 100 + fechaInicioPeriodo.Month;
            int claveFin    = fechaFinPeriodo.Year   * 100 + fechaFinPeriodo.Month;

            var salarios = _db.SalarioBrutoMensual
                .Where(s => s.Empleado_idEmpleado == empleadoId)
                .ToList()
                .Where(s =>
                {
                    int clave = s.anio * 100 + s.mes;
                    return clave >= claveInicio && clave <= claveFin;
                })
                .OrderBy(s => s.anio).ThenBy(s => s.mes)
                .ToList();

            decimal sumatoria = salarios.Sum(s => s.salario_bruto);
            decimal monto     = Math.Round(sumatoria / 12m, 2);

            bool yaGuardado = _db.Aguinaldo.Any(a =>
                a.Empleado_idEmpleado == empleadoId && a.anio == anio);

            var resultado = new ResultadoAguinaldo
            {
                EmpleadoId         = empleadoId,
                NombreEmpleado     = $"{empleado.Persona.nombre} {empleado.Persona.apellido1} {empleado.Persona.apellido2}".Trim(),
                CedulaEmpleado     = empleado.Persona.cedula,
                Anio               = anio,
                FechaInicioPeriodo = fechaInicioPeriodo,
                FechaFinPeriodo    = fechaFinPeriodo,
                SumatoriaSalarios  = sumatoria,
                MesesConsiderados  = salarios.Count,
                MontoAguinaldo     = monto,
                YaGuardado         = yaGuardado
            };

            foreach (var s in salarios)
            {
                resultado.Meses.Add(new DetalleAguinaldoMes
                {
                    Mes          = s.mes,
                    Anio         = s.anio,
                    SalarioBruto = s.salario_bruto
                });
            }

            return resultado;
        }

        // =====================================================
        // GUARDAR AGUINALDO
        // =====================================================
        public void GuardarAguinaldo(ResultadoAguinaldo resultado)
        {
            bool existe = _db.Aguinaldo.Any(a =>
                a.Empleado_idEmpleado == resultado.EmpleadoId &&
                a.anio == resultado.Anio);

            if (existe)
                throw new Exception(
                    $"Ya existe un aguinaldo guardado para este empleado en {resultado.Anio}.");

            _db.Aguinaldo.Add(new Aguinaldo
            {
                Empleado_idEmpleado = resultado.EmpleadoId,
                anio          = resultado.Anio,
                monto         = resultado.MontoAguinaldo,
                fecha_calculo = DateTime.Now,
                fecha_pago    = new DateTime(resultado.Anio, 12, 20),
                estado        = "Calculado"
            });

            _db.SaveChanges();
        }

        // =====================================================
        // OBTENER AGUINALDOS GUARDADOS
        // =====================================================
        public System.Collections.Generic.List<Aguinaldo> ObtenerTodos()
        {
            return _db.Aguinaldo
                .Include(a => a.Empleado.Persona)
                .OrderByDescending(a => a.anio)
                .ThenBy(a => a.Empleado.Persona.apellido1)
                .ToList();
        }
    }
}
