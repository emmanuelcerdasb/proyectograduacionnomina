using ProyectoGraduacionNomina;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        public ActionResult Index()
        {
            int hoy_mes  = DateTime.Today.Month;
            int hoy_anio = DateTime.Today.Year;

            // Conteos globales (todos los roles ven esto)
            ViewBag.TotalEmpleadosActivos = _db.Empleado.Count(e => e.estado == "Activo");
            ViewBag.VacacionesPendientes  = _db.Vacaciones.Count(v => v.estado == "Pendiente");
            ViewBag.HorasExtraPendientes  = _db.HoraExtra.Count(h => !h.aprobado);

            // Nómina del mes actual
            var nominaMes = _db.Nomina
                .Include(n => n.DetalleNomina)
                .FirstOrDefault(n => n.mes == hoy_mes && n.anio == hoy_anio);

            ViewBag.NominaEmpleadosMes = nominaMes?.DetalleNomina?.Count ?? 0;
            ViewBag.NominaTotalMes     = nominaMes?.DetalleNomina?.Sum(d => d.salario_neto) ?? 0m;
            ViewBag.MesActual          = new DateTime(hoy_anio, hoy_mes, 1).ToString("MMMM yyyy");

            // Evaluaciones recientes (solo Admin/RRHH)
            if (User.IsInRole("Administrador") || User.IsInRole("RRHH"))
            {
                ViewBag.EvaluacionesMes = _db.EvaluacionPersonal
                    .Count(e => e.anio == hoy_anio);
            }

            // Aguinaldos calculados este año
            ViewBag.AguinaldosAnio = _db.Aguinaldo.Count(a => a.anio == hoy_anio);

            // Liquidaciones este año
            ViewBag.LiquidacionesAnio = _db.Liquidacion
                .Count(l => l.fecha.Year == hoy_anio);

            // Últimas 5 solicitudes de vacaciones pendientes
            ViewBag.UltimasVacaciones = _db.Vacaciones
                .Include(v => v.Empleado.Persona)
                .Where(v => v.estado == "Pendiente")
                .OrderByDescending(v => v.fecha_solicitud)
                .Take(5)
                .ToList();

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
