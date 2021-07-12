using Common.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models.Requests;
using Common.Models.Responses;

namespace WepApi.Services
{
    public interface IIdentityService
    {
        Task<Result<List<UserResponse>>> GetAllUsersAsync();

        Task<int> GetUserCountAsync();

        Task<IResult> RegisterUserAsync(RegisterRequest model);

        Task<IResult<UserResponse>> GetUserAsync(string userId);

        Task<IResult<UserRolesResponse>> GetUserRolesAsync(string userId);

        Task<IResult> UpdateUserRolesAsync(UpdateUserRolesRequest model);

        Task<Result<LoginResponse>> LoginAsync(LoginRequest model);

        Task<Result<LoginResponse>> GetRefreshTokenAsync(TokenRefreshRequest model);

        #region Role and Permissions
        Task<Result<List<RoleResponse>>> GetAllRolesAsync();

        Task<int> GetRolesCountAsync();

        Task<Result<RoleResponse>> GetRoleByIdAsync(string roleId);

        Task<Result<string>> AddNewRoleAsync(RoleRequest request);

        Task<Result<string>> DeleteRoleAsync(string roleId);

        Task<Result<PermissionResponse>> GetRolePermissionsAsync(string roleId);

        Task<Result<string>> UpdateRolePermissionsAsync(PermissionRequest request);

        #endregion
    }
}
