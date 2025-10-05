using System.Text.Json;

namespace MealToken.API.Helpers
{
	public class ScheduleDateGeneratorService
	{
		public List<DateOnly> GenerateDatesForPeriod(string schedulePeriod, Dictionary<string, object> parameters)
		{
			return schedulePeriod switch
			{
				"Single Date" => GenerateSingleDate(parameters),
				"Date Range" => GenerateDateRange(parameters),
				"Year" => GenerateYearDates(parameters),
				"Month" => GenerateMonthDates(parameters),
				"Week" => GenerateWeekDates(parameters),
				"Weekdays" => GenerateWeekdayDates(parameters),
				"Weekends" => GenerateWeekendDates(parameters),
				"Custom Days" => GenerateCustomDays(parameters),
				_ => new List<DateOnly>()
			};
		}

		private List<DateOnly> GenerateSingleDate(Dictionary<string, object> parameters)
		{
			var selectedDate = DateOnly.Parse(parameters["selectedDate"].ToString());
			return new List<DateOnly> { selectedDate };
		}

		private List<DateOnly> GenerateDateRange(Dictionary<string, object> parameters)
		{
			var startDate = DateOnly.Parse(parameters["startDate"].ToString());
			var endDate = DateOnly.Parse(parameters["endDate"].ToString());

			var dates = new List<DateOnly>();
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				dates.Add(date);
			}
			return dates;
		}

		private List<DateOnly> GenerateYearDates(Dictionary<string, object> parameters)
		{
			var year = int.Parse(parameters["year"].ToString());
			var startDate = new DateOnly(year, 1, 1);
			var endDate = new DateOnly(year, 12, 31);

			var dates = new List<DateOnly>();
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				dates.Add(date);
			}
			return dates;
		}

		private List<DateOnly> GenerateMonthDates(Dictionary<string, object> parameters)
		{
			var year = int.Parse(parameters["year"].ToString());
			var month = int.Parse(parameters["month"].ToString());

			var startDate = new DateOnly(year, month, 1);
			var endDate = startDate.AddMonths(1).AddDays(-1);

			var dates = new List<DateOnly>();
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				dates.Add(date);
			}
			return dates;
		}

		private List<DateOnly> GenerateWeekDates(Dictionary<string, object> parameters)
		{
			var weekStartDate = DateOnly.Parse(parameters["weekStartDate"].ToString());

			var dates = new List<DateOnly>();
			for (int i = 0; i < 7; i++)
			{
				dates.Add(weekStartDate.AddDays(i));
			}
			return dates;
		}

		private List<DateOnly> GenerateWeekdayDates(Dictionary<string, object> parameters)
		{
			var startDate = DateOnly.Parse(parameters["startDate"].ToString());
			var endDate = DateOnly.Parse(parameters["endDate"].ToString());

			var dates = new List<DateOnly>();
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				if (date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday)
				{
					dates.Add(date);
				}
			}
			return dates;
		}

		private List<DateOnly> GenerateWeekendDates(Dictionary<string, object> parameters)
		{
			var startDate = DateOnly.Parse(parameters["startDate"].ToString());
			var endDate = DateOnly.Parse(parameters["endDate"].ToString());

			var dates = new List<DateOnly>();
			for (var date = startDate; date <= endDate; date = date.AddDays(1))
			{
				if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
				{
					dates.Add(date);
				}
			}
			return dates;
		}

		private List<DateOnly> GenerateCustomDays(Dictionary<string, object> parameters)
		{
			var jsonArray = (JsonElement)parameters["selectedDates"];
			var selectedDates = jsonArray.EnumerateArray()
				.Select(x => DateOnly.Parse(x.ToString()))
				.ToList();

			return selectedDates;
		}
	}
}