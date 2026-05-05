using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProyectoGraduacionNomina;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class HorarioController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ===============================
        // GET: Horario
        // ===============================
        public async Task<ActionResult> Index()
        {
            var horarios = db.Horario.Include(h => h.HorarioSemanal);
            return View(await horarios.ToListAsync());
        }

        // ===============================
        // GET: Horario/Details/5
        // ===============================
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var horario = await db.Horario.FindAsync(id);

            if (horario == null)
                return HttpNotFound();

            return View(horario);
        }

        // ===============================
        // GET: Horario/Create
        // ===============================
        public ActionResult Create(int? horarioSemanalId)
        {
            if (horarioSemanalId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var horarioSemanal = db.HorarioSemanal.Find(horarioSemanalId);
            if (horarioSemanal == null)
                return HttpNotFound();

            var horario = new Horario
            {
                HorarioSemanal_idHorarioSemanal = horarioSemanal.idHorarioSemanal
            };

            ViewBag.HorarioSemanalNombre = horarioSemanal.nombre;

            return View(horario);
        }

        // ===============================
        // POST: Horario/Create
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "idHorario,dia_semana,hora_inicio,hora_fin,HorarioSemanal_idHorarioSemanal")]
            Horario horario)
        {
            if (ModelState.IsValid)
            {
                db.Horario.Add(horario);
                await db.SaveChangesAsync();

                return RedirectToAction(
                    "Details",
                    "HorarioSemanal",
                    new { id = horario.HorarioSemanal_idHorarioSemanal }
                );
            }

            return View(horario);
        }

        // ===============================
        // GET: Horario/Edit/5
        // ===============================
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Horario horario = await db.Horario
                .Include(h => h.HorarioSemanal)
                .FirstOrDefaultAsync(h => h.idHorario == id);

            if (horario == null)
                return HttpNotFound();

            ViewBag.HorarioSemanalNombre = horario.HorarioSemanal?.nombre;

            return View(horario);
        }


        // ===============================
        // POST: Horario/Edit/5
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind(Include = "idHorario,dia_semana,hora_inicio,hora_fin,HorarioSemanal_idHorarioSemanal")]
            Horario horario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(horario).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction(
                    "Details",
                    "HorarioSemanal",
                    new { id = horario.HorarioSemanal_idHorarioSemanal }
                );
            }

            return View(horario);
        }

        // ===============================
        // GET: Horario/Delete/5
        // ===============================
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Horario horario = await db.Horario
                .Include(h => h.HorarioSemanal)
                .FirstOrDefaultAsync(h => h.idHorario == id);

            if (horario == null)
                return HttpNotFound();

            ViewBag.HorarioSemanalNombre = horario.HorarioSemanal?.nombre;

            return View(horario);
        }

        // ===============================
        // POST: Horario/Delete/5
        // ===============================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var horario = await db.Horario.FindAsync(id);

            if (horario == null)
                return HttpNotFound();

            if (!horario.HorarioSemanal_idHorarioSemanal.HasValue)
                return RedirectToAction("Index");

            int horarioSemanalId = horario.HorarioSemanal_idHorarioSemanal.Value;

            db.Horario.Remove(horario);
            await db.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "HorarioSemanal",
                new { id = horarioSemanalId }
            );
        }

        // ===============================
        // Dispose
        // ===============================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
