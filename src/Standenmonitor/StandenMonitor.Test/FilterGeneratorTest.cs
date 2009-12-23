using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using StandenMonitor.Core;

namespace StandenMonitor.Test
{
	[TestFixture]
	public class FilterGeneratorTest
	{
		[Test]
		public void EncodeToBase64_ValidFilter_ValidEncodedFilter()
		{
			DocumentFilterFactory documentFilterFactory = new DocumentFilterFactory();
			string encodedQuerystring = documentFilterFactory.EncodeTo64("Age=001&di=1260712501&Kind=D&Sex=1&da=1261403701&Team=17303-42354&Club=HH11AS0&M=R");
			string expectedEncodedString = "QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTczMDMtNDIzNTQmQ2x1Yj1ISDExQVMwJk09Ug==";
			Assert.That(encodedQuerystring, Is.EqualTo(expectedEncodedString), encodedQuerystring);
		}

		[Test]
		public void DecodeToBase64_ValidEncodedFilter_ValidFilter()
		{
			DocumentFilterFactory documentFilterFactory = new DocumentFilterFactory();
			string decodedQueryString = documentFilterFactory.DecodeFrom64("QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTczMDMtNDIzNTQmQ2x1Yj1ISDExQVMwJk09Ug==");
			string expectedQueryString = "Age=001&di=1260712501&Kind=D&Sex=1&da=1261403701&Team=17303-42354&Club=HH11AS0&M=R";
			Assert.That(decodedQueryString, Is.EqualTo(expectedQueryString));
		}

		[Test]
		public void CreateDocumentFilter_AMVJH2_ValidFilterEnsureOrder()
		{
			DateTime startDatum = new DateTime(2009, 12, 13, 13, 55, 1);
			DateTime eindDatum = new DateTime(startDatum.Ticks).AddDays(8);

			DocumentFilterFactory documentFilterFactory = new DocumentFilterFactory();
			DocumentFilter actualFilter = documentFilterFactory.CreateDocumentFilter("R", false, "HH11AS0", "17303-42354", "D", "001", "1", startDatum, eindDatum);

			Assert.That(actualFilter.ToString(), Is.EqualTo("Age=001&di=1260712501&Kind=D&Sex=1&da=1261403701&Team=17303-42354&Club=HH11AS0&M=R"));
		}

		[Test]
		public void CreateRequestUrl_ValidInput_ValidUrlAlsoEnsureProperOrder()
		{
			string expectedUri =
				@"http://www.knhb.nl/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTczMDMtNDIzNTQmQ2x1Yj1ISDExQVMwJk09Ug==";
			DateTime startDatum = new DateTime(2009, 12, 13, 13, 55, 1);
			DateTime eindDatum = new DateTime(startDatum.Ticks).AddDays(8);
			DocumentFilterFactory documentFilter = new DocumentFilterFactory();
			
			string actual = documentFilter.CreateRequestUrl("R", false, "HH11AS0", "17303-42354", "D", "001", "1", startDatum, eindDatum);
			
			Assert.That(actual, Is.EqualTo(expectedUri));
		}

		[Test]
		[Ignore("Integration test, live connection to standenmonitor site")]
		public void TryRequest()
		{
			string expectedUri =
				@"http://www.knhb.nl/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=TT1SJkNsdWI9SEgxMUFTMCZUZWFtPTE3MzAzLTQyMzU0JktpbmQ9RCZBZ2U9MDAxJlNleD0xJmRpPTEyNjA3MTI1MDEmZGE9MTI2MTQwMzcwMQ==";

			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(expectedUri);
			request.MaximumAutomaticRedirections = 4;
			request.MaximumResponseHeadersLength = 10;
			HttpWebResponse response = (HttpWebResponse) request.GetResponse();
			
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

		[Test]
		public void GetTeamScoreData()
		{
			//<div class="contStanden">

		}

		
	}

	[TestFixture]
	public class HockeyStandenDaoTest
	{
		private HockeyStandenDao _standenDao;
		
		[SetUp]
		public void SetUp()
		{
			_standenDao = new HockeyStandenDao();
		}

		[Test]
		public void GetScores_ValidClubAndTeam_ValidScores()
		{
			Club someClub = new Club();
			Team someTeam = new Team();
			DateTime forDate = new DateTime(2009, 12, 20);

			Ranking expectedRanking = new Ranking();
			expectedRanking.Scores = new List<Score> { new Score { Team = "team1", Points = 10 }, new Score { Team = "team2", Points = 8 } };
			
			Ranking ranking = _standenDao.GetRanking(someClub, someTeam, forDate);
			Assert.That(ranking.Scores, Is.EquivalentTo(expectedRanking.Scores));
		}

		[Test]
		public void GetTeams_ValidClub_ValidTeams() 
		{
			string club = "AMVJ";
			IDictionary<string, Team> expectedTeams = new Dictionary<string, Team>();
			expectedTeams.Add("H1", new Team{Name= "H1", Code="123"});
			expectedTeams.Add("H2", new Team{Name= "H2", Code="321"});

			 IDictionary<string, Team> teams = _standenDao.GetTeams("AMVJ");
			Assert.That(teams, Is.EquivalentTo(expectedTeams));

		}
	}

	
}