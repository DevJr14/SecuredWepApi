using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models.Requests
{
    public class PermissionRequest
    {
        public string RoleId { get; set; }
        public IList<RoleClaimsRequest> RoleClaims { get; set; }
    }
}
