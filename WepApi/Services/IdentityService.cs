using AutoMapper;
using Common.Constants;
using Common.Wrappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WepApi.Context;
using WepApi.Helpers;
using Common.Models.Requests;
using Common.Models.Responses;
using WepApi.Settings;

namespace WepApi.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppConfiguration _appConfig;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public IdentityService(SignInManager<ApplicationUser> signInManager, IOptions<AppConfiguration> appConfig, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, IMapper mapper)
        {
            _signInManager = signInManager;
            _appConfig = appConfig.Value;
            _roleManager = roleManager;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Result<string>> AddNewRoleAsync(RoleRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                var existingRole = await _roleManager.FindByNameAsync(request.Name);
                if (existingRole != null) return Result<string>.Fail($"Similar Role already exists.");
                var response = await _roleManager.CreateAsync(new IdentityRole(request.Name));
                return Result<string>.Success("Role Created");
            }
            else
            {
                var existingRole = await _roleManager.FindByIdAsync(request.Id);
                if (existingRole.Name == "Administrator" || existingRole.Name == "Basic")
                {
                    return Result<string>.Fail($"Not allowed to modify {existingRole.Name} Role.");
                }
                existingRole.Name = request.Name;
                existingRole.NormalizedName = request.Name.ToUpper();
                await _roleManager.UpdateAsync(existingRole);
                return Result<string>.Success("Role Updated.");
            }
        }

        public async Task<Result<string>> DeleteRoleAsync(string roleId)
        {
            var existingRole = await _roleManager.FindByIdAsync(roleId);
            if (existingRole.Name != "Administrator" && existingRole.Name != "Basic")
            {
                //Check if Any Users already uses this Role
                bool roleIsNotUsed = true;
                var allUsers = await _userManager.Users.ToListAsync();
                foreach (var user in allUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, existingRole.Name))
                    {
                        roleIsNotUsed = false;
                    }
                }
                if (roleIsNotUsed)
                {
                    await _roleManager.DeleteAsync(existingRole);
                    return Result<string>.Success($"Role {existingRole.Name} deleted.");
                }
                else
                {
                    return Result<string>.Fail($"Not allowed to delete {existingRole.Name} Role as it is being used.");
                }
            }
            else
            {
                return Result<string>.Fail($"Not allowed to delete {existingRole.Name} Role.");
            }
        }

        public async Task<Result<List<RoleResponse>>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var rolesResponse = _mapper.Map<List<RoleResponse>>(roles);
            return Result<List<RoleResponse>>.Success(rolesResponse);
        }

        public async Task<Result<List<UserResponse>>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = _mapper.Map<List<UserResponse>>(users);
            return Result<List<UserResponse>>.Success(result);
        }

        public async Task<Result<LoginResponse>> GetRefreshTokenAsync(TokenRefreshRequest model)
        {
            if (model is null)
            {
                return Result<LoginResponse>.Fail("Invalid Client Token.");
            }
            var userPrincipal = GetPrincipalFromExpiredToken(model.Token);
            var userEmail = userPrincipal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
                return Result<LoginResponse>.Fail("User Not Found.");
            if (user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return Result<LoginResponse>.Fail("Invalid Client Token.");
            var token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user));
            user.RefreshToken = GenerateRefreshToken();
            await _userManager.UpdateAsync(user);

            var response = new LoginResponse { Token = token, RefreshToken = user.RefreshToken, RefreshTokenExpiryTime = user.RefreshTokenExpiryTime };
            return Result<LoginResponse>.Success(response);
        }

        #region Token Generation
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appConfig.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        private async Task<string> GenerateJwtAsync(ApplicationUser user)
        {
            var token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user));
            return token;
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            //Adding Claims to the token
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();
            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, roles[i]));
                var thisRole = await _roleManager.FindByNameAsync(roles[i]);
                var allPermissionsForThisRoles = await _roleManager.GetClaimsAsync(thisRole);
                foreach (var permission in allPermissionsForThisRoles)
                {
                    permissionClaims.Add(permission);
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
            }
            .Union(userClaims)
            .Union(roleClaims)
            .Union(permissionClaims);

            return claims;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
               claims: claims,
               expires: DateTime.UtcNow.AddDays(2),
               signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var encryptedToken = tokenHandler.WriteToken(token);
            return encryptedToken;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var secret = Encoding.UTF8.GetBytes(_appConfig.Secret);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }

        #endregion
        public async Task<Result<RoleResponse>> GetRoleByIdAsync(string roleId)
        {
            var roles = await _roleManager.Roles.SingleOrDefaultAsync(x => x.Id == roleId);
            var rolesResponse = _mapper.Map<RoleResponse>(roles);
            return Result<RoleResponse>.Success(rolesResponse);
        }

        public async Task<Result<PermissionResponse>> GetRolePermissionsAsync(string roleId)
        {
            var model = new PermissionResponse();
            var allPermissions = new List<RoleClaimsResponse>();

            #region GetPermissions

            allPermissions.GetPermissions(typeof(Permissions.Users), roleId);
            allPermissions.GetPermissions(typeof(Permissions.Roles), roleId);
            
            #endregion GetPermissions

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                model.RoleId = role.Id;
                model.RoleName = role.Name;
                var claims = await _roleManager.GetClaimsAsync(role);
                var allClaimValues = allPermissions.Select(a => a.Value).ToList();
                var roleClaimValues = claims.Select(a => a.Value).ToList();
                var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
                foreach (var permission in allPermissions)
                {
                    if (authorizedClaims.Any(a => a == permission.Value))
                    {
                        permission.Selected = true;
                    }
                }
            }
            model.RoleClaims = allPermissions;
            return Result<PermissionResponse>.Success(model);
        }

        public async Task<int> GetRolesCountAsync()
        {
            var count = await _roleManager.Roles.CountAsync();
            return count;
        }

        public async Task<IResult<UserResponse>> GetUserAsync(string userId)
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var result = _mapper.Map<UserResponse>(user);
            return Result<UserResponse>.Success(result);
        }

        public async Task<int> GetUserCountAsync()
        {
            var count = await _userManager.Users.CountAsync();
            return count;
        }

        public async Task<IResult<UserRolesResponse>> GetUserRolesAsync(string userId)
        {
            var viewModel = new List<UserRoleModel>();
            var user = await _userManager.FindByIdAsync(userId);
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new UserRoleModel
                {
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                viewModel.Add(userRolesViewModel);
            }
            var result = new UserRolesResponse { UserRoles = viewModel };
            return Result<UserRolesResponse>.Success(result);
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Result<LoginResponse>.Fail("User Not Found.");
            }
            if (!user.IsActive)
            {
                return Result<LoginResponse>.Fail("User Not Active. Please contact the administrator.");
            }
            if (!user.EmailConfirmed)
            {
                return Result<LoginResponse>.Fail("E-Mail not confirmed.");
            }
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                return Result<LoginResponse>.Fail("Invalid Credentials.");
            }

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(user);

            var token = await GenerateJwtAsync(user);
            var response = new LoginResponse { Token = token, RefreshToken = user.RefreshToken};
            return Result<LoginResponse>.Success(response);
        }

        public async Task<IResult> RegisterUserAsync(RegisterRequest model)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(model.UserName);
            if (userWithSameUserName != null)
            {
                return Result.Fail($"Username '{model.UserName}' is already taken.");
            }
            var user = new ApplicationUser
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.ActivateUser,
                EmailConfirmed = model.AutoConfirmEmail
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Basic");
                    //if (!model.AutoConfirmEmail)
                    //{
                    //    var verificationUri = await SendVerificationEmail(user, origin);
                    //    BackgroundJob.Enqueue(() => _mailService.SendAsync(new MailRequest() { From = "mail@codewithmukesh.com", To = user.Email, Body = $"Please confirm your account by <a href='{verificationUri}'>clicking here</a>.", Subject = "Confirm Registration" }));
                    //    return Result<string>.Success(user.Id, message: $"User Registered. Please check your Mailbox to verify!");
                    //}
                    return Result<string>.Success(user.Id, message: $"User Registered");
                }
                else
                {
                    return Result.Fail(result.Errors.Select(a => a.Description).ToList());
                }
            }
            else
            {
                return Result.Fail($"Email {model.Email } is already registered.");
            }
        }

        public async Task<Result<string>> UpdateRolePermissionsAsync(PermissionRequest request)
        {
            var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId);
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role.Name == "Administrator" && !await _userManager.IsInRoleAsync(currentUser, role.Name))
            {
                return Result<string>.Fail($"Not allowed to modify Permissions for this Role.");
            }
            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
            var selectedClaims = request.RoleClaims.Where(a => a.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _roleManager.AddPermissionClaim(role, claim.Value);
            }
            return Result<string>.Success("Permission Updated.");
        }

        public async Task<IResult> UpdateUserRolesAsync(UpdateUserRolesRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user.Email == "admin@mail.com") return Result.Fail("Not Allowed.");
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            result = await _userManager.AddToRolesAsync(user, model.UserRoles.Where(x => x.Selected).Select(y => y.RoleName));
            return Result.Success("Roles Updated");
        }
    }
}
