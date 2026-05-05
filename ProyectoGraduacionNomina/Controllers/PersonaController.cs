using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoGraduacionNomina;
using ProyectoGraduacionNomina.Helpers;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,Colaborador")]
    public class PersonaController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ============================================================
        // INDEX
        // ============================================================
        public ActionResult Index()
        {
            var persona = db.Persona
                .Include(p => p.TipoCedula)
                .Include(p => p.Direccion)
                .Include(p => p.Direccion.Distrito)
                .Include(p => p.Direccion.Distrito.Canton)
                .Include(p => p.Direccion.Distrito.Canton.Provincia);

            return View(persona.ToList());
        }

        // ============================================================
        // DETAILS
        // ============================================================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var persona = db.Persona
                .Include(p => p.TipoCedula)
                .Include(p => p.Direccion)
                .Include(p => p.Direccion.Distrito.Canton.Provincia)
                .FirstOrDefault(p => p.idPersona == id);

            if (persona == null)
                return HttpNotFound();

            return View(persona);
        }

        // ============================================================
        // CREATE GET
        // ============================================================
        public ActionResult Create()
        {
            ViewBag.Cedula_idCedula = new SelectList(db.TipoCedula, "idTipoCedula", "tipo_cedula");
            ViewBag.Provincia_id = new SelectList(db.Provincia, "idProvincia", "nombre");
            ViewBag.Canton_id = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.Distrito_id = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(new Persona { fecha_creacion = DateTime.Now });
        }

        // ============================================================
        // CREATE POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Persona persona, int? Provincia_id, int? Canton_id, int? Distrito_id, string DetalleDireccion)
        {
            if (!Provincia_id.HasValue || Provincia_id.Value == 0)
                ModelState.AddModelError("Provincia_id", "Debe seleccionar una provincia válida.");

            if (!Canton_id.HasValue || Canton_id.Value == 0)
                ModelState.AddModelError("Canton_id", "Debe seleccionar un cantón válido.");

            if (!Distrito_id.HasValue || Distrito_id.Value == 0)
                ModelState.AddModelError("Distrito_id", "Debe seleccionar un distrito válido.");

            if (string.IsNullOrWhiteSpace(DetalleDireccion))
                ModelState.AddModelError("DetalleDireccion", "Debe escribir el detalle de la dirección.");

            if (!string.IsNullOrWhiteSpace(persona?.cedula))
            {
                if (db.Persona.Any(p => p.cedula == persona.cedula))
                    ModelState.AddModelError("cedula", "Ya existe una persona con esa cédula.");
            }

            if (!string.IsNullOrWhiteSpace(persona?.correo))
            {
                if (db.Persona.Any(p => p.correo == persona.correo))
                    ModelState.AddModelError("correo", "Ya existe una persona con ese correo.");
            }

            if (ModelState.IsValid)
            {
                var direccion = new Direccion
                {
                    Distrito_idDistrito = Distrito_id.Value,
                    detalle = DetalleDireccion
                };

                db.Direccion.Add(direccion);
                db.SaveChanges();

                persona.Direccion_idDireccion = direccion.idDireccion;
                persona.fecha_creacion = DateTime.Now;

                db.Persona.Add(persona);
                db.SaveChanges();

                // ========= AUDITORÍA =========
                BitacoraHelper.Registrar(
                    db,
                    (int)Session["CredencialId"],
                    "CREAR PERSONA",
                    $"Creó persona {persona.nombre} {persona.apellido1} ({persona.cedula})",
                    HttpContext
                );

                TempData["Success"] = "Persona creada correctamente.";
                return RedirectToAction("Index");
            }

            CargarCombosDireccion(Provincia_id ?? 0, Canton_id ?? 0, Distrito_id ?? 0);
            ViewBag.Cedula_idCedula = new SelectList(db.TipoCedula, "idTipoCedula", "tipo_cedula", persona?.Cedula_idCedula);

            return View(persona);
        }

        // ============================================================
        // EDIT GET
        // ============================================================
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var persona = db.Persona
                .Include(p => p.Direccion)
                .Include(p => p.Direccion.Distrito.Canton.Provincia)
                .FirstOrDefault(p => p.idPersona == id);

            if (persona == null)
                return HttpNotFound();

            bool identidadCongelada = db.Empleado.Any(e => e.Persona_idPersona == persona.idPersona);
            ViewBag.IdentidadCongelada = identidadCongelada;

            int provincia = persona.Direccion?.Distrito?.Canton?.Provincia?.idProvincia ?? 0;
            int canton = persona.Direccion?.Distrito?.Canton?.idCanton ?? 0;
            int distrito = persona.Direccion?.Distrito?.idDistrito ?? 0;

            CargarCombosDireccion(provincia, canton, distrito);
            ViewBag.Cedula_idCedula = new SelectList(db.TipoCedula, "idTipoCedula", "tipo_cedula", persona.Cedula_idCedula);
            ViewBag.DetalleDireccion = persona.Direccion?.detalle ?? "";

            return View(persona);
        }

        // ============================================================
        // EDIT POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Persona persona, int? Provincia_id, int? Canton_id, int? Distrito_id, string DetalleDireccion)
        {
            var personaDB = db.Persona
                .Include(p => p.Direccion)
                .FirstOrDefault(p => p.idPersona == persona.idPersona);

            if (personaDB == null)
                return HttpNotFound();

            bool identidadCongelada = db.Empleado.Any(e => e.Persona_idPersona == persona.idPersona);

            if (!ModelState.IsValid)
            {
                CargarCombosDireccion(Provincia_id ?? 0, Canton_id ?? 0, Distrito_id ?? 0);
                return View(persona);
            }

            if (!identidadCongelada)
            {
                personaDB.nombre = persona.nombre;
                personaDB.apellido1 = persona.apellido1;
                personaDB.apellido2 = persona.apellido2;
                personaDB.cedula = persona.cedula;
                personaDB.Cedula_idCedula = persona.Cedula_idCedula;
                personaDB.fecha_nacimiento = persona.fecha_nacimiento;
            }

            personaDB.correo = persona.correo;
            personaDB.telefono = persona.telefono;

            var direccion = db.Direccion.Find(personaDB.Direccion_idDireccion);
            direccion.detalle = DetalleDireccion;
            direccion.Distrito_idDistrito = Distrito_id.Value;

            db.SaveChanges();

            // ========= AUDITORÍA =========
            BitacoraHelper.Registrar(
                db,
                (int)Session["CredencialId"],
                "EDIT",
                $"Editó persona {personaDB.nombre} {personaDB.apellido1} ({personaDB.cedula})",
                HttpContext
            );

            TempData["Success"] = "Persona editada correctamente.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // PERFIL PERSONAL (GET)
        // ============================================================
        [Authorize]
        public ActionResult Perfil()
        {
            int credencialId = (int)Session["CredencialId"];

            var persona = db.Empleado
                .Include(e => e.Persona)
                .Where(e => e.Credencial_idCredencial == credencialId)
                .Select(e => e.Persona)
                .FirstOrDefault();

            if (persona == null)
                return HttpNotFound();

            return View("Edit", persona);
        }

        // ============================================================
        // PERFIL PERSONAL (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Perfil(Persona persona)
        {
            int credencialId = (int)Session["CredencialId"];

            var personaDB = db.Empleado
                .Include(e => e.Persona)
                .Where(e => e.Credencial_idCredencial == credencialId)
                .Select(e => e.Persona)
                .FirstOrDefault();

            if (personaDB == null)
                return HttpNotFound();

            personaDB.correo = persona.correo;
            personaDB.telefono = persona.telefono;

            db.SaveChanges();

            BitacoraHelper.Registrar(
                db,
                credencialId,
                "PERFIL",
                "Actualizó su perfil personal",
                HttpContext
            );

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }

        // ============================================================
        // AJAX
        // ============================================================
        public JsonResult GetCantones(int idProvincia)
        {
            var cantones = db.Canton
                .Where(c => c.Provincia_idProvincia == idProvincia)
                .Select(c => new { c.idCanton, c.nombre })
                .ToList();

            return Json(cantones, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDistritos(int idCanton)
        {
            var distritos = db.Distrito
                .Where(d => d.Canton_idCanton == idCanton)
                .Select(d => new { d.idDistrito, d.nombre })
                .ToList();

            return Json(distritos, JsonRequestBehavior.AllowGet);
        }

        private void CargarCombosDireccion(int provincia, int canton, int distrito)
        {
            ViewBag.Provincia_id = new SelectList(db.Provincia, "idProvincia", "nombre", provincia);
            ViewBag.Canton_id = new SelectList(db.Canton.Where(c => c.Provincia_idProvincia == provincia), "idCanton", "nombre", canton);
            ViewBag.Distrito_id = new SelectList(db.Distrito.Where(d => d.Canton_idCanton == canton), "idDistrito", "nombre", distrito);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
