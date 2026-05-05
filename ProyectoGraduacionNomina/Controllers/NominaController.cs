using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProyectoGraduacionNomina.Servicios;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH,Jefe,Jefa")]
    public class NominaController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // MENÚ / FORMULARIO DE CÁLCULO DE NÓMINA
        // =====================================================
        [HttpGet]
        public ActionResult CalcularNomina()
        {
            CargarEmpleados();
            return View();
        }

        // =====================================================
        // PROCESO DE CÁLCULO DE NÓMINA
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CalcularNomina(int empleadoId, int mes, int anno)
        {
            CargarEmpleados();

            // -----------------------------
            // VALIDACIONES
            // -----------------------------
            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                return View();
            }

            if (mes < 1 || mes > 12)
            {
                TempData["Error"] = "Mes inválido.";
                return View();
            }

            if (anno < 2000 || anno > DateTime.Now.Year + 1)
            {
                TempData["Error"] = "Año inválido.";
                return View();
            }

            try
            {
                DateTime fechaInicio = new DateTime(anno, mes, 1);
                DateTime fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

                var service = new NominaService(_db);
                var resultado = service.CalcularNominaEmpleado(
                    empleadoId,
                    fechaInicio,
                    fechaFin
                );

                return View("DetalleNomina", resultado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        // =====================================================
        // MÉTODOS AUXILIARES
        // =====================================================
        private void CargarEmpleados()
        {
            ViewBag.EmpleadoId = _db.Empleado
                .Include(e => e.Persona)
                .Where(e => e.estado == "Activo")
                .Select(e => new SelectListItem
                {
                    Value = e.idEmpleado.ToString(),
                    Text = e.Persona.nombre + " " +
                           e.Persona.apellido1 + " " +
                           e.Persona.apellido2
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
