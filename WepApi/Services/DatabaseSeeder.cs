using Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WepApi.Context;
using WepApi.Helpers;

namespace WepApi.Services
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DatabaseSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db, ILogger<DatabaseSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _logger = logger;
        }

        public void Seed()
        {
            AddAdministrator();
            AddBasicUser();
            _db.SaveChanges();
        }

        //private void AddCustomerPermissionClaims()
        //{
        //    Task.Run(async () =>
        //    {
        //        var adminRoleInDb = await _roleManager.FindByNameAsync("Administrator");
        //        if (adminRoleInDb != null)
        //        {
        //            await _roleManager.AddCustomPermissionClaim(adminRoleInDb, "Permissions.Users.Chat");
        //        }
        //    }).GetAwaiter().GetResult();
        //}

        private void AddAdministrator()
        {
            Task.Run(async () =>
            {
                //Check if Role Exists
                var adminRole = new IdentityRole("Administrator");
                var adminRoleInDb = await _roleManager.FindByNameAsync("Administrator");
                if (adminRoleInDb == null)
                {
                    await _roleManager.CreateAsync(adminRole);
                    _logger.LogInformation("Seeded Administrator Role.");
                }
                //Check if User Exists
                var superUser = new ApplicationUser
                {
                    FirstName = "Dev",
                    LastName = "Jnr",
                    Email = "admin@mail.com",
                    UserName = "Junju",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };
                var superUserInDb = await _userManager.FindByEmailAsync(superUser.Email);
                if (superUserInDb == null)
                {
                    await _userManager.CreateAsync(superUser, "Admin@1234");
                    var result = await _userManager.AddToRoleAsync(superUser, "Administrator");
                    if (result.Succeeded)
                    {
                        await _roleManager.GeneratePermissionClaimByModule(adminRole, PermissionModules.Users);
                        await _roleManager.GeneratePermissionClaimByModule(adminRole, PermissionModules.Roles);
                    }
                    _logger.LogInformation("Seeded User with Administrator Role.");
                }
            }).GetAwaiter().GetResult();
        }

        private void AddBasicUser()
        {
            Task.Run(async () =>
            {
                //Check if Role Exists
                var basicRole = new IdentityRole("Basic");
                var basicRoleInDb = await _roleManager.FindByNameAsync("Basic");
                if (basicRoleInDb == null)
                {
                    await _roleManager.CreateAsync(basicRole);
                    _logger.LogInformation("Seeded Basic Role.");
                }
                //Check if User Exists
                var basicUser = new ApplicationUser
                {
                    FirstName = "Basic",
                    LastName = "Jonny",
                    Email = "basic@mail.com",
                    UserName = "JonBasic",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };
                var basicUserInDb = await _userManager.FindByEmailAsync(basicUser.Email);
                if (basicUserInDb == null)
                {
                    await _userManager.CreateAsync(basicUser, "Admin@1234");
                    await _userManager.AddToRoleAsync(basicUser, "Basic");
                    _logger.LogInformation("Seeded User with Basic Role.");
                }
            }).GetAwaiter().GetResult();
        }
    }
}
