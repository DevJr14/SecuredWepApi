using Common.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    [Route("api/identity/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public UsersController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        //[Authorize(Policy = Permissions.Users.View)]
        [Authorize(JwtBearerDefaults.AuthenticationScheme, Policy = Permissions.Users.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _identityService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(Policy = Permissions.Users.View)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _identityService.GetUserAsync(id);
            return Ok(user);
        }

        [Authorize(Policy = Permissions.Users.View)]
        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetRolesAsync(string id)
        {
            var userRoles = await _identityService.GetUserRolesAsync(id);
            return Ok(userRoles);
        }

        [Authorize(Policy = Permissions.Users.Edit)]
        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRolesAsync(UpdateUserRolesRequest request)
        {
            return Ok(await _identityService.UpdateUserRolesAsync(request));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            return Ok(await _identityService.RegisterUserAsync(request));
        }
    }
}
