using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MinLength(2)]
        [MaxLength(64)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(64)]
        public string LastName { get; set; }

        [Required]
        public DateTime? DateRegisteredUtc { get; set; }
    }
}
