using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoGraduacionNomina;
using ProyectoGraduacionNomina.Helpers;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,Jefa,Jefe,Colaborador")]
    public class EmpleadoController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ============================================================
        // INDEX
        // ============================================================
        public async Task<ActionResult> Index()
        {
            var empleado = db.Empleado
                .Include(e => e.Credencial)
                .Include(e => e.Direccion)
                .Include(e => e.Jornada)
                .Include(e => e.Persona)
                .Include(e => e.Puesto)
                .Include(e => e.HorarioSemanal);

            return View(await empleado.ToListAsync());
        }

        // ============================================================
        // DETAILS
        // ============================================================
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Empleado empleado = await db.Empleado
                .Include(e => e.Persona)
                .Include(e => e.HorarioSemanal)
                .FirstOrDefaultAsync(e => e.idEmpleado == id);

            if (empleado == null)
                return HttpNotFound();

            return View(empleado);
        }

        // ============================================================
        // CREATE GET
        // ============================================================
        public ActionResult Create()
        {
            ViewBag.Credencial_idCredencial = new SelectList(db.Credencial, "idCredencial", "usuario");
            ViewBag.Direccion_idDireccion = new SelectList(db.Direccion, "idDireccion", "detalle");
            ViewBag.Jornada_idJornada = new SelectList(db.Jornada, "idJornada", "nombre");
            ViewBag.Persona_idPersona = new SelectList(db.Persona, "idPersona", "nombre");
            ViewBag.Puesto_idPuesto = new SelectList(db.Puesto, "idPuesto", "nombre");
            ViewBag.HorarioSemanal_idHorarioSemanal = new SelectList(db.HorarioSemanal, "idHorarioSemanal", "nombre");

            return View();
        }

        // ============================================================
        // CREATE POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include =
                "idEmpleado," +
                "Persona_idPersona," +
                "Credencial_idCredencial," +
                "Puesto_idPuesto," +
                "Jornada_idJornada," +
                "HorarioSemanal_idHorarioSemanal," +
                "fecha_ingreso," +
                "fecha_salida," +
                "estado," +
                "Direccion_idDireccion")]
            Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                db.Empleado.Add(empleado);
                await db.SaveChangesAsync();

                // ================= AUDITORÍA =================
                var persona = await db.Persona
                    .FirstOrDefaultAsync(p => p.idPersona == empleado.Persona_idPersona);

                if (persona != null && Session["CredencialId"] != null)
                {
                    BitacoraHelper.Registrar(
                        db,
                        (int)Session["CredencialId"],
                        "CREAR EMPLEADO",
                        $"Creó empleado: {persona.nombre} {persona.apellido1} {persona.apellido2} | Cédula: {persona.cedula}",
                        HttpContext
                    );
                }

                return RedirectToAction("Index");
            }

            // Re-cargar combos
            ViewBag.Credencial_idCredencial = new SelectList(db.Credencial, "idCredencial", "usuario", empleado.Credencial_idCredencial);
            ViewBag.Direccion_idDireccion = new SelectList(db.Direccion, "idDireccion", "detalle", empleado.Direccion_idDireccion);
            ViewBag.Jornada_idJornada = new SelectList(db.Jornada, "idJornada", "nombre", empleado.Jornada_idJornada);
            ViewBag.Persona_idPersona = new SelectList(db.Persona, "idPersona", "nombre", empleado.Persona_idPersona);
            ViewBag.Puesto_idPuesto = new SelectList(db.Puesto, "idPuesto", "nombre", empleado.Puesto_idPuesto);
            ViewBag.HorarioSemanal_idHorarioSemanal = new SelectList(
                db.HorarioSemanal,
                "idHorarioSemanal",
                "nombre",
                empleado.HorarioSemanal_idHorarioSemanal
            );

            return View(empleado);
        }

        // ============================================================
        // EDIT GET
        // ============================================================
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Empleado empleado = await db.Empleado.FindAsync(id);
            if (empleado == null)
                return HttpNotFound();

            ViewBag.Credencial_idCredencial = new SelectList(db.Credencial, "idCredencial", "usuario", empleado.Credencial_idCredencial);
            ViewBag.Direccion_idDireccion = new SelectList(db.Direccion, "idDireccion", "detalle", empleado.Direccion_idDireccion);
            ViewBag.Jornada_idJornada = new SelectList(db.Jornada, "idJornada", "nombre", empleado.Jornada_idJornada);
            ViewBag.Persona_idPersona = new SelectList(db.Persona, "idPersona", "nombre", empleado.Persona_idPersona);
            ViewBag.Puesto_idPuesto = new SelectList(db.Puesto, "idPuesto", "nombre", empleado.Puesto_idPuesto);
            ViewBag.HorarioSemanal_idHorarioSemanal = new SelectList(
                db.HorarioSemanal,
                "idHorarioSemanal",
                "nombre",
                empleado.HorarioSemanal_idHorarioSemanal
            );

            return View(empleado);
        }

        // ============================================================
        // EDIT POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind(Include =
                "idEmpleado," +
                "Persona_idPersona," +
                "Credencial_idCredencial," +
                "Puesto_idPuesto," +
                "Jornada_idJornada," +
                "HorarioSemanal_idHorarioSemanal," +
                "fecha_ingreso," +
                "fecha_salida," +
                "estado," +
                "Direccion_idDireccion")]
            Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                db.Entry(empleado).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(empleado);
        }

        // ============================================================
        // DELETE GET
        // ============================================================
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Empleado empleado = await db.Empleado
                .Include(e => e.Persona)
                .Include(e => e.Puesto)
                .Include(e => e.HorarioSemanal)
                .FirstOrDefaultAsync(e => e.idEmpleado == id);

            if (empleado == null)
                return HttpNotFound();

            return View(empleado);
        }

        // ============================================================
        // DELETE POST
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Empleado empleado = await db.Empleado
                .Include(e => e.Persona)
                .FirstOrDefaultAsync(e => e.idEmpleado == id);

            if (empleado == null)
                return HttpNotFound();

            // ================= AUDITORÍA =================
            if (empleado.Persona != null && Session["CredencialId"] != null)
            {
                BitacoraHelper.Registrar(
                    db,
                    (int)Session["CredencialId"],
                    "ELIMINAR EMPLEADO",
                    $"Eliminó empleado: {empleado.Persona.nombre} {empleado.Persona.apellido1} {empleado.Persona.apellido2} | Cédula: {empleado.Persona.cedula}",
                    HttpContext
                );
            }

            db.Empleado.Remove(empleado);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // ============================================================
        // DISPOSE
        // ============================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
