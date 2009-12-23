using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Linq;

namespace StandenMonitor.Core
{
	public class DocumentFilterFactory
	{
		/*
Submit
R = uitslagen
P = programma
M = schema
FT = vrije teams
*/
		private const string BASE_URL = @"http://www.knhb.nl/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx";

		internal DocumentFilter CreateDocumentFilter(string modulePart, bool clubMode, string club, string teamCode, string competitionKind, string ageClassCode, string teamSex)
		{
			DateTime eindDatum;
			DateTime startDatum;

			if (modulePart == "P")
			{
				startDatum = DateTime.Now;
				startDatum = new DateTime(startDatum.Year, startDatum.Month, startDatum.Day, 0, 0, 0, 0);

				eindDatum = new DateTime(startDatum.Ticks + (8 * 24 * 60 * 60 * 1000));
			}
			else
			{
				eindDatum = DateTime.Now;
				eindDatum = new DateTime(eindDatum.Year, eindDatum.Month, eindDatum.Day, 0, 0, 0, 0);

				startDatum = new DateTime(eindDatum.Ticks - (8 * 24 * 60 * 60 * 1000));
			}
			return CreateDocumentFilter(modulePart, clubMode, club, teamCode, competitionKind, ageClassCode, teamSex, startDatum, eindDatum);
		}

		internal DocumentFilter CreateDocumentFilter(string modulePart, bool clubMode, string club, string teamCode, string competitionKind, string ageClassCode, string teamSex, DateTime startDatum, DateTime eindDatum)
		{
			DocumentFilter newFilter = new DocumentFilter();

			string moduleName = "";
			if (clubMode)
			{
				newFilter.SetFilterItem("ClubMode", new StringFilterItem("True"));
				moduleName = "C" + modulePart;

				// Button ResultMatrix is Standing in Club modi
				if (modulePart == "M") //Matrix
					moduleName = "CS"; //ClubStanding
			}
			else
			{
				moduleName = moduleName + modulePart;
			}

			// volgorde niet geheel logisch, maar komt wel overeen met knhb site (ook maakt de volgorde niet uit). Dit is expres gedaan om de requests zo normaal mogelijk te laten lijken.
			newFilter.SetFilterItem("Age", new StringFilterItem(ageClassCode));
			newFilter.SetFilterItem("di", new DateFilterItem(startDatum));
			newFilter.SetFilterItem("Kind", new StringFilterItem(competitionKind));
			newFilter.SetFilterItem("Sex", new StringFilterItem(teamSex));
			newFilter.SetFilterItem("da", new DateFilterItem(eindDatum));
			newFilter.SetFilterItem("Team", new StringFilterItem(teamCode));
			newFilter.SetFilterItem("Club", new StringFilterItem(club));
			newFilter.SetFilterItem("M", new StringFilterItem(moduleName));
			
			return newFilter;
		}

		public string CreateRequestUrl(string modulePart, bool clubMode, string club, string teamCode, string competitionKind, string ageClassCode, string teamSex, DateTime startDatum, DateTime eindDatum)
		{
			DocumentFilter documentFilter = CreateDocumentFilter(modulePart, clubMode, club, teamCode, competitionKind, ageClassCode, teamSex, startDatum,
			                                             eindDatum);
			string requestUrl = BASE_URL;
			requestUrl += "?strFilter=" + EncodeTo64(documentFilter.ToString());
			return requestUrl;
		}

		public string EncodeTo64(string toEncode)
		{
			byte[] toEncodeAsBytes
				= Encoding.ASCII.GetBytes(toEncode);
			string returnValue
				= Convert.ToBase64String(toEncodeAsBytes);
			return returnValue;
		}

		public string DecodeFrom64(string encodedData)
		{
			byte[] encodedDataAsBytes
				= Convert.FromBase64String(encodedData);
			string returnValue =
				Encoding.ASCII.GetString(encodedDataAsBytes);
			return returnValue;
		}
	}

	public class HockeyStandenDao
	{


		public IDictionary<string, Team> GetTeams(string clubName)
		{
			throw new NotImplementedException();
		}

		private DocumentFilterFactory _documentFilterFactory;
		public DocumentFilterFactory DocumentFilterFactory
		{
			get
			{
				if (_documentFilterFactory == null) 
					_documentFilterFactory = new DocumentFilterFactory();
				return _documentFilterFactory;
			}
			set { _documentFilterFactory = value; }
		}

		private StandenMonitorScraper _standenMonitorScraper;
		public StandenMonitorScraper StandenMonitorScraper
		{
			get
			{
				if (_standenMonitorScraper == null)
					return _standenMonitorScraper = new StandenMonitorScraper();
				return _standenMonitorScraper;
			}
			set { _standenMonitorScraper = value; }
		}


		public Ranking GetRanking(Club club, Team team, DateTime startDate)
		{
			DateTime endDate = new DateTime(startDate.Ticks).AddDays(8);
			const string codeToRetrieveRanking = "R";
			const string seniorCompetitionKind = "D";
			string requestUrl = DocumentFilterFactory.CreateRequestUrl(codeToRetrieveRanking, false, club.Code, team.Code, seniorCompetitionKind, team.AgeClass, team.Sex,
			                                                    startDate, endDate);

			Ranking ranking = StandenMonitorScraper.GetRanking(requestUrl);

			return ranking;
		}
	}

	public class StandenMonitorScraper
	{
		private StandenReader standenReader = new StandenReader();

		public Ranking GetRanking(string requestUrl)
		{
			HtmlDocument html = GetHtml(requestUrl);
			HtmlNode node = html.DocumentNode;
			HtmlNode standenHtml = node.QuerySelector(".contStanden");
			//Console.WriteLine(standenHtml.InnerHtml);
			Ranking ranking = new Ranking();

			IList<HtmlNode> scoreNodes = node.QuerySelectorAll(".contStanden tr").ToList();
			foreach (HtmlNode scoreNode in scoreNodes)
			{
				Score score = standenReader.ReadScore(scoreNode.OuterHtml);
				if (score != null)
					ranking.Scores.Add(score);
			}

			return ranking;
		}

		public virtual HtmlDocument GetHtml(string requestUrl)
		{
			Console.WriteLine("Requesting: " + requestUrl);
			HtmlDocument html = new HtmlDocument();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
			request.MaximumAutomaticRedirections = 4;
			request.MaximumResponseHeadersLength = 10;
			
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				Console.WriteLine("Content length is {0}", response.ContentLength);
				Console.WriteLine("Content type is {0}", response.ContentType);

				// Get the stream associated with the response.
				Stream receiveStream = response.GetResponseStream();
				// Pipes the stream to a higher level stream reader with the required encoding format. 
				using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
				{
					html.Load(readStream);
					Console.WriteLine("Response stream received.");
					Console.WriteLine(readStream.ReadToEnd());
					readStream.Close();
				}
				response.Close();
			}
			return html;
		}
	}

	public class Team
	{
		public string AgeClass;
		public string Sex;
		public string Name;

		public string Code;
	}

	public class Ranking
	{
		private List<Score> _scores;

		public Ranking()
		{
			_scores = new List<Score>();
		}

		public List<Score> Scores
		{
			get {
				return _scores;
			}
			set {
				_scores = value;
			}
		}
	}

	public class StandenReader
	{
		public Score ReadScore(string html)
		{
			if (string.IsNullOrEmpty(html))
				return null;
			if (!html.StartsWith("<tr", StringComparison.InvariantCultureIgnoreCase) || !html.EndsWith("</tr>", StringComparison.InvariantCultureIgnoreCase))
				return null;

			Score score = new Score();
			
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);
			IList<HtmlNode> values = htmlDocument.DocumentNode.QuerySelectorAll("td").ToList();
			 
			if (values.Count == 0)
				return null;

			score.Team = values[1].QuerySelector("a").InnerText;
			score.Played = int.Parse(values[2].InnerText);
			score.Won = int.Parse(values[3].InnerText);
			score.Ties = int.Parse(values[4].InnerText);
			score.Lost = int.Parse(values[5].InnerText); ;
			score.GoalsMade = int.Parse(values[6].InnerText);
			score.GoalsAgainst = int.Parse(values[7].InnerText);
			score.PenaltyPoints = int.Parse(values[8].InnerText);
			score.Points = int.Parse(values[9].InnerText);
			
			return score;
		}
		
	}

	public class Score
	{
		public int Points;
		public string Team;
		public int Played;
		public int Won;
		public int Even;
		public int Lost;
		public int GoalsMade;
		public int GoalsAgainst;
		public int PenaltyPoints;
		public int Ties;
	}

	public class Club
	{
		public string Code;
	}
}