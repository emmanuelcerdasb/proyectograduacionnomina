using ProyectoGraduacionNomina.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH,Jefe,Jefa")]
    public class VacacionesController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            var vacaciones = _db.Vacaciones
                .Include(v => v.Empleado.Persona)
                .Include(v => v.SolicitudVacaciones)
                .OrderByDescending(v => v.fecha_solicitud)
                .ToList();
            return View(vacaciones);
        }

        // =====================================================
        // DETALLE
        // =====================================================
        public ActionResult Detalle(int id)
        {
            var vac = _db.Vacaciones
                .Include(v => v.Empleado.Persona)
                .Include(v => v.Empleado.Puesto)
                .Include(v => v.SolicitudVacaciones)
                .Include(v => v.HistorialVacaciones)
                .FirstOrDefault(v => v.idVacaciones == id);

            if (vac == null)
            {
                TempData["Error"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            return View(vac);
        }

        // =====================================================
        // SOLICITAR (GET)
        // =====================================================
        [HttpGet]
        public ActionResult Solicitar()
        {
            CargarEmpleados();
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        // =====================================================
        // SOLICITAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Solicitar(int empleadoId, string fechaInicio,
            string fechaFin, string observaciones)
        {
            CargarEmpleados();
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-dd");

            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                return View();
            }

            if (!DateTime.TryParse(fechaInicio, out DateTime inicio) ||
                !DateTime.TryParse(fechaFin,    out DateTime fin))
            {
                TempData["Error"] = "Fechas invalidas.";
                return View();
            }

            if (fin < inicio)
            {
                TempData["Error"] = "La fecha de fin debe ser mayor o igual a la fecha de inicio.";
                return View();
            }

            int diasHabiles = ContarDiasHabiles(inicio, fin);
            if (diasHabiles == 0)
            {
                TempData["Error"] = "El periodo seleccionado no contiene dias habiles.";
                return View();
            }

            try
            {
                // Crear registro de vacaciones
                var vac = new Vacaciones
                {
                    Empleado_idEmpleado = empleadoId,
                    fecha_inicio       = inicio,
                    fecha_fin          = fin,
                    dias_habiles       = diasHabiles,
                    estado             = "Pendiente",
                    observaciones      = observaciones ?? "",
                    fecha_solicitud    = DateTime.Now,
                    fecha_aprobacion   = null
                };
                _db.Vacaciones.Add(vac);
                _db.SaveChanges();

                // Crear solicitud vinculada
                var sol = new SolicitudVacaciones
                {
                    Vacaciones_idVacaciones = vac.idVacaciones,
                    fecha_inicio            = inicio,
                    fecha_fin               = fin,
                    fecha_solicitud         = DateTime.Now,
                    estado                  = "Pendiente",
                    aprobado_por            = null,
                    fecha_aprobacion        = null
                };
                _db.SolicitudVacaciones.Add(sol);

                // Historial
                _db.HistorialVacaciones.Add(new HistorialVacaciones
                {
                    Vacaciones_idVacaciones = vac.idVacaciones,
                    accion                 = "Solicitud creada",
                    comentario             = $"{diasHabiles} dias habiles del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}",
                    fecha                  = DateTime.Now
                });

                _db.SaveChanges();

                if (Session["CredencialId"] != null)
                    BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                        "SOLICITAR VACACIONES",
                        $"Solicitud de vacaciones: empleadoId={empleadoId} | {inicio:dd/MM/yyyy}-{fin:dd/MM/yyyy} | {diasHabiles} dias habiles",
                        this.HttpContext);

                TempData["Success"] = $"Solicitud de vacaciones registrada ({diasHabiles} dias habiles).";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        // =====================================================
        // APROBAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aprobar(int id, string comentario)
        {
            var vac = _db.Vacaciones
                .Include(v => v.SolicitudVacaciones)
                .FirstOrDefault(v => v.idVacaciones == id);

            if (vac == null)
            {
                TempData["Error"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            try
            {
                vac.estado           = "Aprobado";
                vac.fecha_aprobacion = DateTime.Now;

                var sol = vac.SolicitudVacaciones.FirstOrDefault();
                if (sol != null)
                {
                    sol.estado           = "Aprobado";
                    sol.aprobado_por     = 1;
                    sol.fecha_aprobacion = DateTime.Now;
                }

                _db.HistorialVacaciones.Add(new HistorialVacaciones
                {
                    Vacaciones_idVacaciones = id,
                    accion                 = "Aprobado",
                    comentario             = comentario ?? "",
                    fecha                  = DateTime.Now
                });

                _db.SaveChanges();

                if (Session["CredencialId"] != null)
                    BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                        "APROBAR VACACIONES",
                        $"Vacaciones idVacaciones={id} aprobadas.",
                        this.HttpContext);

                TempData["Success"] = "Solicitud aprobada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Detalle", new { id });
        }

        // =====================================================
        // RECHAZAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rechazar(int id, string comentario)
        {
            var vac = _db.Vacaciones
                .Include(v => v.SolicitudVacaciones)
                .FirstOrDefault(v => v.idVacaciones == id);

            if (vac == null)
            {
                TempData["Error"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            try
            {
                vac.estado           = "Rechazado";
                vac.fecha_aprobacion = DateTime.Now;

                var sol = vac.SolicitudVacaciones.FirstOrDefault();
                if (sol != null)
                {
                    sol.estado           = "Rechazado";
                    sol.aprobado_por     = 1;
                    sol.fecha_aprobacion = DateTime.Now;
                }

                _db.HistorialVacaciones.Add(new HistorialVacaciones
                {
                    Vacaciones_idVacaciones = id,
                    accion                 = "Rechazado",
                    comentario             = comentario ?? "",
                    fecha                  = DateTime.Now
                });

                _db.SaveChanges();

                if (Session["CredencialId"] != null)
                    BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                        "RECHAZAR VACACIONES",
                        $"Vacaciones idVacaciones={id} rechazadas. Motivo: {comentario}",
                        this.HttpContext);

                TempData["Success"] = "Solicitud rechazada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Detalle", new { id });
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private static int ContarDiasHabiles(DateTime inicio, DateTime fin)
        {
            int dias = 0;
            for (DateTime d = inicio; d <= fin; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    dias++;
            }
            return dias;
        }

        private void CargarEmpleados()
        {
            ViewBag.EmpleadoId = _db.Empleado
                .Include(e => e.Persona)
                .Where(e => e.estado == "Activo")
                .Select(e => new SelectListItem
                {
                    Value = e.idEmpleado.ToString(),
                    Text  = e.Persona.nombre + " " + e.Persona.apellido1 + " " + e.Persona.apellido2
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
