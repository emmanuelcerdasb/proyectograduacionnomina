using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProyectoGraduacionNomina;
using ProyectoGraduacionNomina.Helpers;

namespace ProyectoGraduacionNomina.Controllers
{
    // ============================================================
    // CRUD DE CREDENCIALES - SOLO ADMINISTRADOR
    // ============================================================
    [Authorize(Roles = "Administrador")]
    public class CredencialController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ============================================================
        // MÉTODO AUXILIAR PARA DATOS DEL USUARIO ACTUAL
        // ============================================================
        private string ObtenerDatosUsuarioActual()
        {
            if (Session["CredencialId"] == null)
                return "Usuario desconocido";

            int credencialId = (int)Session["CredencialId"];

            var empleado = db.Empleado
                .Include(e => e.Persona)
                .FirstOrDefault(e => e.Credencial_idCredencial == credencialId);

            if (empleado == null)
                return $"Credencial ID: {credencialId}";

            var p = empleado.Persona;
            return $"{p.nombre} {p.apellido1} {p.apellido2} | Cédula: {p.cedula}";
        }

        // ===============================
        // INDEX
        // ===============================
        public async Task<ActionResult> Index()
        {
            var credenciales = db.Credencial.Include(c => c.Rol);
            return View(await credenciales.ToListAsync());
        }

        // ===============================
        // DETAILS
        // ===============================
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var credencial = await db.Credencial
                .Include(c => c.Rol)
                .FirstOrDefaultAsync(c => c.idCredencial == id);

            if (credencial == null)
                return HttpNotFound();

            return View(credencial);
        }

        // ===============================
        // CREATE (GET)
        // ===============================
        public ActionResult Create()
        {
            ViewBag.Rol_idRol = new SelectList(db.Rol, "idRol", "nombre");
            return View();
        }

        // ===============================
        // CREATE (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Credencial credencial)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rol_idRol = new SelectList(db.Rol, "idRol", "nombre", credencial.Rol_idRol);
                return View(credencial);
            }

            credencial.contrasena = PasswordHelper.HashPassword(credencial.contrasena);
            credencial.fecha_creacion = DateTime.Now;
            credencial.requiere_cambio = true;
            credencial.activo = true;

            db.Credencial.Add(credencial);
            await db.SaveChangesAsync();

            BitacoraHelper.Registrar(
                db,
                (int)Session["CredencialId"],
                "CREAR CREDENCIAL",
                $"Credencial creada para usuario: {credencial.usuario}. Acción realizada por: {ObtenerDatosUsuarioActual()}",
                this.HttpContext
            );

            TempData["Success"] = "Credencial creada correctamente.";
            return RedirectToAction("Index");
        }

        // ===============================
        // EDIT (GET)
        // ===============================
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var credencial = await db.Credencial.FindAsync(id);
            if (credencial == null)
                return HttpNotFound();

            ViewBag.Rol_idRol = new SelectList(db.Rol, "idRol", "nombre", credencial.Rol_idRol);
            return View(credencial);
        }

        // ===============================
        // EDIT (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Credencial model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rol_idRol = new SelectList(db.Rol, "idRol", "nombre", model.Rol_idRol);
                return View(model);
            }

            var credencial = await db.Credencial.FindAsync(model.idCredencial);
            if (credencial == null)
                return HttpNotFound();

            credencial.usuario = model.usuario;
            credencial.activo = model.activo;
            credencial.Rol_idRol = model.Rol_idRol;

            await db.SaveChangesAsync();

            BitacoraHelper.Registrar(
                db,
                (int)Session["CredencialId"],
                "EDITAR CREDENCIAL",
                $"Credencial modificada: {credencial.usuario}. Acción realizada por: {ObtenerDatosUsuarioActual()}",
                this.HttpContext
            );

            TempData["Success"] = "Credencial actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // ===============================
        // DELETE (POST)
        // ===============================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var credencial = await db.Credencial.FindAsync(id);
            if (credencial == null)
                return HttpNotFound();

            db.Credencial.Remove(credencial);
            await db.SaveChangesAsync();

            BitacoraHelper.Registrar(
                db,
                (int)Session["CredencialId"],
                "ELIMINAR CREDENCIAL",
                $"Credencial eliminada: {credencial.usuario}. Acción realizada por: {ObtenerDatosUsuarioActual()}",
                this.HttpContext
            );

            TempData["Success"] = "Credencial eliminada correctamente.";
            return RedirectToAction("Index");
        }

        // ===============================
        // RESET CONTRASEÑA (ADMIN)
        // ===============================
        public async Task<ActionResult> ResetPassword(int id)
        {
            var credencial = await db.Credencial.FindAsync(id);
            if (credencial == null)
                return HttpNotFound();

            credencial.contrasena = PasswordHelper.HashPassword("Temporal123!");
            credencial.requiere_cambio = true;
            credencial.fecha_ultimo_cambio = null;

            await db.SaveChangesAsync();

            BitacoraHelper.Registrar(
                db,
                (int)Session["CredencialId"],
                "RESET CONTRASEÑA",
                $"Contraseña reiniciada para usuario: {credencial.usuario}. Acción realizada por: {ObtenerDatosUsuarioActual()}",
                this.HttpContext
            );

            TempData["Success"] = "Contraseña reiniciada.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
