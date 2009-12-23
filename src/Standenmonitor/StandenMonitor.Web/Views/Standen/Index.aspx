<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="System.Web.UI.MobileControls"%>

<%@ Import Namespace="StandenMonitor.Core" %>
<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Home Page
</asp:Content>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Standen</h2>
	Bla bla praatje
	<a href ="./Standen/amvj/H2/"> AMVJ - Heren 2</a>
</asp:Content>
