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
    [Authorize(Roles = "Administrador,Colaborador")]
    public class PuestoController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // GET: Puesto
        public async Task<ActionResult> Index()
        {
            // Guardar estado previo y desactivar creación de proxies para evitar objetos DynamicProxy
            var prevProxySetting = db.Configuration.ProxyCreationEnabled;
            db.Configuration.ProxyCreationEnabled = false;

            var puestos = await db.Puesto
                .Include(p => p.Departamento) // asegurarse de traer el departamento
                .ToListAsync();

            // Restaurar configuración original
            db.Configuration.ProxyCreationEnabled = prevProxySetting;

            return View(puestos);
        }

        // GET: Puesto/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var puesto = await db.Puesto
                .Include(p => p.Departamento)
                .FirstOrDefaultAsync(p => p.idPuesto == id);

            if (puesto == null)
            {
                return HttpNotFound();
            }
            return View(puesto);
        }

        // GET: Puesto/Create
        public ActionResult Create()
        {
            ViewBag.Departamento_idDepartamento = new SelectList(db.Departamento, "idDepartamento", "nombre");
            return View();
        }

        // POST: Puesto/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idPuesto,nombre,planilla,salario_base,salario_por_hora,estado,fecha_inicio,fecha_fin,Departamento_idDepartamento")] Puesto puesto)
        {
            if (ModelState.IsValid)
            {
                db.Puesto.Add(puesto);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Departamento_idDepartamento = new SelectList(db.Departamento, "idDepartamento", "nombre", puesto.Departamento_idDepartamento);
            return View(puesto);
        }

        // GET: Puesto/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Puesto puesto = await db.Puesto.FindAsync(id);
            if (puesto == null)
            {
                return HttpNotFound();
            }
            ViewBag.Departamento_idDepartamento = new SelectList(db.Departamento, "idDepartamento", "nombre", puesto.Departamento_idDepartamento);
            return View(puesto);
        }

        // POST: Puesto/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idPuesto,nombre,planilla,salario_base,salario_por_hora,estado,fecha_inicio,fecha_fin,Departamento_idDepartamento")] Puesto puesto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(puesto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Departamento_idDepartamento = new SelectList(db.Departamento, "idDepartamento", "nombre", puesto.Departamento_idDepartamento);
            return View(puesto);
        }

        // GET: Puesto/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var puesto = await db.Puesto
                .Include(p => p.Departamento)
                .FirstOrDefaultAsync(p => p.idPuesto == id);

            if (puesto == null)
            {
                return HttpNotFound();
            }
            return View(puesto);
        }

        // POST: Puesto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Puesto puesto = await db.Puesto.FindAsync(id);
            db.Puesto.Remove(puesto);
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
