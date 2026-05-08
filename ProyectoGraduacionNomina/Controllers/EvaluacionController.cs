using ProyectoGraduacionNomina.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH")]
    public class EvaluacionController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            var evaluaciones = _db.EvaluacionPersonal
                .Include(e => e.Empleado.Persona)
                .OrderByDescending(e => e.anio)
                .ThenByDescending(e => e.semestre)
                .ThenBy(e => e.Empleado.Persona.apellido1)
                .ToList();
            return View(evaluaciones);
        }

        // =====================================================
        // CREAR (GET)
        // =====================================================
        [HttpGet]
        public ActionResult Crear()
        {
            CargarEmpleados();
            ViewBag.AnioActual = DateTime.Today.Year;
            ViewBag.SemestreActual = DateTime.Today.Month <= 6 ? 1 : 2;
            return View();
        }

        // =====================================================
        // CREAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(int empleadoId, int anio, byte semestre,
            decimal notaPuntualidad, decimal notaCalidad, decimal notaTrabajoEquipo,
            decimal notaIniciativa, decimal notaCumplimiento,
            string evaluador, string comentarios, string fechaEvaluacion)
        {
            CargarEmpleados();
            ViewBag.AnioActual = DateTime.Today.Year;
            ViewBag.SemestreActual = DateTime.Today.Month <= 6 ? 1 : 2;

            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(evaluador))
            {
                TempData["Error"] = "Debe ingresar el nombre del evaluador.";
                return View();
            }

            decimal[] notas = { notaPuntualidad, notaCalidad, notaTrabajoEquipo, notaIniciativa, notaCumplimiento };
            if (notas.Any(n => n < 1m || n > 10m))
            {
                TempData["Error"] = "Todas las notas deben estar entre 1 y 10.";
                return View();
            }

            if (!DateTime.TryParse(fechaEvaluacion, out DateTime fechaEval))
            {
                TempData["Error"] = "Fecha de evaluacion invalida.";
                return View();
            }

            bool existe = _db.EvaluacionPersonal.Any(e =>
                e.Empleado_idEmpleado == empleadoId &&
                e.anio == anio && e.semestre == semestre);

            if (existe)
            {
                TempData["Error"] = $"Ya existe una evaluacion para este empleado en ese periodo.";
                return View();
            }

            decimal notaFinal = Math.Round(
                (notaPuntualidad + notaCalidad + notaTrabajoEquipo + notaIniciativa + notaCumplimiento) / 5m, 2);

            string calificacion;
            if (notaFinal >= 9m) calificacion = "Excelente";
            else if (notaFinal >= 7m) calificacion = "Bueno";
            else if (notaFinal >= 5m) calificacion = "Regular";
            else calificacion = "Deficiente";

            try
            {
                _db.EvaluacionPersonal.Add(new EvaluacionPersonal
                {
                    Empleado_idEmpleado  = empleadoId,
                    anio                 = anio,
                    semestre             = semestre,
                    nota_puntualidad     = notaPuntualidad,
                    nota_calidad         = notaCalidad,
                    nota_trabajo_equipo  = notaTrabajoEquipo,
                    nota_iniciativa      = notaIniciativa,
                    nota_cumplimiento    = notaCumplimiento,
                    nota_final           = notaFinal,
                    calificacion         = calificacion,
                    evaluador            = evaluador.Trim(),
                    comentarios          = comentarios ?? "",
                    fecha_evaluacion     = fechaEval
                });
                _db.SaveChanges();

                if (Session["CredencialId"] != null)
                    BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                        "CREAR EVALUACION",
                        $"Evaluacion registrada: empleadoId={empleadoId} | Periodo {anio}-S{semestre} | Nota: {notaFinal:N2} ({calificacion})",
                        this.HttpContext);

                TempData["Success"] = $"Evaluacion registrada con nota final {notaFinal:N2} ({calificacion}).";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        // =====================================================
        // DETALLE
        // =====================================================
        public ActionResult Detalle(int id)
        {
            var eval = _db.EvaluacionPersonal
                .Include(e => e.Empleado.Persona)
                .Include(e => e.Empleado.Puesto)
                .FirstOrDefault(e => e.idEvaluacion == id);

            if (eval == null)
            {
                TempData["Error"] = "Evaluacion no encontrada.";
                return RedirectToAction("Index");
            }
            return View(eval);
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
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
