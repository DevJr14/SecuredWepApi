using Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models.Requests
{
    public class UpdateUserRolesRequest
    {
        public string UserId { get; set; }
        public IList<UserRoleModel> UserRoles { get; set; }
    }
}
