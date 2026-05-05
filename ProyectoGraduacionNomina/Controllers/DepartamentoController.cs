using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProyectoGraduacionNomina;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DepartamentoController : Controller
    {
        private readonly BD_NominaEntities db = new BD_NominaEntities();

        // GET: Departamento
        public async Task<ActionResult> Index()
        {
            var lista = await db.Departamento.ToListAsync();
            return View(lista);
        }

        // GET: Departamento/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var departamento = await db.Departamento.FindAsync(id);
            if (departamento == null)
                return HttpNotFound();

            return View(departamento);
        }

        // GET: Departamento/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departamento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idDepartamento,nombre,descripcion")] Departamento departamento)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validar si ya existe un departamento con el mismo nombre
                    bool existe = await db.Departamento.AnyAsync(d => d.nombre == departamento.nombre);
                    if (existe)
                    {
                        ModelState.AddModelError("", "Ya existe un departamento con ese nombre.");
                        return View(departamento);
                    }

                    db.Departamento.Add(departamento);
                    await db.SaveChangesAsync();

                    TempData["MensajeExito"] = "Departamento creado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el departamento: " + ex.Message);
            }

            return View(departamento);
        }

        // GET: Departamento/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var departamento = await db.Departamento.FindAsync(id);
            if (departamento == null)
                return HttpNotFound();

            return View(departamento);
        }

        // POST: Departamento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idDepartamento,nombre,descripcion")] Departamento departamento)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validar duplicado al editar
                    bool existe = await db.Departamento.AnyAsync(d => d.nombre == departamento.nombre && d.idDepartamento != departamento.idDepartamento);
                    if (existe)
                    {
                        ModelState.AddModelError("", "Ya existe otro departamento con ese nombre.");
                        return View(departamento);
                    }

                    db.Entry(departamento).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["MensajeExito"] = "Departamento actualizado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el departamento: " + ex.Message);
            }

            return View(departamento);
        }

        // GET: Departamento/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var departamento = await db.Departamento.FindAsync(id);
            if (departamento == null)
                return HttpNotFound();

            return View(departamento);
        }

        // POST: Departamento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var departamento = await db.Departamento.FindAsync(id);
                if (departamento == null)
                    return HttpNotFound();

                db.Departamento.Remove(departamento);
                await db.SaveChangesAsync();

                TempData["MensajeExito"] = "Departamento eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar el departamento: " + ex.Message;
            }

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
