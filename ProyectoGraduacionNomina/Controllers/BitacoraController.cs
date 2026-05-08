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
        private const int PorPagina = 50;

        public ActionResult Index(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string accion,
            string rol,
            int pagina = 1)
        {
            var query = db.Bitacora
                .Include(b => b.Credencial)
                .Include(b => b.Credencial.Rol)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(b => b.fecha >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(b => b.fecha <= fechaFin.Value);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(b => b.accion == accion);

            if (!string.IsNullOrEmpty(rol))
                query = query.Where(b => b.Credencial.Rol.nombre == rol);

            // Combos
            ViewBag.Acciones = new SelectList(
                db.Bitacora.Select(b => b.accion).Distinct().ToList()
            );
            ViewBag.Roles = new SelectList(
                db.Rol.Select(r => r.nombre).ToList()
            );

            // Paginación
            int total = query.Count();
            int totalPaginas = (int)Math.Ceiling(total / (double)PorPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, totalPaginas)));

            ViewBag.PaginaActual  = pagina;
            ViewBag.TotalPaginas  = totalPaginas;
            ViewBag.TotalRegistros = total;

            // Preservar filtros en links de paginación
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin    = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.AccionFiltro = accion;
            ViewBag.RolFiltro    = rol;

            return View(query
                .OrderByDescending(b => b.fecha)
                .Skip((pagina - 1) * PorPagina)
                .Take(PorPagina)
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
