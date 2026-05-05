using ProyectoGraduacionNomina;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,Jefa,Jefe")]
    public class ReportesController : Controller
    {
        private readonly BD_NominaEntities _db = new BD_NominaEntities();

        // =====================================================
        // MÉTODO CENTRAL PARA CARGAR EMPLEADOS
        // =====================================================
        private void CargarEmpleados()
        {
            ViewBag.empleadoId = _db.Empleado
                .Select(e => new SelectListItem
                {
                    Value = e.idEmpleado.ToString(),
                    Text = e.Persona.nombre + " "
                         + e.Persona.apellido1 + " "
                         + e.Persona.apellido2
                         + " - " + e.Persona.cedula
                })
                .ToList();
        }

        // =====================================================
        // INDEX
        // =====================================================
        public ActionResult Index()
        {
            return View();
        }

        // =====================================================
        // REPORTE ASISTENCIA POR EMPLEADO (FORMULARIO)
        // =====================================================
        public ActionResult AsistenciaEmpleado()
        {
            CargarEmpleados();

            ViewBag.FechaInicio = System.DateTime.Today.AddDays(-7);
            ViewBag.FechaFin = System.DateTime.Today;

            return View();
        }

        // =====================================================
        // RESUMEN ASISTENCIA MENSUAL (FORMULARIO)
        // =====================================================
        public ActionResult ResumenAsistenciaMensual()
        {
            CargarEmpleados();

            ViewBag.Mes = System.DateTime.Today.Month;
            ViewBag.Anno = System.DateTime.Today.Year;

            return View();
        }
    }
}
