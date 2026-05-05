using ProyectoGraduacionNomina;
using ProyectoGraduacionNomina.Helpers;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ProyectoGraduacionNomina.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private BD_NominaEntities db = new BD_NominaEntities();

        // ============================================================
        // GET: Account/Login
        // ============================================================
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ============================================================
        // POST: Account/Login
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string usuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
            {
                TempData["Error"] = "Debe ingresar usuario y contraseña.";
                return RedirectToAction("Login");
            }

            var cred = await db.Credencial
                .Include(c => c.Rol)
                .FirstOrDefaultAsync(c => c.usuario == usuario);

            if (cred == null || !PasswordHelper.VerifyPassword(contrasena, cred.contrasena))
            {
                TempData["Error"] = "Usuario o contraseña incorrectos.";
                return RedirectToAction("Login");
            }

            if (!cred.activo)
            {
                TempData["Error"] = "La cuenta está inactiva.";
                return RedirectToAction("Login");
            }

            // ====================== AUTH TICKET ======================
            string roles = cred.Rol.nombre;

            var authTicket = new FormsAuthenticationTicket(
                1,
                cred.usuario,
                DateTime.Now,
                DateTime.Now.AddHours(8),
                false,
                roles
            );

            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

            Response.Cookies.Add(new HttpCookie(
                FormsAuthentication.FormsCookieName,
                encryptedTicket
            ));

            // ====================== SESSION ==========================
            Session["CredencialId"] = cred.idCredencial;
            Session["Rol"] = cred.Rol.nombre;

            // ====================== AUDITORÍA LOGIN ==================
            BitacoraHelper.Registrar(
                db,
                cred.idCredencial,
                "LOGIN",
                $"Inicio de sesión exitoso. Rol: {cred.Rol.nombre}",
                this.HttpContext
            );

            // ====================== SESIÓN USUARIO ===================
            try
            {
                var sesion = new SesionUsuario
                {
                    Credencial_idCredencial = cred.idCredencial,
                    fecha_inicio = DateTime.Now,
                    ip = Request.UserHostAddress,
                    dispositivo = Request.Browser.Browser,
                    activo = true
                };

                db.SesionUsuario.Add(sesion);
                await db.SaveChangesAsync();
                Session["idSesion"] = sesion.idSesion;
            }
            catch { }

            // ================== ASOCIAR EMPLEADO =====================
            var empleado = await db.Empleado
                .Include(e => e.Persona)
                .FirstOrDefaultAsync(e => e.Credencial_idCredencial == cred.idCredencial);

            if (empleado == null)
            {
                FormsAuthentication.SignOut();
                Session.Clear();
                TempData["Error"] = "Su usuario no está asociado a un empleado.";
                return RedirectToAction("Login");
            }

            Session["EmpleadoId"] = empleado.idEmpleado;
            Session["EmpleadoNombre"] =
                $"{empleado.Persona.nombre} {empleado.Persona.apellido1}";

            // ========= CAMBIO OBLIGATORIO DE CONTRASEÑA =========
            if (cred.requiere_cambio)
            {
                return RedirectToAction("CambiarContrasena", "Credencial");
            }

            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // Logout
        // ============================================================
        [Authorize]
        public ActionResult Logout()
        {
            try
            {
                if (Session["CredencialId"] != null)
                {
                    int credencialId = (int)Session["CredencialId"];

                    BitacoraHelper.Registrar(
                        db,
                        credencialId,
                        "LOGOUT",
                        "Cierre de sesión del usuario",
                        this.HttpContext
                    );
                }

                if (Session["idSesion"] != null)
                {
                    int idSesion = (int)Session["idSesion"];
                    var ses = db.SesionUsuario.Find(idSesion);
                    if (ses != null)
                    {
                        ses.fecha_fin = DateTime.Now;
                        ses.activo = false;
                        db.SaveChanges();
                    }
                }
            }
            catch { }

            FormsAuthentication.SignOut();
            Session.Clear();

            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
