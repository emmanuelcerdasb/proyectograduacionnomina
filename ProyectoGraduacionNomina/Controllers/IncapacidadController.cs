using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize]
    public class IncapacidadController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            return View();
        }

        // =====================================================
        // MIS INCAPACIDADES (COLABORADOR)
        // =====================================================
        public ActionResult MisIncapacidades()
        {
            int empleadoId = ObtenerEmpleadoIdSesion();

            var incapacidades = _db.Incapacidad
                .Include(i => i.TipoIncapacidad)
                .Where(i => i.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(i => i.fecha_inicio)
                .ToList();

            return View(incapacidades);
        }

        // =====================================================
        // INCAPACIDADES ACTIVAS (RRHH / JEFATURA)
        // =====================================================
        [Authorize(Roles = "Administrador,Jefe,Jefa,RRHH")]
        public ActionResult IncapacidadesActivas()
        {
            var incapacidades = _db.Incapacidad
                .Include(i => i.Empleado)
                .Include(i => i.TipoIncapacidad)
                .Where(i => i.estado == "Pendiente" || i.estado == "Aprobada")
                .OrderByDescending(i => i.fecha_inicio)
                .ToList();

            return View(incapacidades);
        }

        // =====================================================
        // CREAR INCAPACIDAD (GET)
        // =====================================================
        public ActionResult CrearIncapacidad()
        {
            CargarTiposIncapacidad();
            CargarEmpleadosSiCorresponde();
            ViewBag.FechaMin = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        // =====================================================
        // CREAR INCAPACIDAD (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearIncapacidad(Incapacidad incapacidad)
        {
            try
            {
                // ===============================
                // Empleado
                // ===============================
                if (User.IsInRole("Colaborador"))
                    incapacidad.Empleado_idEmpleado = ObtenerEmpleadoIdSesion();
                else if (incapacidad.Empleado_idEmpleado == 0)
                    throw new Exception("Debe seleccionar un empleado.");

                // ===============================
                // Validaciones legales
                // ===============================
                if (incapacidad.fecha_fin < incapacidad.fecha_inicio)
                    throw new Exception("La fecha fin no puede ser menor a la fecha inicio.");

                if (incapacidad.porcentaje_pago < 0 || incapacidad.porcentaje_pago > 1)
                    throw new Exception("El porcentaje de pago debe estar entre 0 y 1 (ejemplo: 0.60 = 60%).");

                // ===============================
                // Origen (OBLIGATORIO)
                // ===============================
                if (User.IsInRole("Colaborador"))
                    incapacidad.origen = "Colaborador";
                else if (User.IsInRole("RRHH"))
                    incapacidad.origen = "RRHH";
                else if (User.IsInRole("Jefe") || User.IsInRole("Jefa"))
                    incapacidad.origen = "Jefatura";
                else
                    incapacidad.origen = "Administrador";

                // ===============================
                // Campos del sistema
                // ===============================
                incapacidad.estado = "Pendiente";
                incapacidad.fecha_registro = DateTime.Now;
                incapacidad.fecha_actualizacion = null;
                incapacidad.es_prorroga = false;
                incapacidad.afecta_asistencia = true;

                incapacidad.dias_incapacidad =
                    (incapacidad.fecha_fin - incapacidad.fecha_inicio).Days + 1;

                _db.Incapacidad.Add(incapacidad);
                _db.SaveChanges();

                TempData["Success"] = "Incapacidad registrada correctamente.";
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var entityErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityErrors.ValidationErrors)
                    {
                        sb.AppendLine($"Campo: {validationError.PropertyName} - Error: {validationError.ErrorMessage}");
                    }
                }

                TempData["Error"] = sb.ToString();
                CargarTiposIncapacidad();
                CargarEmpleadosSiCorresponde();
                ViewBag.FechaMin = DateTime.Today.ToString("yyyy-MM-dd");
                return View(incapacidad);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                CargarTiposIncapacidad();
                CargarEmpleadosSiCorresponde();
                ViewBag.FechaMin = DateTime.Today.ToString("yyyy-MM-dd");
                return View(incapacidad);
            }
        }

        // =====================================================
        // APROBAR INCAPACIDAD (IMPACTA ASISTENCIA)
        // =====================================================
        [Authorize(Roles = "Administrador,Jefe,Jefa,RRHH")]
        public ActionResult Aprobar(int id)
        {
            var incapacidad = _db.Incapacidad.Find(id);

            if (incapacidad == null)
                return HttpNotFound();

            if (incapacidad.estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden aprobar incapacidades pendientes.";
                return RedirectToAction("IncapacidadesActivas");
            }

            incapacidad.estado = "Aprobada";
            incapacidad.fecha_actualizacion = DateTime.Now;

            // 🔥 IMPACTO REAL EN ASISTENCIA
            AplicarImpactoAsistencia(incapacidad);

            _db.SaveChanges();

            TempData["Success"] = "Incapacidad aprobada correctamente.";
            return RedirectToAction("IncapacidadesActivas");
        }

        // =====================================================
        // RECHAZAR INCAPACIDAD
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Jefe,Jefa,RRHH")]
        [ValidateAntiForgeryToken]
        public ActionResult Rechazar(int id, string observaciones)
        {
            var incapacidad = _db.Incapacidad.Find(id);

            if (incapacidad == null)
                return HttpNotFound();

            if (incapacidad.estado != "Pendiente")
            {
                TempData["Error"] = "Solo se pueden rechazar incapacidades pendientes.";
                return RedirectToAction("IncapacidadesActivas");
            }

            incapacidad.estado = "Rechazada";
            incapacidad.observaciones_rrhh = observaciones;
            incapacidad.fecha_actualizacion = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Incapacidad rechazada correctamente.";
            return RedirectToAction("IncapacidadesActivas");
        }

        // =====================================================
        // FINALIZAR INCAPACIDAD (USO INTERNO / NÓMINA)
        // =====================================================
        public void FinalizarIncapacidad(int incapacidadId)
        {
            var incapacidad = _db.Incapacidad.Find(incapacidadId);

            if (incapacidad == null)
                throw new Exception("Incapacidad no encontrada.");

            if (incapacidad.estado != "Aprobada")
                throw new Exception("Solo se pueden finalizar incapacidades aprobadas.");

            incapacidad.estado = "Finalizada";
            incapacidad.fecha_actualizacion = DateTime.Now;

            _db.SaveChanges();
        }

        // =====================================================
        // IMPACTO REAL EN ASISTENCIA
        // =====================================================
        private void AplicarImpactoAsistencia(Incapacidad incapacidad)
        {
            DateTime fecha = incapacidad.fecha_inicio;

            while (fecha <= incapacidad.fecha_fin)
            {
                var asistencia = _db.Asistencia.FirstOrDefault(a =>
                    a.Empleado_idEmpleado == incapacidad.Empleado_idEmpleado &&
                    DbFunctions.TruncateTime(a.fecha) == fecha.Date);

                if (asistencia == null)
                {
                    asistencia = new Asistencia
                    {
                        Empleado_idEmpleado = incapacidad.Empleado_idEmpleado,
                        fecha = fecha,
                        estado = "Incapacidad",
                        es_justificada = true,
                        horas_trabajadas = 0,
                        observacion = "Incapacidad aprobada",
                        fecha_registro = DateTime.Now
                    };

                    _db.Asistencia.Add(asistencia);
                }
                else
                {
                    asistencia.estado = "Incapacidad";
                    asistencia.es_justificada = true;
                    asistencia.horas_trabajadas = 0;
                    asistencia.observacion = "Incapacidad aprobada";
                    asistencia.fecha_actualizacion = DateTime.Now;
                }

                fecha = fecha.AddDays(1);
            }
        }

        // =====================================================
        // MÉTODOS AUXILIARES
        // =====================================================
        private void CargarTiposIncapacidad()
        {
            ViewBag.TipoIncapacidad_idTipoIncapacidad = _db.TipoIncapacidad
                .Select(t => new SelectListItem
                {
                    Value = t.idTipoIncapacidad.ToString(),
                    Text = t.nombre
                })
                .ToList();
        }

        private void CargarEmpleadosSiCorresponde()
        {
            if (User.IsInRole("Administrador") ||
                User.IsInRole("Jefe") ||
                User.IsInRole("Jefa") ||
                User.IsInRole("RRHH"))
            {
                ViewBag.Empleado_idEmpleado = _db.Empleado
                    .Select(e => new SelectListItem
                    {
                        Value = e.idEmpleado.ToString(),
                        Text = e.Persona.nombre + " " +
                               e.Persona.apellido1 + " " +
                               e.Persona.apellido2
                    })
                    .ToList();
            }
        }

        private int ObtenerEmpleadoIdSesion()
        {
            if (Session["EmpleadoId"] == null)
                throw new Exception("No se encontró el EmpleadoId en sesión.");

            return Convert.ToInt32(Session["EmpleadoId"]);
        }

        // =====================================================
        // DISPOSE
        // =====================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }
    }
}
