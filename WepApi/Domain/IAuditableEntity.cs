using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WepApi.Domain
{
    public interface IAuditableEntity
    {
        string CreatedBy { get; set; }

        DateTime CreatedOn { get; set; }

        string ModifiedBy { get; set; }

        DateTime? ModifiedOn { get; set; }

        public string DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
