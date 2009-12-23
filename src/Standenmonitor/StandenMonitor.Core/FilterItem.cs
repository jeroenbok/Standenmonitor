using System;

namespace StandenMonitor.Core
{
	public abstract class FilterItem
	{
		public abstract string FilterValue { get; }
	}

	public abstract class FilterItem<T> : FilterItem
	{
		public T Value { get; set; }
	}

	public class DateFilterItem : FilterItem<DateTime>
	{
		public DateFilterItem(DateTime value)
		{
			Value = value;
		}

		public override string FilterValue
		{
			get
			{
				DateTime jsStartDate = new DateTime(1970, 1, 1);
				long newTicks = (Value.Ticks - jsStartDate.Ticks) / (1000 * 10000);
				return newTicks.ToString();
			}
		}
	}

	public class StringFilterItem : FilterItem<String>
	{
		public StringFilterItem(string value)
		{
			Value = value;
		}

		public override string FilterValue
		{
			get { return Value; }
		}
	}

	public class IntegerFilterItem : FilterItem<int>
	{
		public override string FilterValue
		{
			get { return Value.ToString(); }
		}
	}
}