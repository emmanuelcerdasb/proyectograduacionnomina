using ProyectoGraduacionNomina;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH,Jefa,Jefe")]
    public class ReportesController : Controller
    {
        private readonly BD_NominaEntities _db = new BD_NominaEntities();

        private void CargarEmpleados()
        {
            ViewBag.empleadoId = _db.Empleado
                .Include(e => e.Persona)
                .Select(e => new SelectListItem
                {
                    Value = e.idEmpleado.ToString(),
                    Text  = e.Persona.nombre + " " + e.Persona.apellido1 + " "
                          + e.Persona.apellido2 + " - " + e.Persona.cedula
                })
                .ToList();
        }

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            return View();
        }

        // =====================================================
        // REPORTE ASISTENCIA POR EMPLEADO (formulario)
        // =====================================================
        public ActionResult AsistenciaEmpleado()
        {
            CargarEmpleados();
            ViewBag.FechaInicio = DateTime.Today.AddDays(-7);
            ViewBag.FechaFin    = DateTime.Today;
            return View();
        }

        // =====================================================
        // RESUMEN ASISTENCIA MENSUAL (formulario)
        // =====================================================
        public ActionResult ResumenAsistenciaMensual()
        {
            CargarEmpleados();
            ViewBag.Mes  = DateTime.Today.Month;
            ViewBag.Anno = DateTime.Today.Year;
            return View();
        }

        // =====================================================
        // REPORTE NOMINA POR PERIODO
        // =====================================================
        [HttpGet]
        public ActionResult NominaPeriodo()
        {
            ViewBag.AnioActual = DateTime.Today.Year;
            ViewBag.MesActual  = DateTime.Today.Month;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NominaPeriodo(int mes, int anno)
        {
            ViewBag.AnioActual = DateTime.Today.Year;
            ViewBag.MesActual  = DateTime.Today.Month;

            var nomina = _db.Nomina
                .Include(n => n.DetalleNomina.Select(d => d.Empleado.Persona))
                .Include(n => n.DetalleNomina.Select(d => d.Empleado.Puesto))
                .FirstOrDefault(n => n.mes == mes && n.anio == anno);

            ViewBag.Mes  = mes;
            ViewBag.Anno = anno;

            if (nomina == null)
            {
                TempData["Error"] = $"No existe nomina cerrada para {mes}/{anno}.";
                return View();
            }

            return View("ResultadoNominaPeriodo", nomina);
        }

        // =====================================================
        // REPORTE AGUINALDO POR ANIO
        // =====================================================
        [HttpGet]
        public ActionResult AguinaldoAnio()
        {
            ViewBag.AnioActual = DateTime.Today.Year;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AguinaldoAnio(int anio)
        {
            ViewBag.AnioActual = DateTime.Today.Year;

            var aguinaldos = _db.Aguinaldo
                .Include(a => a.Empleado.Persona)
                .Include(a => a.Empleado.Puesto)
                .Where(a => a.anio == anio)
                .OrderBy(a => a.Empleado.Persona.apellido1)
                .ToList();

            ViewBag.Anio = anio;
            return View("ResultadoAguinaldoAnio", aguinaldos);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
