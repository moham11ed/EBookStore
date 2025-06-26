using Microsoft.AspNetCore.Identity;
using BookShoppingCartMvcUI.Models;
using Microsoft.Extensions.Logging;

namespace BookShoppingCartMvcUI.Data
{
    public class DbSeeder // إزالة static
    {
        private readonly UserManager<IdentityUser> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(UserManager<IdentityUser> userMgr, RoleManager<IdentityRole> roleMgr,
                       ApplicationDbContext context, ILogger<DbSeeder> logger)
        {
            _userMgr = userMgr ?? throw new ArgumentNullException(nameof(userMgr));
            _roleMgr = roleMgr ?? throw new ArgumentNullException(nameof(roleMgr));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedDefaultData()
        {
            // إضافة الأدوار
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await _roleMgr.RoleExistsAsync(role))
                {
                    var result = await _roleMgr.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create role {Role}: {Errors}", role, string.Join(", ", result.Errors));
                    }
                }
            }

            // إضافة مستخدم Admin
            var adminEmail = "admin@gmail.com";
            var adminUser = await _userMgr.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await _userMgr.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await _userMgr.AddToRoleAsync(newAdmin, "Admin");
                }
                else
                {
                    _logger.LogError("Failed to create admin user {Email}: {Errors}", adminEmail, string.Join(", ", result.Errors));
                }
            }

            // إضافة بيانات أخرى للكتب أو الأنواع
            if (!_context.Genres.Any())
            {
                _context.Genres.AddRange(new List<Genre>
                {
                    new Genre { GenreName = "Adventure" }, // تأكد من تهيئة GenreName
                    new Genre { GenreName = "Science Fiction" }
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}