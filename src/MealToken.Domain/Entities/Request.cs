using MealToken.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
    public class Request
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealRequestId    { get; set; }
        public int TenantId { get; set; }
        public DateOnly EventDate { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public int NoofAttendees { get; set; }
        public UserRequestStatus Status { get; set; }
        public int RequesterId {  get; set; }
        public int? ApproverOrRejectedId { get; set; }

    }
  
}
