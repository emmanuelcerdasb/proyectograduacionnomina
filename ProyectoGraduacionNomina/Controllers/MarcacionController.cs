using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProyectoGraduacionNomina;
using ProyectoGraduacionNomina.Helpers;

namespace ProyectoGraduacionNomina.Controllers
{
    [Authorize(Roles = "Administrador,Jefa,Jefe,Colaborador")]
    public class MarcacionController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ============================================================
        // GET: Marcacion/Marcar
        // ============================================================
        public async Task<ActionResult> Marcar()
        {
            int? empId = Session["EmpleadoId"] as int?;

            if (empId == null)
            {
                TempData["Error"] = "No está asociado a ningún empleado.";
                return RedirectToAction("Index", "Home");
            }

            var empleado = await db.Empleado
                .Include(e => e.Persona)
                .FirstOrDefaultAsync(e => e.idEmpleado == empId.Value);

            if (empleado == null)
            {
                TempData["Error"] = "Empleado no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // -----------------------------------------
            // ÚLTIMA MARCACIÓN
            // -----------------------------------------
            var ultimoReg = await db.RegistroMarcacion
                .Where(r => r.Empleado_idEmpleado == empId.Value)
                .OrderByDescending(r => r.fecha_registro)
                .FirstOrDefaultAsync();

            bool habilitarEntrada = false;
            bool habilitarSalida = false;

            if (ultimoReg == null)
            {
                habilitarEntrada = true;
            }
            else if (ultimoReg.tipo == "Entrada")
            {
                habilitarSalida = true;
            }
            else if (ultimoReg.tipo == "Salida")
            {
                habilitarEntrada = true;
            }

            ViewBag.NombreEmpleado =
                $"{empleado.Persona.nombre} {empleado.Persona.apellido1} {empleado.Persona.apellido2} — {empleado.Persona.cedula}";

            ViewBag.Empleado_idEmpleado = empId.Value;
            ViewBag.HabilitarEntrada = habilitarEntrada;
            ViewBag.HabilitarSalida = habilitarSalida;

            return View();
        }

        // ============================================================
        // POST: Marcacion/MarcarEntradaSalida
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarcarEntradaSalida(string tipo, string observacion)
        {
            int? empId = Session["EmpleadoId"] as int?;

            if (empId == null)
            {
                TempData["Error"] = "No se pudo identificar al empleado.";
                return RedirectToAction("Marcar");
            }

            if (string.IsNullOrWhiteSpace(tipo))
            {
                TempData["Error"] = "Debe seleccionar Entrada o Salida.";
                return RedirectToAction("Marcar");
            }

            DateTime ahora = DateTime.Now;

            var registro = new RegistroMarcacion
            {
                Empleado_idEmpleado = empId.Value,
                fecha = ahora.Date,
                hora = ahora.TimeOfDay,
                tipo = tipo,
                origen = "Colaborador",
                observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion,
                fecha_registro = ahora
            };

            db.RegistroMarcacion.Add(registro);
            await db.SaveChangesAsync();

            // SINCRONIZACIÓN MARCACIÓN → ASISTENCIA
            SincronizarAsistencia(empId.Value, registro.fecha);
            await db.SaveChangesAsync();

            if (Session["CredencialId"] != null)
                BitacoraHelper.Registrar(db, (int)Session["CredencialId"],
                    $"MARCACION {tipo.ToUpper()}",
                    $"Empleado ID {empId.Value} marcó {tipo} el {ahora:dd/MM/yyyy} a las {ahora:HH:mm:ss}.",
                    this.HttpContext);

            TempData["Success"] = $"Marcación de {tipo} registrada correctamente.";
            return RedirectToAction("Marcar");
        }

        // ============================================================
        // SINCRONIZAR MARCACIONES CON ASISTENCIA
        // ============================================================
        private void SincronizarAsistencia(int empleadoId, DateTime fecha)
        {
            var marcaciones = db.RegistroMarcacion
                .Where(r =>
                    r.Empleado_idEmpleado == empleadoId &&
                    r.fecha == fecha)
                .OrderBy(r => r.hora)
                .ToList();

            var asistencia = db.Asistencia
                .FirstOrDefault(a =>
                    a.Empleado_idEmpleado == empleadoId &&
                    a.fecha == fecha);

            // --------------------------------------------------------
            // CREAR ASISTENCIA SI NO EXISTE (INICIALIZACIÓN COMPLETA)
            // --------------------------------------------------------
            if (asistencia == null)
            {
                asistencia = new Asistencia
                {
                    Empleado_idEmpleado = empleadoId,
                    fecha = fecha,

                    estado = "Pendiente",
                    es_justificada = false,

                    horas_trabajadas = 0,
                    horas_netas = 0,
                    horas_extra = 0,

                    tiene_tardia = false,
                    minutos_tardia = 0,

                    observacion = null,

                    fecha_registro = DateTime.Now,
                    fecha_actualizacion = DateTime.Now
                };

                db.Asistencia.Add(asistencia);
            }

            var entrada = marcaciones.FirstOrDefault(m => m.tipo == "Entrada");
            var salida = marcaciones.LastOrDefault(m => m.tipo == "Salida");

            asistencia.hora_entrada = entrada?.hora;
            asistencia.hora_salida = salida?.hora;

            if (entrada != null && salida != null)
            {
                var horas = (salida.hora - entrada.hora).TotalHours;
                if (horas < 0) horas = 0;

                asistencia.horas_trabajadas = (decimal)Math.Round(horas, 2);

                const decimal horasAlmuerzo = 1.0m;
                asistencia.horas_netas =
                    Math.Max(0, asistencia.horas_trabajadas - horasAlmuerzo);

                asistencia.estado = "Presente";
                asistencia.es_justificada = true;
            }
            else
            {
                asistencia.horas_trabajadas = 0;
                asistencia.horas_netas = 0;

                asistencia.estado = "Ausente";
                asistencia.es_justificada = false;
            }

            asistencia.fecha_actualizacion = DateTime.Now;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
