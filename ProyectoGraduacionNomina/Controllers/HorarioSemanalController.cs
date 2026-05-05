using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProyectoGraduacionNomina;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class HorarioSemanalController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // GET: HorarioSemanal
        public async Task<ActionResult> Index()
        {
            var horarioSemanal = db.HorarioSemanal
                .Include(h => h.Jornada);

            return View(await horarioSemanal.ToListAsync());
        }

        // GET: HorarioSemanal/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var horarioSemanal = await db.HorarioSemanal
                .Include(h => h.Jornada)
                .Include(h => h.Horario) // 👈 carga días asociados
                .FirstOrDefaultAsync(h => h.idHorarioSemanal == id);

            if (horarioSemanal == null)
                return HttpNotFound();

            return View(horarioSemanal);
        }

        // GET: HorarioSemanal/Create
        public ActionResult Create()
        {
            ViewBag.Jornada_idJornada = new SelectList(db.Jornada, "idJornada", "nombre");
            return View();
        }

        // POST: HorarioSemanal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "idHorarioSemanal,nombre,descripcion,Jornada_idJornada")]
            HorarioSemanal horarioSemanal)
        {
            if (ModelState.IsValid)
            {
                horarioSemanal.fecha_creacion = DateTime.Now;
                db.HorarioSemanal.Add(horarioSemanal);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Jornada_idJornada = new SelectList(
                db.Jornada,
                "idJornada",
                "nombre",
                horarioSemanal.Jornada_idJornada
            );

            return View(horarioSemanal);
        }

        // GET: HorarioSemanal/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var horarioSemanal = await db.HorarioSemanal.FindAsync(id);
            if (horarioSemanal == null)
                return HttpNotFound();

            ViewBag.Jornada_idJornada = new SelectList(
                db.Jornada,
                "idJornada",
                "nombre",
                horarioSemanal.Jornada_idJornada
            );

            return View(horarioSemanal);
        }

        // POST: HorarioSemanal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
    [Bind(Include = "idHorarioSemanal,nombre,descripcion,Jornada_idJornada")]
        HorarioSemanal horarioSemanal)
        {
            if (ModelState.IsValid)
            {
                var original = await db.HorarioSemanal
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.idHorarioSemanal == horarioSemanal.idHorarioSemanal);

                horarioSemanal.fecha_creacion = original.fecha_creacion;

                db.Entry(horarioSemanal).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Jornada_idJornada = new SelectList(
                db.Jornada,
                "idJornada",
                "nombre",
                horarioSemanal.Jornada_idJornada
            );

            return View(horarioSemanal);
        }


        // GET: HorarioSemanal/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var horarioSemanal = await db.HorarioSemanal.FindAsync(id);
            if (horarioSemanal == null)
                return HttpNotFound();

            return View(horarioSemanal);
        }

        // POST: HorarioSemanal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var horarioSemanal = await db.HorarioSemanal.FindAsync(id);

            // 🔒 eliminar primero los Horarios hijos
            var horarios = db.Horario
                .Where(h => h.HorarioSemanal_idHorarioSemanal == id);

            db.Horario.RemoveRange(horarios);
            db.HorarioSemanal.Remove(horarioSemanal);

            await db.SaveChangesAsync();
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
