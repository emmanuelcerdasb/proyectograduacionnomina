using ProyectoGraduacionNomina;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoGraduacionNomina.Servicios
{
    public class HorasExtraService
    {
        private readonly BD_NominaEntities _db;

        public HorasExtraService(BD_NominaEntities db)
        {
            _db = db;
        }

        // =========================================
        // CREAR SOLICITUD DE HORAS EXTRA
        // =========================================
        public void CrearSolicitudHorasExtra(
            int empleadoId,
            int claseHoraExtraId,
            DateTime fecha,
            decimal cantidadHoras
        )
        {
            var horaExtra = new HoraExtra
            {
                Empleado_idEmpleado = empleadoId,
                ClaseHoraExtra_idClaseHoraExtra = claseHoraExtraId,
                fecha = fecha.Date,
                cantidad_horas = cantidadHoras,
                aprobado = false
            };

            _db.HoraExtra.Add(horaExtra);
            _db.SaveChanges();

            var solicitud = new SolicitudHorasExtra
            {
                HoraExtra_idHoraExtra = horaExtra.idHoraExtra,
                fecha_solicitud = DateTime.Now.Date,
                estado = "PENDIENTE"
            };

            _db.SolicitudHorasExtra.Add(solicitud);
            _db.SaveChanges();
        }

        // =========================================
        // SOLICITUDES DEL COLABORADOR
        // =========================================
        public List<SolicitudHorasExtra> ObtenerSolicitudesPorEmpleado(int empleadoId)
        {
            return _db.SolicitudHorasExtra
                .Where(s => s.HoraExtra.Empleado_idEmpleado == empleadoId)
                .OrderByDescending(s => s.fecha_solicitud)
                .ToList();
        }

        // =========================================
        // SOLICITUDES PENDIENTES (JEFATURA / RRHH)
        // =========================================
        public List<SolicitudHorasExtra> ObtenerSolicitudesPendientes()
        {
            return _db.SolicitudHorasExtra
                .Where(s => s.estado == "PENDIENTE")
                .OrderBy(s => s.fecha_solicitud)
                .ToList();
        }

        // =========================================
        // APROBAR SOLICITUD
        // =========================================
        public void AprobarSolicitud(
            int solicitudId,
            int usuarioAprobadorId
        )
        {
            var solicitud = _db.SolicitudHorasExtra
                .FirstOrDefault(s => s.idSolicitudHorasExtra == solicitudId);

            if (solicitud == null)
                throw new Exception("Solicitud no encontrada");

            solicitud.estado = "APROBADA";
            solicitud.aprobado_por = usuarioAprobadorId;
            solicitud.fecha_aprobacion = DateTime.Now.Date;

            // Marcar la hora extra como aprobada
            solicitud.HoraExtra.aprobado = true;

            _db.SaveChanges();
        }

        // =========================================
        // RECHAZAR SOLICITUD
        // =========================================
        public void RechazarSolicitud(
            int solicitudId,
            int usuarioAprobadorId
        )
        {
            var solicitud = _db.SolicitudHorasExtra
                .FirstOrDefault(s => s.idSolicitudHorasExtra == solicitudId);

            if (solicitud == null)
                throw new Exception("Solicitud no encontrada");

            solicitud.estado = "RECHAZADA";
            solicitud.aprobado_por = usuarioAprobadorId;
            solicitud.fecha_aprobacion = DateTime.Now.Date;

            solicitud.HoraExtra.aprobado = false;

            _db.SaveChanges();
        }

        // =========================================
        // HORAS EXTRA APROBADAS (PARA NÓMINA)
        // =========================================
        public List<HoraExtra> ObtenerHorasExtraAprobadas(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin
        )
        {
            return _db.HoraExtra
                .Where(h =>
                    h.Empleado_idEmpleado == empleadoId &&
                    h.aprobado == true &&
                    h.fecha >= fechaInicio.Date &&
                    h.fecha <= fechaFin.Date
                )
                .ToList();
        }
    }
}
