using System.Collections.Generic;

namespace StandenMonitor.Core
{
	public class DocumentFilter
	{
		private IDictionary<string, FilterItem> FilterItems { get; set; }
		
		public DocumentFilter()
		{
			FilterItems = new Dictionary<string, FilterItem>();
		}

		public void SetFilterItem(string name, FilterItem value)
		{
			if (FilterItems.ContainsKey(name))
			{
				FilterItems[name] = value;
			} else
			{
				FilterItems.Add(name, value);
			}
		}

		public FilterItem GetFilterItem(string name)
		{
			if (FilterItems.ContainsKey(name))
			{
				return FilterItems[name];
			}
			return null;
		}

		public override string ToString()
		{
			string queryString = string.Empty;
			foreach (string name in FilterItems.Keys)
			{
				if (queryString.Length > 0)
				{
					queryString += "&";
				}

				queryString += name;
				queryString += "=";
				queryString += FilterItems[name].FilterValue;
			}

			return queryString;
		}
	}
}