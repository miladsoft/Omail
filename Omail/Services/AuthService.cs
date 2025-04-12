using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Omail.Authentication;
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

        // Add property to expose context for extension methods
        internal AppDbContext Context => _context;

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

        /// <summary>
        /// Gets all departments from the database
        /// </summary>
        /// <returns>A list of all departments</returns>
        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            try
            {
                return await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving departments");
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

        public async Task<List<Employee>> GetUsersAsync()
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
                _logger?.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task ActivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Employees.FindAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                    
                user.IsActive = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error activating user {UserId}", userId);
                throw;
            }
        }

        public async Task DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Employees.FindAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                    
                user.IsActive = false;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deactivating user {UserId}", userId);
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

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            return password;
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            try
            {
                var userIdResult = await _sessionStorage.GetAsync<int>("userId");
                return userIdResult.Success ? userIdResult.Value : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(int userId)
        {
            try
            {
                var user = await _context.Employees.FindAsync(userId);
                if (user == null)
                    return false;
                    
                // Generate a random password
                string newPassword = GenerateRandomPassword();
                
                // Update password hash in database
                user.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();
                
                // Send email with new password (in a real app)
                // This would typically call an email service
                // For now, just log the action
                _logger?.LogInformation($"Password reset for user {user.Email}. New password would be sent via email.");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error resetting user password");
                throw;
            }
        }

        public async Task<bool> SetUserActiveStatusAsync(int userId, bool isActive)
        {
            try
            {
                var user = await _context.Employees.FindAsync(userId);
                if (user == null)
                    return false;
                    
                user.IsActive = isActive;
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting user active status");
                throw;
            }
        }

        /// <summary>
        /// Allows an administrator to impersonate another user
        /// </summary>
        /// <param name="userId">The ID of the user to impersonate</param>
        /// <returns>True if impersonation was successful, false otherwise</returns>
        public async Task<bool> ImpersonateUserAsync(int userId)
        {
            try
            {
                // Find the user to impersonate
                var userToImpersonate = await _context.Employees
                    .Include(e => e.Section)
                    .ThenInclude(s => s.Department)
                    .FirstOrDefaultAsync(e => e.Id == userId && e.IsActive);
                
                if (userToImpersonate == null)
                {
                    _logger?.LogWarning($"Attempt to impersonate non-existent or inactive user with ID: {userId}");
                    return false;
                }
                
                // Get the current user ID from the session storage
                var currentUserId = await GetCurrentUserIdAsync();
                
                // Store the original user ID in session for later restoration
                if (currentUserId.HasValue)
                {
                    // Save original user ID so we can switch back later
                    await _sessionStorage.SetAsync("originalUserId", currentUserId.Value);
                    await _sessionStorage.SetAsync("isImpersonating", true);
                }
                
                // Set the session to the impersonated user
                await _sessionStorage.SetAsync("userId", userToImpersonate.Id);
                await _sessionStorage.SetAsync("userEmail", userToImpersonate.Email);
                await _sessionStorage.SetAsync("userName", userToImpersonate.FullName);
                
                // Log the impersonation
                _logger?.LogInformation($"User {currentUserId} is now impersonating user {userToImpersonate.Id} ({userToImpersonate.Email})");
                
                // Notify authentication state has changed
                if (_authStateProvider is AuthenticationStateProvider authProvider)
                {
                    await authProvider.GetAuthenticationStateAsync();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error during user impersonation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ends the current impersonation session and returns to the original user
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> EndImpersonationAsync()
        {
            try
            {
                var isImpersonating = await _sessionStorage.GetAsync<bool>("isImpersonating");
                
                if (!isImpersonating.Success || !isImpersonating.Value)
                {
                    return false;
                }
                
                var originalUserIdResult = await _sessionStorage.GetAsync<int>("originalUserId");
                
                if (!originalUserIdResult.Success)
                {
                    return false;
                }
                
                int originalUserId = originalUserIdResult.Value;
                
                var originalUser = await _context.Employees.FindAsync(originalUserId);
                
                if (originalUser == null)
                {
                    return false;
                }
                
                // Restore original user
                await _sessionStorage.SetAsync("userId", originalUser.Id);
                await _sessionStorage.SetAsync("userEmail", originalUser.Email);
                await _sessionStorage.SetAsync("userName", originalUser.FullName);
                
                // Clear impersonation flags
                await _sessionStorage.DeleteAsync("isImpersonating");
                await _sessionStorage.DeleteAsync("originalUserId");
                
                // Notify authentication state has changed
                await ((OmailAuthenticationStateProvider)_authStateProvider).NotifyUserAuthenticationAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error ending impersonation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a password reset email to a user
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
                if (user == null)
                {
                    _logger?.LogWarning($"Password reset attempt for non-existent or inactive user: {email}");
                    return false;
                }
                
                // Generate a secure random token
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "")
                    .Substring(0, 24);
                
                // Store token with expiration in a real implementation
                // For demonstration purposes, we'll just log it
                _logger?.LogInformation($"Generated password reset token for {email}: {token}");
                
                // In a real app, send an actual email with a link containing the token
                // For now, we'll just simulate success
                _logger?.LogInformation($"Password reset email would be sent to {email}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error sending password reset email: {ex.Message}");
                throw;
            }
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
