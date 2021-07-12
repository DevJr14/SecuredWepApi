using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WepApi.Domain
{
    public class AuditableEntity : IAuditableEntity
    {
        public int Id { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
