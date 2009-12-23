<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="System.Web.UI.MobileControls"%>

<%@ Import Namespace="StandenMonitor.Core" %>
<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Home Page
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Standen van H2</h2>
	<table>
		<tr>
			<th>Teamnaam</th>
		<th><abbr title="Gespeelde wedstrijden">GS</abbr></th>
		<th><abbr title="Gewonnen wedstrijden">GW</abbr></th>
		<th><abbr title="Gelijk gespeelde wedstrijden">GL</abbr>
		</th>
		<th><abbr title="Verloren wedstrijden">VL</abbr>
		</th>
		<th><abbr title="Doelpunten voor">V</abbr>
		</th>
		<th><abbr title="Doelpunten tegen">T</abbr>
		</th>
		<th><abbr title="Punten in mindering">PIM</abbr>
		</th>
		<th><abbr title="Punten">PT</abbr>
		</th>
		</tr>
		<% foreach (var score in (ViewData["Scores"] as List<Score>)) { %>
			<tr>
				<td><%= score.Team %></td>
				<td><%= score.Played %></td>
				<td><%= score.Won %></td>
				<td><%= score.Ties %></td>
				<td><%= score.Lost %></td>
				<td><%= score.GoalsMade %></td>
				<td><%= score.GoalsAgainst %></td>
				<td><%= score.PenaltyPoints %></td>
				<td><%= score.Points %></td>
				
			</tr>
		<% } %>
	</table>
</asp:Content>
