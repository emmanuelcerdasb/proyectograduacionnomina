using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ProyectoGraduacionNomina.Servicios
{
    public class AsistenciaService
    {
        private readonly BD_NominaEntities _db;

        // CONFIGURACIÓN GLOBAL
        private readonly int MINUTOS_TOLERANCIA_TARDIA = 10;

        public AsistenciaService(BD_NominaEntities context)
        {
            _db = context;
        }

        // =====================================================
        // REGISTRAR ENTRADA
        // =====================================================
        public void RegistrarEntrada(int empleadoId, DateTime fechaHora)
        {
            DateTime fecha = fechaHora.Date;

            var empleado = _db.Empleado
                .Include(e => e.HorarioSemanal)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            if (empleado == null)
                throw new Exception("Empleado no encontrado.");

            if (_db.Asistencia.Any(a =>
                a.Empleado_idEmpleado == empleadoId &&
                a.fecha == fecha))
                throw new Exception("Ya existe una marcación para este día.");

            var horarioDia = ObtenerHorarioDelDia(empleado, fechaHora.DayOfWeek);

            bool tieneTardia = false;

            if (horarioDia != null)
            {
                TimeSpan horaEsperada =
                    horarioDia.hora_inicio.Add(
                        TimeSpan.FromMinutes(MINUTOS_TOLERANCIA_TARDIA));

                if (fechaHora.TimeOfDay > horaEsperada)
                    tieneTardia = true;
            }

            var asistencia = new Asistencia
            {
                Empleado_idEmpleado = empleadoId,
                fecha = fecha,
                hora_entrada = fechaHora.TimeOfDay,
                tiene_tardia = tieneTardia
            };

            _db.Asistencia.Add(asistencia);
            _db.SaveChanges();
        }

        // =====================================================
        // REGISTRAR SALIDA
        // =====================================================
        public void RegistrarSalida(int empleadoId, DateTime fechaHora)
        {
            DateTime fecha = fechaHora.Date;

            var asistencia = _db.Asistencia.FirstOrDefault(a =>
                a.Empleado_idEmpleado == empleadoId &&
                a.fecha == fecha);

            if (asistencia == null)
                throw new Exception("No existe entrada registrada para hoy.");

            if (asistencia.hora_salida != null)
                throw new Exception("La salida ya fue registrada.");

            asistencia.hora_salida = fechaHora.TimeOfDay;

            var empleado = _db.Empleado
                .Include(e => e.HorarioSemanal)
                .FirstOrDefault(e => e.idEmpleado == empleadoId);

            var horarioDia = ObtenerHorarioDelDia(empleado, fechaHora.DayOfWeek);

            if (horarioDia != null)
            {
                if (fechaHora.TimeOfDay > horarioDia.hora_fin)
                {
                    TimeSpan extra = fechaHora.TimeOfDay - horarioDia.hora_fin;

                    _db.HoraExtra.Add(new HoraExtra
                    {
                        Empleado_idEmpleado = empleadoId,
                        fecha = fecha,
                        cantidad_horas = (decimal)extra.TotalHours,
                        aprobado = false
                    });
                }
            }

            _db.SaveChanges();
        }

        // =====================================================
        // ASISTENCIA DIARIA (USADO POR NÓMINA)
        // =====================================================
        public ResultadoAsistenciaDiaria ObtenerAsistenciaDiaria(int empleadoId, DateTime fecha)
        {
            var asistencia = _db.Asistencia.FirstOrDefault(a =>
                a.Empleado_idEmpleado == empleadoId &&
                a.fecha == fecha);

            if (asistencia == null)
            {
                return new ResultadoAsistenciaDiaria
                {
                    Fecha = fecha,
                    EsAusencia = true
                };
            }

            decimal horasNetas = 0;

            if (asistencia.hora_entrada != null && asistencia.hora_salida != null)
            {
                horasNetas = (decimal)(
                    asistencia.hora_salida.Value -
                    asistencia.hora_entrada.Value).TotalHours;
            }

            return new ResultadoAsistenciaDiaria
            {
                Fecha = fecha,
                HoraEntrada = asistencia.hora_entrada,
                HoraSalida = asistencia.hora_salida,
                HorasNetas = horasNetas,
                TieneTardia = asistencia.tiene_tardia,
                MinutosTardia = asistencia.tiene_tardia
                    ? Math.Max(0,
                        (int)(
                            asistencia.hora_entrada.Value -
                            ObtenerHoraInicioEsperada(empleadoId, fecha)
                        ).TotalMinutes)
                    : 0,
                EsAusencia = false
            };
        }

        // =====================================================
        // REPORTE DETALLADO
        // =====================================================
        public List<ResultadoAsistenciaDiaria> ObtenerReporteAsistenciaEmpleado(
            int empleadoId, DateTime inicio, DateTime fin)
        {
            var empleado = _db.Empleado
                .Include(e => e.Persona)
                .First(e => e.idEmpleado == empleadoId);

            var lista = new List<ResultadoAsistenciaDiaria>();

            for (DateTime f = inicio; f <= fin; f = f.AddDays(1))
            {
                var dia = ObtenerAsistenciaDiaria(empleadoId, f);

                dia.EmpleadoId = empleadoId;
                dia.NombreEmpleado = empleado.Persona.nombre + " " +
                                     empleado.Persona.apellido1 + " " +
                                     empleado.Persona.apellido2;
                dia.CedulaEmpleado = empleado.Persona.cedula;

                lista.Add(dia);
            }

            return lista;
        }


        // =====================================================
        // RESUMEN MENSUAL
        // =====================================================
        public ResumenAsistenciaMensual ObtenerResumenMensual(int empleadoId, int mes, int anno)
        {
            DateTime inicio = new DateTime(anno, mes, 1);
            DateTime fin = inicio.AddMonths(1).AddDays(-1);

            var dias = new List<ResultadoAsistenciaDiaria>();

            for (DateTime f = inicio; f <= fin; f = f.AddDays(1))
                dias.Add(ObtenerAsistenciaDiaria(empleadoId, f));

            return new ResumenAsistenciaMensual
            {
                Mes = mes,
                Anno = anno,
                DiasLaborados = dias.Count(d => !d.EsAusencia),
                DiasAusencia = dias.Count(d => d.EsAusencia),
                DiasConTardia = dias.Count(d => d.TieneTardia),
                TotalHorasNetas = dias.Sum(d => d.HorasNetas)
            };
        }

        // =====================================================
        // HORARIO
        // =====================================================
        private Horario ObtenerHorarioDelDia(Empleado empleado, DayOfWeek diaSemana)
        {
            if (empleado.HorarioSemanal_idHorarioSemanal == null)
                return null;

            string nombreDia = ObtenerNombreDia(diaSemana);

            return _db.Horario.FirstOrDefault(h =>
                h.HorarioSemanal_idHorarioSemanal ==
                    empleado.HorarioSemanal_idHorarioSemanal &&
                h.dia_semana == nombreDia);
        }

        private TimeSpan ObtenerHoraInicioEsperada(int empleadoId, DateTime fecha)
        {
            var empleado = _db.Empleado
                .Include(e => e.HorarioSemanal)
                .First(e => e.idEmpleado == empleadoId);

            var horario = ObtenerHorarioDelDia(empleado, fecha.DayOfWeek);
            return horario?.hora_inicio ?? TimeSpan.Zero;
        }

        private string ObtenerNombreDia(DayOfWeek dia)
        {
            switch (dia)
            {
                case DayOfWeek.Monday: return "Lunes";
                case DayOfWeek.Tuesday: return "Martes";
                case DayOfWeek.Wednesday: return "Miércoles";
                case DayOfWeek.Thursday: return "Jueves";
                case DayOfWeek.Friday: return "Viernes";
                case DayOfWeek.Saturday: return "Sábado";
                case DayOfWeek.Sunday: return "Domingo";
                default: return "";
            }
        }

        private DateTime FechaSegura(DateTime fecha)
        {
            var minimoSql = new DateTime(1753, 1, 1);

            if (fecha < minimoSql)
                return DateTime.Now;

            return fecha;
        }

    }
}
