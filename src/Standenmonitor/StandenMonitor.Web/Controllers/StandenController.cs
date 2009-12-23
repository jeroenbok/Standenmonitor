using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StandenMonitor.Core;

namespace StandenMonitor.Web.Controllers
{
	[HandleError]
	public class StandenController : Controller
	{
		public ActionResult Index()
		{

			return View();
		}

		public ActionResult Ranking(string club, string team)
		{
			Club someClub = null;
			Team someTeam = null;

			// get club
			if (club == "amvj")
				someClub = new Club { Code = "HH11AS0" };
			
			// get team
			if (team == "H2")
				someTeam = new Team { Code = "17303-42354", AgeClass = "001", Name = "AMVJ H2", Sex = "1" };

			DateTime startDatum = new DateTime(2009, 12, 13, 13, 55, 1);

			Ranking ranking = new HockeyStandenDao().GetRanking(someClub, someTeam, startDatum);

			ViewData["Scores"] = ranking.Scores;

			return View();
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
