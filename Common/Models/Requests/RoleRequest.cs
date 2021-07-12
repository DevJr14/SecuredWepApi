﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models.Requests
{
    public class RoleRequest
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
