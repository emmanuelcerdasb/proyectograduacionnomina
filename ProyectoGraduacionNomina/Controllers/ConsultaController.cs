using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH,Jefa,Jefe")]
    public class ConsultaController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        public ActionResult Index()
        {
            return View();
        }

        // =====================================================
        // PERFIL 360 DEL EMPLEADO
        // =====================================================
        [HttpGet]
        public ActionResult PerfilEmpleado()
        {
            CargarEmpleados();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PerfilEmpleado(int empleadoId)
        {
            CargarEmpleados();

            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                return View();
            }

            var empleado = _db.Empleado
                .Include(e => e.Persona)
                .Include(e => e.Puesto.Departamento)
                .Include(e => e.Jornada)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            if (empleado == null)
            {
                TempData["Error"] = "Empleado no encontrado.";
                return View();
            }

            // Nominas recientes (ultimos 6 meses)
            var nominas = _db.DetalleNomina
                .Include(d => d.Nomina)
                .Where(d => d.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(d => d.Nomina.anio).ThenByDescending(d => d.Nomina.mes)
                .Take(6)
                .ToList();

            // Vacaciones (ultimas 5)
            var vacaciones = _db.Vacaciones
                .Where(v => v.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(v => v.fecha_solicitud)
                .Take(5)
                .ToList();

            // Evaluaciones
            var evaluaciones = _db.EvaluacionPersonal
                .Where(e => e.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(e => e.anio).ThenByDescending(e => e.semestre)
                .Take(4)
                .ToList();

            // Liquidaciones
            var liquidaciones = _db.Liquidacion
                .Include(l => l.TipoLiquidacion)
                .Where(l => l.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(l => l.fecha)
                .ToList();

            // Aguinaldos
            var aguinaldos = _db.Aguinaldo
                .Where(a => a.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(a => a.anio)
                .Take(3)
                .ToList();

            // Horas extra (ultimas 5 aprobadas)
            var horasExtra = _db.HoraExtra
                .Where(h => h.Empleado_idEmpleado == empleadoId && h.aprobado)
                .OrderByDescending(h => h.fecha)
                .Take(5)
                .ToList();

            ViewBag.Empleado     = empleado;
            ViewBag.Nominas      = nominas;
            ViewBag.Vacaciones   = vacaciones;
            ViewBag.Evaluaciones = evaluaciones;
            ViewBag.Liquidaciones = liquidaciones;
            ViewBag.Aguinaldos   = aguinaldos;
            ViewBag.HorasExtra   = horasExtra;

            return View("ResultadoPerfilEmpleado");
        }

        private void CargarEmpleados()
        {
            ViewBag.EmpleadoId = _db.Empleado
                .Include(e => e.Persona)
                .Select(e => new SelectListItem
                {
                    Value = e.idEmpleado.ToString(),
                    Text  = e.Persona.nombre + " " + e.Persona.apellido1 + " " + e.Persona.apellido2
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
