using MealToken.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealToken.Domain.Entities
{
   public class UserDepartment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserDepartmentId { get; set; }
        public int TenantId { get; set; }
        public int UserRequestId { get; set; }
        public int? UserId { get; set; }
        public int DepartmentId { get; set; }
        public UserRequestStatus RequestStatus { get; set; }
        
    }
}
