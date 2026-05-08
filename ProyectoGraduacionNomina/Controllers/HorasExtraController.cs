using ProyectoGraduacionNomina.Helpers;
using ProyectoGraduacionNomina.Servicios;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize]
    public class HorasExtraController : Controller
    {
        private BD_NominaEntities _db = new BD_NominaEntities();
        private HorasExtraService _service;

        public HorasExtraController()
        {
            _service = new HorasExtraService(_db);
        }

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            return View();
        }

        // =====================================================
        // CREAR SOLICITUD (GET)
        // =====================================================
        public ActionResult CrearSolicitudHoraExtra()
        {
            CargarClasesHoraExtra();
            CargarEmpleadosSiCorresponde();

            ViewBag.FechaMin = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.FechaMax = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");

            return View();
        }

        // =====================================================
        // CREAR SOLICITUD (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearSolicitudHoraExtra(
            int claseHoraExtraId,
            DateTime fecha,
            decimal cantidadHoras,
            int? empleadoId // SOLO para admin/jefatura
        )
        {
            try
            {
                int empleadoFinalId;

                // COLABORADOR → usa sesión
                if (User.IsInRole("Colaborador"))
                {
                    if (Session["EmpleadoId"] == null)
                        throw new Exception("Sesión inválida. Vuelva a iniciar sesión.");

                    empleadoFinalId = (int)Session["EmpleadoId"];
                }
                else
                {
                    // ADMIN / JEFE / RRHH
                    if (!empleadoId.HasValue)
                        throw new Exception("Debe seleccionar un empleado.");

                    empleadoFinalId = empleadoId.Value;
                }

                _service.CrearSolicitudHorasExtra(
                    empleadoFinalId,
                    claseHoraExtraId,
                    fecha,
                    cantidadHoras
                );

                if (Session["CredencialId"] != null)
                    BitacoraHelper.Registrar(_db, (int)Session["CredencialId"],
                        "SOLICITAR HORAS EXTRA",
                        $"Solicitud horas extra: empleadoId={empleadoFinalId} | Fecha={fecha:dd/MM/yyyy} | Horas={cantidadHoras}",
                        this.HttpContext);

                TempData["Success"] = "Solicitud de horas extra enviada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                CargarClasesHoraExtra();
                CargarEmpleadosSiCorresponde();

                ViewBag.FechaMin = DateTime.Today.ToString("yyyy-MM-dd");
                ViewBag.FechaMax = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");

                return View();
            }
        }

        // =====================================================
        // MIS SOLICITUDES (COLABORADOR)
        // =====================================================
        [Authorize(Roles = "Colaborador,Administrador,Jefe,Jefa,RRHH")]
        public ActionResult MisSolicitudes()
        {
            if (Session["EmpleadoId"] == null)
                return RedirectToAction("Login", "Account");

            int empleadoId = (int)Session["EmpleadoId"];

            var solicitudes = _service.ObtenerSolicitudesPorEmpleado(empleadoId);

            return View(solicitudes);
        }


        // =====================================================
        // SOLICITUDES PENDIENTES
        // =====================================================
        [Authorize(Roles = "Administrador,Jefe,Jefa,RRHH")]
        public ActionResult SolicitudesPendientes()
        {
            var solicitudes = _service.ObtenerSolicitudesPendientes();
            return View(solicitudes);
        }

        // =====================================================
        // APROBAR SOLICITUD
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Jefe,Jefa,RRHH")]
        public ActionResult Aprobar(int id)
        {
            if (Session["CredencialId"] == null)
                return RedirectToAction("Login", "Account");

            try
            {
                int credencialId = (int)Session["CredencialId"];
                _service.AprobarSolicitud(id, credencialId);

                BitacoraHelper.Registrar(_db, credencialId,
                    "APROBAR HORAS EXTRA",
                    $"Horas extra idHoraExtra={id} aprobadas.",
                    this.HttpContext);

                TempData["Success"] = "Solicitud de horas extra aprobada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("SolicitudesPendientes");
        }

        // =====================================================
        // RECHAZAR SOLICITUD
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Jefe,Jefa,RRHH")]
        public ActionResult Rechazar(int id)
        {
            if (Session["CredencialId"] == null)
                return RedirectToAction("Login", "Account");

            try
            {
                int credencialId = (int)Session["CredencialId"];
                _service.RechazarSolicitud(id, credencialId);

                BitacoraHelper.Registrar(_db, credencialId,
                    "RECHAZAR HORAS EXTRA",
                    $"Horas extra idHoraExtra={id} rechazadas.",
                    this.HttpContext);

                TempData["Success"] = "Solicitud de horas extra rechazada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("SolicitudesPendientes");
        }

        // =====================================================
        // HELPERS PRIVADOS
        // =====================================================
        private void CargarClasesHoraExtra()
        {
            ViewBag.ClaseHoraExtraId = _db.ClaseHoraExtra
                .Select(c => new SelectListItem
                {
                    Value = c.idClaseHoraExtra.ToString(),
                    Text = c.nombre + " (" + c.porcentaje + "%)"
                })
                .ToList();
        }

        private void CargarEmpleadosSiCorresponde()
        {
            if (User.IsInRole("Administrador") ||
                User.IsInRole("Jefe") ||
                User.IsInRole("Jefa") ||
                User.IsInRole("RRHH"))
            {
                ViewBag.EmpleadoId = _db.Empleado
                    .Select(e => new SelectListItem
                    {
                        Value = e.idEmpleado.ToString(),
                        Text = e.Persona.nombre + " " + e.Persona.apellido1
                    })
                    .ToList();
            }
        }
    }
}
