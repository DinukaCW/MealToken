using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Enums
{
	public enum TimePeriod
	{
		Today,
		Yesterday,
		ThisWeek,
		LastWeek,
		ThisMonth,
		LastMonth,
		ThisYear,
		LastYear,
		AllTime,
		CustomRange
	}
}
