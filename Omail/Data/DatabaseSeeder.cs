using Microsoft.EntityFrameworkCore;
using Omail.Data.Models;
using System.Security.Cryptography;
using System.Text;

namespace Omail.Data
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // First create organization if it doesn't exist
                Organization organization = await _context.Organizations
                    .OrderBy(o => o.Id)
                    .FirstOrDefaultAsync();
                    
                if (organization == null)
                {
                    _logger.LogInformation("Creating default organization...");
                    organization = new Organization
                    {
                        Name = "Omail Corporation",
                        Description = "Default Organization"
                    };
                    _context.Organizations.Add(organization);
                    await _context.SaveChangesAsync();
                }

                // Seed departments if none exist
                Department adminDepartment = await _context.Departments
                    .OrderBy(d => d.Id)
                    .FirstOrDefaultAsync(d => d.Name == "Administration");

                if (adminDepartment == null)
                {
                    _logger.LogInformation("Creating Administration department...");
                    adminDepartment = new Department
                    {
                        Name = "Administration",
                        Description = "System Administration Department",
                        OrganizationId = organization.Id
                    };
                    _context.Departments.Add(adminDepartment);
                    await _context.SaveChangesAsync();
                }

                // Get or create admin section
                Section adminSection = await _context.Sections
                    .OrderBy(s => s.Id)
                    .FirstOrDefaultAsync(s => s.Name == "System Administrators");
                
                if (adminSection == null)
                {
                    _logger.LogInformation("Creating System Administrators section...");
                    adminSection = new Section
                    {
                        Name = "System Administrators",
                        Description = "System Administrators Section",
                        DepartmentId = adminDepartment.Id
                    };
                    _context.Sections.Add(adminSection);
                    await _context.SaveChangesAsync();
                }

                // Verify the section was created successfully
                adminSection = await _context.Sections
                    .OrderBy(s => s.Id)
                    .FirstOrDefaultAsync(s => s.Name == "System Administrators");

                if (adminSection == null)
                {
                    _logger.LogError("Failed to create System Administrators section");
                    return;
                }

                // Seed admin user
                var adminUser = await _context.Employees
                    .OrderBy(e => e.Id)
                    .FirstOrDefaultAsync(e => e.Email == "admin@omail.com");

                if (adminUser == null)
                {
                    _logger.LogInformation("Creating admin user...");
                    
                    adminUser = new Employee
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@omail.com",
                        PasswordHash = HashPassword("Admin@123"),
                        PhoneNumber = "123-456-7890",
                        Position = "Administrator",
                        ProfilePicture = "",
                        IsManager = true,
                        IsActive = true,
                        SectionId = adminSection.Id,
                        HireDate = DateTime.UtcNow
                    };
                    
                    _context.Employees.Add(adminUser);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Admin user created successfully with email: admin@omail.com and password: Admin@123");
                }
                else
                {
                    // Ensure admin user has correct password (in case it was changed and you need to reset it)
                    adminUser.PasswordHash = HashPassword("Admin@123");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Admin user password reset to: Admin@123");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
