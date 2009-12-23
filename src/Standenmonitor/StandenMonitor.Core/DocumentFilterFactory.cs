using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

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
		public Ranking GetRanking(string requestUrl)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
			request.MaximumAutomaticRedirections = 4;
			request.MaximumResponseHeadersLength = 10;
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			Console.WriteLine("Content length is {0}", response.ContentLength);
			Console.WriteLine("Content type is {0}", response.ContentType);

			// Get the stream associated with the response.
			Stream receiveStream = response.GetResponseStream();

			// Pipes the stream to a higher level stream reader with the required encoding format. 
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

			Console.WriteLine("Response stream received.");
			Console.WriteLine(readStream.ReadToEnd());
			response.Close();
			readStream.Close();
		}
	}

	public class Team
	{
		public string AgeClass;
		public string Sex;
		public string Name { get; set; }

		public string Code { get; set;}
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

	public class Score
	{
		public int Points;
		public string Team;
	}

	public class Club
	{
		public string Code;
	}
}