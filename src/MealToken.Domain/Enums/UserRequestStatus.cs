using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Enums
{
	public enum UserRequestStatus
	{
		Pending = 1,
		Approved = 2,
		Rejected = 3,
		Expired = 4
	}
}
