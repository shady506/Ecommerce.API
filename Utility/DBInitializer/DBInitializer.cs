using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace  Ecommerce.API.Utility.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DBInitializer> _logger;

        public DBInitializer(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<DBInitializer> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    _roleManager.CreateAsync(new(SD.SuperAdminRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.AdminRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.CompanyRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.CustomerRole)).GetAwaiter().GetResult();

                    var result = _userManager.CreateAsync(new()
                    {
                        Email = "SuperAdmin@EraaSoft.com",
                        EmailConfirmed = true,
                        UserName = "SuperAdmin",
                        Name = "Super Admin"
                    }, "Admin123@").GetAwaiter().GetResult();

                    var user = _userManager.FindByEmailAsync("SuperAdmin@EraaSoft.com").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(user, SD.SuperAdminRole).GetAwaiter().GetResult();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError("Check connection. Use DB on local server (.)");
            }
        }
    }
}
