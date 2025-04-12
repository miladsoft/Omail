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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
