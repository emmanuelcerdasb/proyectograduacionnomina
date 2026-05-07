using ProyectoGraduacionNomina.Servicios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH")]
    public class AguinaldoController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();
        private AguinaldoService _service;

        public AguinaldoController()
        {
            _service = new AguinaldoService(new BD_NominaEntities());
        }

        // =====================================================
        // INDEX — lista de aguinaldos guardados
        // =====================================================
        public ActionResult Index()
        {
            var aguinaldos = _service.ObtenerTodos();
            return View(aguinaldos);
        }

        // =====================================================
        // CALCULAR (GET)
        // =====================================================
        [HttpGet]
        public ActionResult Calcular()
        {
            CargarEmpleados();
            ViewBag.AnioActual = DateTime.Today.Year;
            return View();
        }

        // =====================================================
        // CALCULAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calcular(int empleadoId, int anio)
        {
            CargarEmpleados();
            ViewBag.AnioActual = DateTime.Today.Year;

            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                return View();
            }

            if (anio < 2000 || anio > DateTime.Now.Year + 1)
            {
                TempData["Error"] = "Año invalido.";
                return View();
            }

            try
            {
                var resultado = _service.CalcularAguinaldo(empleadoId, anio);
                return View("DetalleAguinaldo", resultado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        // =====================================================
        // GUARDAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(int empleadoId, int anio)
        {
            try
            {
                var resultado = _service.CalcularAguinaldo(empleadoId, anio);
                _service.GuardarAguinaldo(resultado);
                TempData["Success"] = $"Aguinaldo de {resultado.NombreEmpleado} ({anio}) guardado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
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
