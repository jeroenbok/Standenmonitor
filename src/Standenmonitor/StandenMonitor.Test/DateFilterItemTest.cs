using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using StandenMonitor.Core;

namespace StandenMonitor.Test
{
	[TestFixture]
	public class DateFilterItemTest
	{
		[Test]
		public void GetValue_ValidDate_ConvertsToKNHBDate()
		{
			string expectedFilter = "1260712501";
			DateTime date = new DateTime(2009, 12, 13, 13, 55, 1);
			
			DateFilterItem filterItem = new DateFilterItem(date);
			string filterValue = filterItem.FilterValue;
			Assert.That(filterValue, Is.EqualTo(expectedFilter));
			
		}
	}

}
