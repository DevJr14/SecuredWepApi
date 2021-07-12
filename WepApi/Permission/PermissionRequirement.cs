using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WepApi.Permission
{
    internal class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
