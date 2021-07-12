using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WepApi.Services
{
    public interface ICurrentUserService
    {
        string UserId { get; }
    }
}
