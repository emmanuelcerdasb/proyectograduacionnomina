using ProyectoGraduacionNomina.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TipoLiquidacionController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            return View(_db.TipoLiquidacion.OrderBy(t => t.nombre).ToList());
        }

        // =====================================================
        // CREAR (GET)
        // =====================================================
        [HttpGet]
        public ActionResult Crear()
        {
            return View();
        }

        // =====================================================
        // CREAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(string codigo, string nombre, string descripcion,
            bool aplicaCesantia, bool aplicaPreaviso)
        {
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "Código y nombre son obligatorios.";
                return View();
            }

            if (_db.TipoLiquidacion.Any(t => t.codigo == codigo.Trim()))
            {
                TempData["Error"] = "Ya existe un tipo con ese código.";
                return View();
            }

            _db.TipoLiquidacion.Add(new TipoLiquidacion
            {
                codigo           = codigo.Trim().ToUpper(),
                nombre           = nombre.Trim(),
                descripcion      = descripcion ?? "",
                aplica_cesantia  = aplicaCesantia,
                aplica_preaviso  = aplicaPreaviso
            });
            _db.SaveChanges();

            if (Session["CredencialId"] != null)
                BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                    "CREAR TIPO LIQUIDACION", $"Tipo creado: {codigo} - {nombre}", this.HttpContext);

            TempData["Success"] = "Tipo de liquidación creado correctamente.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // EDITAR (GET)
        // =====================================================
        [HttpGet]
        public ActionResult Editar(int id)
        {
            var tipo = _db.TipoLiquidacion.Find(id);
            if (tipo == null) return HttpNotFound();
            return View(tipo);
        }

        // =====================================================
        // EDITAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, string nombre, string descripcion,
            bool aplicaCesantia, bool aplicaPreaviso)
        {
            var tipo = _db.TipoLiquidacion.Find(id);
            if (tipo == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre es obligatorio.";
                return View(tipo);
            }

            tipo.nombre          = nombre.Trim();
            tipo.descripcion     = descripcion ?? "";
            tipo.aplica_cesantia = aplicaCesantia;
            tipo.aplica_preaviso = aplicaPreaviso;
            _db.SaveChanges();

            if (Session["CredencialId"] != null)
                BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                    "EDITAR TIPO LIQUIDACION", $"Tipo editado: {tipo.codigo} - {tipo.nombre}", this.HttpContext);

            TempData["Success"] = "Tipo de liquidación actualizado.";
            return RedirectToAction("Index");
        }

        // =====================================================
        // ELIMINAR (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            var tipo = _db.TipoLiquidacion.Find(id);
            if (tipo == null) return HttpNotFound();

            if (tipo.Liquidacion.Any())
            {
                TempData["Error"] = "No se puede eliminar: tiene liquidaciones asociadas.";
                return RedirectToAction("Index");
            }

            string nombre = tipo.nombre;
            _db.TipoLiquidacion.Remove(tipo);
            _db.SaveChanges();

            if (Session["CredencialId"] != null)
                BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                    "ELIMINAR TIPO LIQUIDACION", $"Tipo eliminado: {nombre}", this.HttpContext);

            TempData["Success"] = "Tipo de liquidación eliminado.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
