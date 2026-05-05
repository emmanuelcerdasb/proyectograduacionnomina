using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace ProyectoGraduacionNomina
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // ============================================================
        //  DECODIFICAR ROLES DESDE FORMS AUTH (CRÍTICO)
        // ============================================================
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User == null)
                return;

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
                return;

            if (!(HttpContext.Current.User.Identity is FormsIdentity identity))
                return;

            FormsAuthenticationTicket ticket = identity.Ticket;

            if (ticket == null || string.IsNullOrEmpty(ticket.UserData))
                return;

            // Roles vienen separados por coma
            string[] roles = ticket.UserData.Split(',');

            HttpContext.Current.User = new GenericPrincipal(identity, roles);
        }
    }
}
