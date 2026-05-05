namespace ProyectoGraduacionNomina.Helpers
{
    public static class BitacoraVisualHelper
    {
        public static string ObtenerClaseAccion(string accion)
        {
            if (string.IsNullOrWhiteSpace(accion))
                return "badge bg-secondary";

            switch (accion.ToUpper())
            {
                case "CREATE":
                    return "badge bg-success";

                case "CREAR EMPLEADO":
                    return "badge bg-success";

                case "CREAR CREDENCIAL":
                    return "badge bg-success";

                case "UPDATE":
                    return "badge bg-primary";

                case "DELETE":
                    return "badge bg-danger";

                case "LOGIN":
                    return "badge bg-info text-dark";

                case "LOGOUT":
                    return "badge bg-secondary";

                default:
                    return "badge bg-light text-dark";
            }
        }
    }
}
