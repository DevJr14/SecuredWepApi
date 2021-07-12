using Common.Constants;
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
    [Route("api/identity/role")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public RolesController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [Authorize(Policy = Permissions.Roles.View)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _identityService.GetAllRolesAsync();
            return Ok(roles);
        }

        [Authorize(Policy = Permissions.Roles.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(RoleRequest request)
        {
            var response = await _identityService.AddNewRoleAsync(request);
            return Ok(response);
        }

        [Authorize(Policy = Permissions.Roles.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _identityService.DeleteRoleAsync(id);
            return Ok(response);
        }

        [Authorize(Policy = Permissions.Roles.Edit)]
        [HttpGet("permissions/{roleId}")]
        public async Task<IActionResult> GetPermissionsByRoleId([FromRoute] string roleId)
        {
            var response = await _identityService.GetRolePermissionsAsync(roleId);
            return Ok(response);
        }

        [Authorize(Policy = Permissions.Roles.Edit)]
        [HttpPut("permissions/update")]
        public async Task<IActionResult> Update(PermissionRequest model)
        {
            var response = await _identityService.UpdateRolePermissionsAsync(model);
            return Ok(response);
        }
    }
}
