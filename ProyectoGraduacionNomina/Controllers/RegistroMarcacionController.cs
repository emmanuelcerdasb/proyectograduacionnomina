using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProyectoGraduacionNomina;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,Jefa,Jefe,Colaborador")]
    public class RegistroMarcacionController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ============================================================
        // INDEX
        // ============================================================
        public async Task<ActionResult> Index()
        {
            var registros = db.RegistroMarcacion
                .Include(r => r.Empleado)
                .Include(r => r.Empleado.Persona)
                .OrderByDescending(r => r.fecha_registro);

            return View(await registros.ToListAsync());
        }

        // ============================================================
        // DETAILS
        // ============================================================
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registro = await db.RegistroMarcacion
                .Include(r => r.Empleado)
                .Include(r => r.Empleado.Persona)
                .FirstOrDefaultAsync(r => r.idRegistroMarcacion == id);

            if (registro == null) return HttpNotFound();

            return View(registro);
        }

        // ============================================================
        // CREATE GET
        // ============================================================
        public ActionResult Create()
        {
            ViewBag.Empleado_idEmpleado = new SelectList(
                db.Empleado.Include(e => e.Persona).ToList()
                    .Select(e => new
                    {
                        e.idEmpleado,
                        Nombre = e.Persona.nombre + " " + e.Persona.apellido1 + " " + e.Persona.apellido2 + " — " + e.Persona.cedula
                    }),
                "idEmpleado",
                "Nombre"
            );

            return View(new RegistroMarcacion
            {
                fecha = DateTime.Now.Date,
                hora = DateTime.Now.TimeOfDay
            });
        }

        // ============================================================
        // CREATE POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegistroMarcacion registro)
        {
            // Validación básica
            if (registro.Empleado_idEmpleado <= 0)
                ModelState.AddModelError("Empleado_idEmpleado", "Debe seleccionar un empleado.");

            if (registro.fecha == default(DateTime))
                ModelState.AddModelError("fecha", "Debe seleccionar una fecha.");

            if (registro.hora == null || registro.hora == TimeSpan.Zero)
                ModelState.AddModelError("hora", "Debe seleccionar una hora.");

            if (string.IsNullOrWhiteSpace(registro.tipo))
                ModelState.AddModelError("tipo", "Debe seleccionar el tipo de marcación (Entrada/Salida).");

            // Validar duplicados exactos
            bool duplicado = await db.RegistroMarcacion.AnyAsync(r =>
                r.Empleado_idEmpleado == registro.Empleado_idEmpleado &&
                DbFunctions.TruncateTime(r.fecha) == registro.fecha.Date &&
                r.hora == registro.hora &&
                r.tipo == registro.tipo
            );

            if (duplicado)
                ModelState.AddModelError("", "Ya existe una marcación idéntica para este empleado.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                ViewBag.Empleado_idEmpleado = new SelectList(
                    db.Empleado.Include(e => e.Persona).ToList()
                        .Select(e => new
                        {
                            e.idEmpleado,
                            Nombre = e.Persona.nombre + " " + e.Persona.apellido1 + " " + e.Persona.apellido2 + " — " + e.Persona.cedula
                        }),
                    "idEmpleado",
                    "Nombre",
                    registro.Empleado_idEmpleado
                );

                return View(registro);
            }

            registro.fecha_registro = DateTime.Now;

            db.RegistroMarcacion.Add(registro);
            await db.SaveChangesAsync();

            TempData["Success"] = "Marcación registrada correctamente.";
            return RedirectToAction("Index");
        }


        // ============================================================
        // EDIT GET
        // (YA NO SE ENVÍA UN SELECTLIST)
        // ============================================================
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            RegistroMarcacion registro = await db.RegistroMarcacion
                .Include(r => r.Empleado)
                .Include(r => r.Empleado.Persona)
                .FirstOrDefaultAsync(r => r.idRegistroMarcacion == id);

            if (registro == null) return HttpNotFound();

            return View(registro);
        }

        // ============================================================
        // EDIT POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RegistroMarcacion registro)
        {
            // Validación
            if (registro.Empleado_idEmpleado <= 0)
                ModelState.AddModelError("Empleado_idEmpleado", "Empleado inválido.");

            if (registro.fecha == default(DateTime))
                ModelState.AddModelError("fecha", "Debe seleccionar una fecha.");

            if (registro.hora == null || registro.hora == TimeSpan.Zero)
                ModelState.AddModelError("hora", "Debe seleccionar una hora.");

            if (string.IsNullOrWhiteSpace(registro.tipo))
                ModelState.AddModelError("tipo", "Debe seleccionar el tipo de marcación.");

            // Detectar duplicados sin incluirse a sí mismo
            bool duplicado = await db.RegistroMarcacion.AnyAsync(r =>
                r.idRegistroMarcacion != registro.idRegistroMarcacion &&
                r.Empleado_idEmpleado == registro.Empleado_idEmpleado &&
                DbFunctions.TruncateTime(r.fecha) == registro.fecha.Date &&
                r.hora == registro.hora &&
                r.tipo == registro.tipo
            );

            if (duplicado)
                ModelState.AddModelError("", "Ya existe una marcación idéntica para este empleado.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                // Obtener el empleado para mostrarlo en la vista
                registro.Empleado = db.Empleado
                    .Include(e => e.Persona)
                    .FirstOrDefault(e => e.idEmpleado == registro.Empleado_idEmpleado);

                return View(registro);
            }

            var registroDB = await db.RegistroMarcacion.FindAsync(registro.idRegistroMarcacion);

            if (registroDB == null) return HttpNotFound();

            registroDB.Empleado_idEmpleado = registro.Empleado_idEmpleado;
            registroDB.fecha = registro.fecha;
            registroDB.hora = registro.hora;
            registroDB.tipo = registro.tipo;
            registroDB.origen = registro.origen;
            registroDB.observacion = registro.observacion;

            db.Entry(registroDB).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["Success"] = "Marcación actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // DELETE GET
        // ============================================================
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            RegistroMarcacion registro = await db.RegistroMarcacion
                .Include(r => r.Empleado)
                .Include(r => r.Empleado.Persona)
                .FirstOrDefaultAsync(r => r.idRegistroMarcacion == id);

            if (registro == null) return HttpNotFound();

            return View(registro);
        }

        // ============================================================
        // DELETE POST
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RegistroMarcacion registro = await db.RegistroMarcacion.FindAsync(id);

            if (registro == null)
            {
                TempData["Error"] = "Marcación no encontrada.";
                return RedirectToAction("Index");
            }

            db.RegistroMarcacion.Remove(registro);
            await db.SaveChangesAsync();

            TempData["Success"] = "Marcación eliminada correctamente.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // DISPOSE
        // ============================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
