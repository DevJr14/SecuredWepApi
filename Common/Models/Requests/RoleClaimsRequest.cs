using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models.Requests
{
    public class RoleClaimsRequest
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
