using ProyectoGraduacionNomina;
using ProyectoGraduacionNomina.Servicios;
using System;
using System.Linq;
using System.Web.Mvc;

public class AsistenciaController : Controller
{
    private readonly BD_NominaEntities _db = new BD_NominaEntities();
    private readonly AsistenciaService _asistenciaService;

    public AsistenciaController()
    {
        _asistenciaService = new AsistenciaService(_db);
    }

    // =====================================================
    // MARCACIÓN
    // =====================================================
    public ActionResult Marcar()
    {
        ViewBag.Empleados = _db.Empleado
            .Select(e => new SelectListItem
            {
                Value = e.idEmpleado.ToString(),
                Text = e.Persona.nombre + " " +
                       e.Persona.apellido1 + " " +
                       e.Persona.apellido2
            })
            .ToList();

        return View();
    }

    [HttpPost]
    public ActionResult MarcarEntrada(int empleadoId)
    {
        try
        {
            _asistenciaService.RegistrarEntrada(empleadoId, DateTime.Now);
            TempData["Success"] = "Entrada registrada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Marcar");
    }

    [HttpPost]
    public ActionResult MarcarSalida(int empleadoId)
    {
        try
        {
            _asistenciaService.RegistrarSalida(empleadoId, DateTime.Now);
            TempData["Success"] = "Salida registrada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Marcar");
    }

    // =====================================================
    // REPORTES
    // =====================================================
    [HttpPost]
    public ActionResult ReporteEmpleado(int empleadoId, DateTime inicio, DateTime fin)
    {
        var lista =
            _asistenciaService.ObtenerReporteAsistenciaEmpleado(
                empleadoId, inicio, fin);

        if (lista.Any())
        {
            ViewBag.NombreEmpleado = lista.First().NombreEmpleado;
            ViewBag.CedulaEmpleado = lista.First().CedulaEmpleado;
        }
        else
        {
            ViewBag.NombreEmpleado = "Sin datos";
            ViewBag.CedulaEmpleado = "";
        }

        ViewBag.FechaInicio = inicio.ToString("dd/MM/yyyy");
        ViewBag.FechaFin = fin.ToString("dd/MM/yyyy");

        return View("~/Views/Asistencia/ReporteEmpleado.cshtml", lista);
    }

    [HttpPost]
    public ActionResult ResumenMensual(int empleadoId, int mes, int anno)
    {
        var resumen =
            _asistenciaService.ObtenerResumenMensual(
                empleadoId, mes, anno);

        return View("~/Views/Asistencia/ResumenMensual.cshtml", resumen);
    }
}
