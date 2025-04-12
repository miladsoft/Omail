using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Omail.Data;
using Omail.Data.Models;
using System.Security.Cryptography;
using System.Text;

namespace Omail.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            ProtectedSessionStorage sessionStorage,
            AuthenticationStateProvider authStateProvider,
            ILogger<AuthService> logger)
        {
            _context = context;
            _sessionStorage = sessionStorage;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        public async Task<Employee> GetCurrentUserAsync()
        {
            try
            {
                var userIdResult = await _sessionStorage.GetAsync<int>("userId");
                if (!userIdResult.Success)
                {
                    return null;
                }

                var userId = userIdResult.Value;
                return await _context.Employees
                    .Include(e => e.Section)
                    .ThenInclude(s => s.Department)
                    .FirstOrDefaultAsync(e => e.Id == userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _context.Employees
                    .Include(e => e.Section)
                    .ThenInclude(s => s.Department)
                    .FirstOrDefaultAsync(e => e.Email == email && e.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User with email {Email} not found or not active", email);
                    return false;
                }

                var hashedPassword = HashPassword(password);
                if (user.PasswordHash != hashedPassword)
                {
                    _logger.LogWarning("Login attempt failed: Invalid password for user {Email}", email);
                    return false;
                }

                // Store user data in session
                await _sessionStorage.SetAsync("userId", user.Id);
                await _sessionStorage.SetAsync("userEmail", user.Email);
                await _sessionStorage.SetAsync("userName", user.FullName);

                _logger.LogInformation("User {Email} logged in successfully", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", email);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _sessionStorage.DeleteAsync("userId");
            await _sessionStorage.DeleteAsync("userEmail");
            await _sessionStorage.DeleteAsync("userName");
        }

        public async Task<Employee> RegisterAsync(Employee employee, string password)
        {
            if (await _context.Employees.AnyAsync(u => u.Email == employee.Email))
                throw new Exception("An account with this email already exists.");

            employee.PasswordHash = HashPassword(password);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return employee;
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Employees.FindAsync(userId);
                if (user != null)
                {
                    // In a real application, you might want to implement soft delete
                    // or check for dependent records before deleting
                    _context.Employees.Remove(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"User with ID {userId} deleted successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {userId}");
                throw;
            }
        }

        public async Task DeleteSectionAsync(int sectionId)
        {
            try
            {
                var section = await _context.Sections.FindAsync(sectionId);
                if (section != null)
                {
                    // Check for employees in this section
                    var hasEmployees = await _context.Employees.AnyAsync(e => e.SectionId == sectionId);
                    if (hasEmployees)
                    {
                        throw new InvalidOperationException("Cannot delete section with assigned employees. Reassign employees first.");
                    }

                    _context.Sections.Remove(section);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Section with ID {sectionId} deleted successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting section with ID {sectionId}");
                throw;
            }
        }

        public async Task<Organization> GetOrganizationAsync()
        {
            try
            {
                var organization = await _context.Organizations
                    .Include(o => o.Departments)
                    .FirstOrDefaultAsync();
                    
                return organization ?? new Organization { Name = "Default Organization", Description = "Please set up your organization" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization");
                throw;
            }
        }

        public async Task<List<Department>> GetDepartmentsWithSectionsAsync()
        {
            try
            {
                return await _context.Departments
                    .Include(d => d.Sections)
                        .ThenInclude(s => s.Employees)
                    .Include(d => d.Organization)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments with sections");
                throw;
            }
        }

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            try
            {
                return await _context.Departments
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                throw;
            }
        }

        public async Task<List<Section>> GetSectionsAsync()
        {
            try
            {
                return await _context.Sections
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sections");
                throw;
            }
        }

        public async Task UpdateOrganizationAsync(Organization organization)
        {
            try
            {
                _context.Organizations.Update(organization);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization");
                throw;
            }
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return department;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                throw;
            }
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            try
            {
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department");
                throw;
            }
        }

        public async Task DeleteDepartmentAsync(int departmentId)
        {
            try
            {
                var department = await _context.Departments.FindAsync(departmentId);
                if (department == null) return;

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                throw;
            }
        }

        public async Task<Section> CreateSectionAsync(Section section)
        {
            try
            {
                _context.Sections.Add(section);
                await _context.SaveChangesAsync();
                return section;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating section");
                throw;
            }
        }

        public async Task UpdateSectionAsync(Section section)
        {
            try
            {
                _context.Sections.Update(section);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section");
                throw;
            }
        }

        public async Task<Employee?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.Employees
                    .Include(e => e.Section)
                    .FirstOrDefaultAsync(e => e.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by id");
                throw;
            }
        }

        public async Task<Employee> CreateUserAsync(UserModel model)
        {
            try
            {
                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Position = model.Position ?? string.Empty,
                    ProfilePicture = model.ProfilePicture ?? string.Empty,
                    IsManager = model.IsManager,
                    IsActive = model.IsActive,
                    SectionId = model.SectionId,
                    HireDate = model.HireDate,
                    PasswordHash = HashPassword(model.Password)
                };
                
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task UpdateUserAsync(UserModel model, bool changePassword = false)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(model.Id);
                if (employee == null) 
                    throw new Exception("User not found");

                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.Email = model.Email;
                employee.PhoneNumber = model.PhoneNumber ?? string.Empty;
                employee.Position = model.Position ?? string.Empty;
                employee.ProfilePicture = model.ProfilePicture ?? string.Empty;
                employee.IsManager = model.IsManager;
                employee.IsActive = model.IsActive;
                employee.SectionId = model.SectionId;
                employee.HireDate = model.HireDate;

                if (changePassword && !string.IsNullOrEmpty(model.Password))
                {
                    employee.PasswordHash = HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                throw;
            }
        }

        public async Task UpdateUserAsync(Employee employee)
        {
            try
            {
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                throw;
            }
        }

        public async Task<List<Employee>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Employees
                    .Include(e => e.Section)
                        .ThenInclude(s => s.Department)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        // For Dashboard Statistics
        public async Task<int> GetTotalUserCountAsync()
        {
            return await _context.Employees.CountAsync();
        }

        public async Task<List<Employee>> GetRecentUsersAsync(int count = 5)
        {
            return await _context.Employees
                .OrderByDescending(e => e.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTotalOrganizationsCountAsync()
        {
            return await _context.Organizations.CountAsync();
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        // Define UserModel class matching the one in UserEdit.razor
        public class UserModel
        {
            public int Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? Position { get; set; }
            public string? ProfilePicture { get; set; }
            public int SectionId { get; set; }
            public bool IsManager { get; set; }
            public bool IsActive { get; set; }
            public DateTime HireDate { get; set; } = DateTime.Today;
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
