using ProyectoGraduacionNomina.Helpers;
using ProyectoGraduacionNomina.Servicios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,RRHH")]
    public class LiquidacionController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();
        private LiquidacionService _service;

        public LiquidacionController()
        {
            _service = new LiquidacionService(new BD_NominaEntities());
        }

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            var liquidaciones = _service.ObtenerTodas();
            return View(liquidaciones);
        }

        // =====================================================
        // CALCULAR (GET)
        // =====================================================
        [HttpGet]
        public ActionResult Calcular()
        {
            CargarEmpleados();
            CargarTipos();
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        // =====================================================
        // CALCULAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calcular(int empleadoId, int tipoLiquidacionId,
            string fechaLiquidacion, string observaciones)
        {
            CargarEmpleados();
            CargarTipos();
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-dd");

            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado.";
                return View();
            }

            if (tipoLiquidacionId <= 0)
            {
                TempData["Error"] = "Debe seleccionar el tipo de liquidacion.";
                return View();
            }

            if (!DateTime.TryParse(fechaLiquidacion, out DateTime fecha))
            {
                TempData["Error"] = "Fecha de liquidacion invalida.";
                return View();
            }

            if (fecha > DateTime.Today)
            {
                TempData["Error"] = "La fecha de liquidacion no puede ser una fecha futura.";
                return View();
            }

            try
            {
                var resultado = _service.CalcularLiquidacion(
                    empleadoId, tipoLiquidacionId, fecha, observaciones ?? "");
                return View("DetalleLiquidacion", resultado);
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
        public ActionResult Guardar(int empleadoId, int tipoLiquidacionId,
            string fechaLiquidacion, string observaciones)
        {
            try
            {
                if (!DateTime.TryParse(fechaLiquidacion, out DateTime fecha))
                {
                    TempData["Error"] = "Fecha de liquidacion invalida.";
                    return RedirectToAction("Calcular");
                }

                if (fecha > DateTime.Today)
                {
                    TempData["Error"] = "La fecha de liquidacion no puede ser una fecha futura.";
                    return RedirectToAction("Calcular");
                }

                var resultado = _service.CalcularLiquidacion(
                    empleadoId, tipoLiquidacionId, fecha, observaciones ?? "");
                _service.GuardarLiquidacion(resultado);

                if (Session["CredencialId"] != null)
                    BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                        "GUARDAR LIQUIDACION",
                        $"Liquidacion guardada: {resultado.NombreEmpleado} | Tipo: {resultado.NombreTipoLiquidacion} | Neto: {resultado.NetoPagar:N2}",
                        this.HttpContext);

                TempData["Success"] = $"Liquidacion de {resultado.NombreEmpleado} guardada correctamente.";
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

        private void CargarTipos()
        {
            ViewBag.TipoLiquidacionId = _db.TipoLiquidacion
                .Select(t => new SelectListItem
                {
                    Value = t.idTipoLiquidacion.ToString(),
                    Text  = t.nombre
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
