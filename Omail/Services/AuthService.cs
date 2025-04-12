using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Omail.Data;
using Omail.Data.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Omail.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(
            AppDbContext context,
            ProtectedLocalStorage localStorage,
            AuthenticationStateProvider authStateProvider)
        {
            _context = context;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<Employee> GetCurrentUserAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated != true)
                return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                return null;

            return await _context.Employees
                .Include(e => e.Section)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _context.Employees
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
                return false;

            if (!VerifyPasswordHash(password, user.PasswordHash))
                return false;

            // Store user info in local storage
            await _localStorage.SetAsync("userId", user.Id);
            await _localStorage.SetAsync("userEmail", user.Email);
            await _localStorage.SetAsync("userName", user.FullName);

            return true;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.DeleteAsync("userId");
            await _localStorage.DeleteAsync("userEmail");
            await _localStorage.DeleteAsync("userName");
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

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == storedHash;
        }
    }
}
