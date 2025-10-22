using MealToken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Models.Reports
{
	using System.ComponentModel.DataAnnotations;

	public class RequestDto
    {
        [Required(ErrorMessage = "Event date is required")]
        public DateOnly EventDate { get; set; }

        [Required(ErrorMessage = "Event type is required")]
        [StringLength(100, ErrorMessage = "Event type cannot exceed 100 characters")]
        public string EventType { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of attendees must be at least 1")]
        public int NoOfAttendess { get; set; }

        [MinLength(1, ErrorMessage = "At least one meal is required")]
        public List<RequestMealDto>? RequestMeals { get; set; }
    }

    public class RequestMealDto
    {
        [Required(ErrorMessage = "Meal type is required")]
        public int MealTypeId { get; set; }

        public int? SubTypeId { get; set; }

        [Required(ErrorMessage = "Meal cost ID is required")]
        public int MealCostId { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }
    }


}
