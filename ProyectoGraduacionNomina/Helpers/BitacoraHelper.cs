using System;
using System.Linq;
using System.Web;

namespace ProyectoGraduacionNomina.Helpers
{
    public static class BitacoraHelper
    {
        public static void Registrar(
            BD_NominaEntities context,
            int credencialId,
            string accion,
            string descripcionBase,
            HttpContextBase httpContext = null
        )
        {
            string descripcionFinal = descripcionBase;

            // Intentar obtener Persona asociada
            var empleado = context.Empleado
                .Include("Persona")
                .FirstOrDefault(e => e.Credencial_idCredencial == credencialId);

            if (empleado != null && empleado.Persona != null)
            {
                var p = empleado.Persona;
                descripcionFinal +=
                    $" | Persona: {p.nombre} {p.apellido1} {p.apellido2} | Cédula: {p.cedula}";
            }
            else
            {
                descripcionFinal += $" | Credencial ID: {credencialId}";
            }

            var bitacora = new Bitacora
            {
                Credencial_idCredencial = credencialId,
                accion = accion,
                descripcion = descripcionFinal,
                fecha = DateTime.Now,
                ip_origen = httpContext?.Request?.UserHostAddress
            };

            context.Bitacora.Add(bitacora);
            context.SaveChanges();
        }
    }
}
