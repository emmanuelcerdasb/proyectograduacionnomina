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
    public class BitacoraController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // GET: Bitacora
        public ActionResult Index(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string accion,
            string rol)
        {
            var query = db.Bitacora
                .Include(b => b.Credencial)
                .Include(b => b.Credencial.Rol)
                .AsQueryable();

            // Filtro por fecha inicio
            if (fechaInicio.HasValue)
                query = query.Where(b => b.fecha >= fechaInicio.Value);

            // Filtro por fecha fin
            if (fechaFin.HasValue)
                query = query.Where(b => b.fecha <= fechaFin.Value);

            // Filtro por acción
            if (!string.IsNullOrEmpty(accion))
                query = query.Where(b => b.accion == accion);

            // Filtro por rol
            if (!string.IsNullOrEmpty(rol))
                query = query.Where(b => b.Credencial.Rol.nombre == rol);

            // Combos
            ViewBag.Acciones = new SelectList(
                db.Bitacora.Select(b => b.accion).Distinct().ToList()
            );

            ViewBag.Roles = new SelectList(
                db.Rol.Select(r => r.nombre).ToList()
            );

            return View(query
                .OrderByDescending(b => b.fecha)
                .ToList());
        }


        // GET: Bitacora/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bitacora bitacora = await db.Bitacora.FindAsync(id);
            if (bitacora == null)
            {
                return HttpNotFound();
            }
            return View(bitacora);
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
