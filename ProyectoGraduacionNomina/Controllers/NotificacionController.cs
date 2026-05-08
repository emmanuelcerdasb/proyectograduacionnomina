using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize]
    public class NotificacionController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();

        // Devuelve el total de pendientes para el badge del campanazo
        [ChildActionOnly]
        public ActionResult Badge()
        {
            int total = 0;

            // Solo roles que aprueban ven el conteo
            if (User.IsInRole("Administrador") ||
                User.IsInRole("RRHH") ||
                User.IsInRole("Jefe") ||
                User.IsInRole("Jefa"))
            {
                int vac = _db.Vacaciones.Count(v => v.estado == "Pendiente");
                int he  = _db.HoraExtra.Count(h => !h.aprobado);
                total   = vac + he;
            }

            return PartialView("_NotifBadge", total);
        }

        // Conteo individual para los links del sidebar
        [ChildActionOnly]
        public ActionResult BadgeVacaciones()
        {
            int count = 0;
            if (User.IsInRole("Administrador") || User.IsInRole("RRHH") ||
                User.IsInRole("Jefe") || User.IsInRole("Jefa"))
                count = _db.Vacaciones.Count(v => v.estado == "Pendiente");
            return PartialView("_NotifCount", count);
        }

        [ChildActionOnly]
        public ActionResult BadgeHorasExtra()
        {
            int count = 0;
            if (User.IsInRole("Administrador") || User.IsInRole("RRHH") ||
                User.IsInRole("Jefe") || User.IsInRole("Jefa"))
                count = _db.HoraExtra.Count(h => !h.aprobado);
            return PartialView("_NotifCount", count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
