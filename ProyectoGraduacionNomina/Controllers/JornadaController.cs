using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoGraduacionNomina;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class JornadaController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // GET: Jornada
        public async Task<ActionResult> Index()
        {
            return View(await db.Jornada.ToListAsync());
        }

        // GET: Jornada/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jornada jornada = await db.Jornada.FindAsync(id);
            if (jornada == null)
            {
                return HttpNotFound();
            }
            return View(jornada);
        }

        // GET: Jornada/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Jornada/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idJornada,nombre,horas_semanales,descripcion")] Jornada jornada)
        {
            if (ModelState.IsValid)
            {
                db.Jornada.Add(jornada);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(jornada);
        }

        // GET: Jornada/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jornada jornada = await db.Jornada.FindAsync(id);
            if (jornada == null)
            {
                return HttpNotFound();
            }
            return View(jornada);
        }

        // POST: Jornada/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idJornada,nombre,horas_semanales,descripcion")] Jornada jornada)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jornada).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(jornada);
        }

        // GET: Jornada/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jornada jornada = await db.Jornada.FindAsync(id);
            if (jornada == null)
            {
                return HttpNotFound();
            }
            return View(jornada);
        }

        // POST: Jornada/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Jornada jornada = await db.Jornada.FindAsync(id);
            db.Jornada.Remove(jornada);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
