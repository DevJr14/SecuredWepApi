using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WepApi.Domain;

namespace WepApi.Context
{
    public class ApplicationUser : IdentityUser, IAuditableEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }

        public bool IsActive { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
