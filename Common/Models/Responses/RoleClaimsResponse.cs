using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models.Responses
{
    public class RoleClaimsResponse
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
