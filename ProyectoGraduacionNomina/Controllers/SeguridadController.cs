using ProyectoGraduacionNomina.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SeguridadController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // INDEX — hub de seguridad (vista ya existente)
        // =====================================================
        public ActionResult Index()
        {
            return View();
        }

        // =====================================================
        // SESIONES ACTIVAS
        // =====================================================
        public ActionResult SesionesActivas()
        {
            var sesiones = _db.SesionUsuario
                .Include(s => s.Credencial)
                .Include(s => s.Credencial.Rol)
                .Where(s => s.activo)
                .OrderByDescending(s => s.fecha_inicio)
                .ToList();

            return View(sesiones);
        }

        // =====================================================
        // CERRAR SESIÓN DE OTRO USUARIO (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CerrarSesion(int id)
        {
            var sesion = _db.SesionUsuario.Find(id);
            if (sesion == null)
            {
                TempData["Error"] = "Sesión no encontrada.";
                return RedirectToAction("SesionesActivas");
            }

            sesion.activo    = false;
            sesion.fecha_fin = DateTime.Now;
            _db.SaveChanges();

            if (Session["CredencialId"] != null)
                BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                    "CERRAR SESION",
                    $"Sesión idSesion={id} cerrada manualmente por administrador.",
                    this.HttpContext);

            TempData["Success"] = "Sesión cerrada correctamente.";
            return RedirectToAction("SesionesActivas");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
