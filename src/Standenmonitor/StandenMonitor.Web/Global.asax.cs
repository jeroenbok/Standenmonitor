using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StandenMonitor.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default",                                              // Route name
				"",                           // URL with parameters
				new { controller = "Standen", action = "Index"}  // Parameter defaults
			);

			routes.MapRoute(
				"Standen",                                              // Route name
				"Standen/{club}/{team}",                           // URL with parameters
				new { controller = "Standen", action = "Ranking", club = "AMVJ", team = "H2" }  // Parameter defaults
			);

		}

		protected void Application_Start()
		{
			RegisterRoutes(RouteTable.Routes);
		}
	}
}