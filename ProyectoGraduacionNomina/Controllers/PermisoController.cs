using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize]
    public class PermisoController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // =========================================================
        // INDEX
        // =========================================================
        public ActionResult Index()
        {
            return View();
        }

        // =========================================================
        // MIS SOLICITUDES (COLABORADOR)
        // =========================================================
        public ActionResult MisSolicitudes()
        {
            int empleadoId = ObtenerEmpleadoIdSesion();

            var permisos = _db.Permiso
                .Include(p => p.TipoPermiso)
                .Where(p => p.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(p => p.fecha_registro)
                .ToList();

            return View(permisos);
        }

        // =========================================================
        // SOLICITUDES PENDIENTES (JEFATURA / ADMIN)
        // =========================================================
        [Authorize(Roles = "Administrador,Jefa,Jefe,RRHH")]
        public ActionResult SolicitudesPendientes()
        {
            var permisos = _db.Permiso
                .Include(p => p.Empleado)
                .Include(p => p.TipoPermiso)
                .Where(p => p.estado == "Pendiente")
                .OrderBy(p => p.fecha_registro)
                .ToList();

            return View(permisos);
        }


        // =========================================================
        // APROBAR PERMISO
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Jefa,Jefe,RRHH")]
        [ValidateAntiForgeryToken]
        public ActionResult Aprobar(int id)
        {
            var permiso = _db.Permiso.Find(id);

            if (permiso == null)
                return HttpNotFound();

            if (permiso.estado != "Pendiente")
                return RedirectToAction("SolicitudesPendientes");

            permiso.estado = "Aprobado";
            permiso.fecha_actualizacion = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Permiso aprobado correctamente.";
            return RedirectToAction("SolicitudesPendientes");
        }

        // =========================================================
        // RECHAZAR PERMISO
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Jefa,Jefe,RRHH")]
        [ValidateAntiForgeryToken]
        public ActionResult Rechazar(int id)
        {
            var permiso = _db.Permiso.Find(id);

            if (permiso == null)
                return HttpNotFound();

            if (permiso.estado != "Pendiente")
                return RedirectToAction("SolicitudesPendientes");

            permiso.estado = "Rechazado";
            permiso.fecha_actualizacion = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Permiso rechazado.";
            return RedirectToAction("SolicitudesPendientes");
        }


        // =========================================================
        // CREAR SOLICITUD (GET)
        // =========================================================
        public ActionResult CrearSolicitudPermiso()
        {
            CargarTiposPermiso();
            return View();
        }

        // =========================================================
        // CREAR SOLICITUD (POST)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearSolicitudPermiso(Permiso permiso)
        {
            if (ModelState.IsValid)
            {
                permiso.Empleado_idEmpleado = ObtenerEmpleadoIdSesion();
                permiso.estado = "Pendiente";
                permiso.fecha_registro = DateTime.Now;

                _db.Permiso.Add(permiso);
                _db.SaveChanges();

                return RedirectToAction("MisSolicitudes");
            }

            CargarTiposPermiso();
            return View(permiso);
        }

        // =========================================================
        // MÉTODOS AUXILIARES
        // =========================================================
        private void CargarTiposPermiso()
        {
            ViewBag.TipoPermiso_idTipoPermiso = _db.TipoPermiso
                .Select(t => new SelectListItem
                {
                    Value = t.idTipoPermiso.ToString(),
                    Text = t.nombre + " | (" +
                        (t.es_administrativo
                            ? "Administrativo"
                            : (t.tiene_goce ? "Con goce de salario" : "Sin goce de salario")
                        ) + ")"
                })
                .ToList();
        }

        private int ObtenerEmpleadoIdSesion()
        {
            // 🔴 Ajustá el nombre si tu sesión usa otro identificador
            if (Session["EmpleadoId"] == null)
            {
                throw new Exception("No se encontró el EmpleadoId en sesión.");
            }

            return Convert.ToInt32(Session["EmpleadoId"]);
        }

        // =========================================================
        // DISPOSE
        // =========================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
