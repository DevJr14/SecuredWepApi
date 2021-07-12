using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models.Requests;
using WepApi.Services;

namespace WepApi.Controllers
{
    [Route("api/identity/token")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ICurrentUserService _currentUserService;

        public AuthController(IIdentityService identityService, ICurrentUserService currentUserService)
        {
            _identityService = identityService;
            _currentUserService = currentUserService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequest model)
        {
            var response = await _identityService.LoginAsync(model);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh([FromBody] TokenRefreshRequest model)
        {
            var response = await _identityService.GetRefreshTokenAsync(model);
            return Ok(response);
        }
    }
}
