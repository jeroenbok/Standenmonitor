using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using NUnit.Framework;
using Rhino.Mocks;
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
	}

	[TestFixture]
	public class HockeyStandenDaoTest
	{
		private HockeyStandenDao _standenDao;
		private HtmlDocument _dummyHtml;
		
		[SetUp]
		public void SetUp()
		{
			_standenDao = new HockeyStandenDao();
			_dummyHtml = new HtmlDocument();

			using (TextReader reader = new StreamReader("./dummyHtml_standen_amvjh2.html"))
			{
				_dummyHtml.LoadHtml(reader.ReadToEnd());
			}
			

		}

		[Test(Description = "Integration Test")]
		public void GetScores_ValidClubAndTeam_ValidScores()
		{
			StandenMonitorScraper scraper = MockRepository.GeneratePartialMock<StandenMonitorScraper>();
			_standenDao.StandenMonitorScraper = scraper;
			Club someClub = new Club { Code = "HH11AS0" };
			Team someTeam = new Team { Code = "17303-42354", AgeClass = "001", Name = "AMVJ H2", Sex = "1"};
			//DateTime forDate = new DateTime(2009, 12, 20);
			DateTime startDatum = new DateTime(2009, 12, 13, 13, 55, 1);

			scraper.Stub(x => x.GetHtml("")).IgnoreArguments().Return(_dummyHtml);
			Ranking ranking = _standenDao.GetRanking(someClub, someTeam, startDatum);

			Ranking expectedRanking = new Ranking();
			expectedRanking.Scores = new List<Score>
			                         	{
			                         		new Score { Team = "AthenA H2", Points = 31 }, 
											new Score { Team = "Myra H3", Points = 27 }, 
											new Score { Team = "Pinoké H8", Points = 24 }, 
											new Score { Team = "Hurley H6", Points = 23 }, 
											new Score { Team = "Amsterdam H10", Points = 22 }, 
											new Score { Team = "Pinoké H11", Points = 19 }, 
											new Score { Team = "Xenios H3", Points = 17 }, 
											new Score { Team = "HIC H4", Points = 15 }, 
											new Score { Team = "AMVJ H2", Points = 10 }, 
											new Score { Team = "Amsterdam H11", Points = 9 }, 
											new Score { Team = "VVV H4", Points = 6 }, 
											new Score { Team = "Castricum H2", Points = 5 }, 
			                         	};
			Assert.That(ranking.Scores.Count, Is.EqualTo(expectedRanking.Scores.Count));

			for (int position = 0; position < expectedRanking.Scores.Count; position++)
			{
				Score expectedScore = expectedRanking.Scores[position];
				Score actualScore = ranking.Scores[position];

				Assert.That(actualScore.Team, Is.EqualTo(expectedScore.Team));
				Assert.That(actualScore.Points, Is.EqualTo(expectedScore.Points)); 
			}
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

	[TestFixture]
	public class StanderReaderTest
	{
		private StandenReader _reader;
		

		[SetUp]
		public void SetUp()
		{
			_reader = new StandenReader();
		}

		[Test]
		public void ReadTeamStanden_NoTableRowInHtml_ReturnsNull()
		{
			string html = "<td></td>";
			Assert.That(_reader.ReadScore(html), Is.Null);
		}

		[Test]
		public void ReadTeamStanden_Header_ReturnsNull()
		{
			string html = "<tr><th><abbr title=\"Positie\">Pos.</abbr></th>" +
			              "<th>Teamnaam</th> " +
			              "<th><abbr title=\"Gespeelde wedstrijden\">GS</abbr></th> " +
			              "<th><abbr title=\"Gewonnen wedstrijden\">GW</abbr></th> " +
			              "<th><abbr title=\"Gelijk gespeelde wedstrijden\">GL</abbr></th> " +
			              "<th><abbr title=\"Verloren wedstrijden\">VL</abbr></th> " +
			              "<th><abbr title=\"Doelpunten voor\">V</abbr></th> " +
			              "<th><abbr title=\"Doelpunten tegen\">T</abbr></th> " +
			              "<th><abbr title=\"Punten in mindering\">PIM</abbr></th> " +
			              "<th><abbr title=\"Punten\">PT</abbr></th></tr>";
			Assert.That(_reader.ReadScore(html), Is.Null);
		}

		[Test]
		public void ReadTeamStanden_ValidTeam_FilledScoreForTeam()
		{
			string html = "<tr>" +
			              "<td>1</td>" +
			              "<td><a href=\"/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjY3NDgtNDIzNTQmQ2x1Yj1MVjk1S1k2Jk09VFI%3d\" class=\"hrefTeam \">AthenA H2</a></td>" +
			              "<td>11</td>" +
			              "<td>10</td>" +
			              "<td>1</td>" +
			              "<td>0</td>" +
			              "<td>38</td>" +
			              "<td>13</td>" +
			              "<td>0</td>" +
			              "<td>31</td>" +
			              "</tr>";
			Score score = _reader.ReadScore(html);
			score.Team = "AthenA H2";
			score.Played = 11;
			score.Won = 10;
			score.Even = 1;
			score.Lost = 0;
			score.GoalsMade = 38;
			score.GoalsAgainst = 13;
			score.PenaltyPoints = 0;
			score.Points = 31;
		}

//		<h2 class="fullColumn">Standen:</h2>
//                <table border="0" cellpadding="2" cellspacing="0" class="tblResults fullColumn">
//                    <colgroup>
//                        <col class="colPosition">
//                        <col class="colTeamname">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <col class="colValue">
//                        <tr>
//                            <th>
//                                <abbr title="Positie">Pos.</abbr></th>
//                            <th>
//                                Teamnaam</th>
//                            <th>
//                                <abbr title="Gespeelde wedstrijden">GS</abbr></th>
//                            <th>
//                                <abbr title="Gewonnen wedstrijden">GW</abbr></th>
//                            <th>
//                                <abbr title="Gelijk gespeelde wedstrijden">GL</abbr></th>
//                            <th>
//                                <abbr title="Verloren wedstrijden">VL</abbr></th>
//                            <th>
//                                <abbr title="Doelpunten voor">V</abbr></th>
//                            <th>
//                                <abbr title="Doelpunten tegen">T</abbr></th>
//                            <th>
//                                <abbr title="Punten in mindering">PIM</abbr></th>
//                            <th>
//                                <abbr title="Punten">PT</abbr></th>
//                        </tr>
//                        
//                                <tr>
//                                    <td>1</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjY3NDgtNDIzNTQmQ2x1Yj1MVjk1S1k2Jk09VFI%3d" class="hrefTeam ">AthenA H2</a></td>
//                                    <td>11</td>
//                                    <td>10</td>
//                                    <td>1</td>
//                                    <td>0</td>
//                                    <td>38</td>
//                                    <td>13</td>
//                                    <td>0</td>
//                                    <td>31</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>2</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjI1NDAtNDIzNTQmQ2x1Yj1ISDExTFY0Jk09VFI%3d" class="hrefTeam ">Myra H3</a></td>
//                                    <td>12</td>
//                                    <td>9</td>
//                                    <td>0</td>
//                                    <td>3</td>
//                                    <td>41</td>
//                                    <td>26</td>
//                                    <td>0</td>
//                                    <td>27</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>3</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjMzODMtNDIzNTQmQ2x1Yj1ISDExTVc0Jk09VFI%3d" class="hrefTeam ">Pinoké H8</a></td>
//                                    <td>12</td>
//                                    <td>8</td>
//                                    <td>0</td>
//                                    <td>4</td>
//                                    <td>34</td>
//                                    <td>19</td>
//                                    <td>0</td>
//                                    <td>24</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>4</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjExMDAtNDIzNTQmQ2x1Yj1ISDExSlIwJk09VFI%3d" class="hrefTeam ">Hurley H6</a></td>
//                                    <td>12</td>
//                                    <td>7</td>
//                                    <td>2</td>
//                                    <td>3</td>
//                                    <td>30</td>
//                                    <td>17</td>
//                                    <td>0</td>
//                                    <td>23</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>5</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTcyNjctNDIzNTQmQ2x1Yj1ISDExQVIzJk09VFI%3d" class="hrefTeam ">Amsterdam H10</a></td>
//                                    <td>12</td>
//                                    <td>7</td>
//                                    <td>1</td>
//                                    <td>4</td>
//                                    <td>42</td>
//                                    <td>19</td>
//                                    <td>0</td>
//                                    <td>22</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>6</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjMzODctNDIzNTQmQ2x1Yj1ISDExTVc0Jk09VFI%3d" class="hrefTeam ">Pinoké H11</a></td>
//                                    <td>12</td>
//                                    <td>6</td>
//                                    <td>1</td>
//                                    <td>5</td>
//                                    <td>28</td>
//                                    <td>18</td>
//                                    <td>0</td>
//                                    <td>19</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>7</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjYzMjktNDIzNTQmQ2x1Yj1ISDExUlkzJk09VFI%3d" class="hrefTeam ">Xenios H3</a></td>
//                                    <td>12</td>
//                                    <td>5</td>
//                                    <td>2</td>
//                                    <td>5</td>
//                                    <td>41</td>
//                                    <td>39</td>
//                                    <td>0</td>
//                                    <td>17</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>8</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjY3NzYtNDIzNTQmQ2x1Yj1ISDExSFAwJk09VFI%3d" class="hrefTeam ">HIC H4</a></td>
//                                    <td>12</td>
//                                    <td>5</td>
//                                    <td>0</td>
//                                    <td>7</td>
//                                    <td>33</td>
//                                    <td>33</td>
//                                    <td>0</td>
//                                    <td>15</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>9</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTczMDMtNDIzNTQmQ2x1Yj1ISDExQVMwJk09VFI%3d" class="hrefTeam activeTeam">AMVJ H2</a></td>
//                                    <td>12</td>
//                                    <td>3</td>
//                                    <td>1</td>
//                                    <td>8</td>
//                                    <td>20</td>
//                                    <td>41</td>
//                                    <td>0</td>
//                                    <td>10</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>10</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTcyNjktNDIzNTQmQ2x1Yj1ISDExQVIzJk09VFI%3d" class="hrefTeam ">Amsterdam H11</a></td>
//                                    <td>11</td>
//                                    <td>3</td>
//                                    <td>0</td>
//                                    <td>8</td>
//                                    <td>21</td>
//                                    <td>38</td>
//                                    <td>0</td>
//                                    <td>9</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>11</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MjU4MTAtNDIzNTQmQ2x1Yj1ISDExUkQ2Jk09VFI%3d" class="hrefTeam ">VVV H4</a></td>
//                                    <td>12</td>
//                                    <td>2</td>
//                                    <td>0</td>
//                                    <td>10</td>
//                                    <td>19</td>
//                                    <td>55</td>
//                                    <td>0</td>
//                                    <td>6</td>
//                                </tr>
//                            
//                                <tr>
//                                    <td>12</td>
//                                    <td><a href="/competities/standenmotor/standenmotor/cDU593_Standenmotor.aspx?strFilter=QWdlPTAwMSZkaT0xMjYwNzEyNTAxJktpbmQ9RCZTZXg9MSZkYT0xMjYxNDAzNzAxJlRlYW09MTgzNzYtNDIzNTQmQ2x1Yj1ISDExQ0swJk09VFI%3d" class="hrefTeam ">Castricum H2</a></td>
//                                    <td>12</td>
//                                    <td>1</td>
//                                    <td>2</td>
//                                    <td>9</td>
//                                    <td>13</td>
//                                    <td>42</td>
//                                    <td>0</td>
//                                    <td>5</td>
//                                </tr>
//                            
//                </colgroup></table>
//                <br>
//                <div class="contLegenda">
//                    <h3>Legenda</h3>
//                    <p>Betekenis afkortingen bij het standen overzicht</p>
//                    <ul>
//                        <li>
//                            <strong>GS</strong>Gespeelde wedstrijden
//                        <li>
//                            <strong>GW</strong>Gewonnen wedstrijden
//                        <li>
//                            <strong>GL</strong>Gelijk gespeelde wedstrijden
//                        <li>
//                            <strong>VL</strong>Verloren wedstrijden
//                        <li>
//                            <strong>V</strong>Doelpunten voor
//                        <li>
//                            <strong>T</strong>Doelpunten tegen
//                        <li>
//                            <strong>PIM</strong>Punten in mindering
//                        <li>
//                            <strong>PT</strong>Punten</li>
//                    </li></li></li></li></li></li></li></ul>
//                </div>

	}

	
}