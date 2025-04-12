using Microsoft.EntityFrameworkCore;
using Omail.Data;
using Omail.Data.Models;
using System.Security.Cryptography;
using System.Text;

namespace Omail.Services
{
    public static class AuthServiceExtensions
    {
        public static async Task<Section> GetSectionByIdAsync(this AuthService authService, int id)
        {
            using var context = authService.CreateDbContext();
            return await context.Sections
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public static async Task<List<Employee>> GetEmployeesBySectionAsync(this AuthService authService, int sectionId)
        {
            using var context = authService.CreateDbContext();
            return await context.Employees
                .Where(e => e.SectionId == sectionId)
                .ToListAsync();
        }

        public static async Task<bool> UpdateProfileAsync(this AuthService authService, int userId, object profileData)
        {
            using var context = authService.CreateDbContext();
            var user = await context.Employees.FindAsync(userId);
            
            if (user == null)
                return false;

            // Use reflection to copy properties from profileData to user
            var properties = profileData.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var userProp = typeof(Employee).GetProperty(prop.Name);
                if (userProp != null && userProp.CanWrite)
                {
                    var value = prop.GetValue(profileData);
                    userProp.SetValue(user, value);
                }
            }

            await context.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> ChangePasswordAsync(this AuthService authService, int userId, string currentPassword, string newPassword)
        {
            using var context = authService.CreateDbContext();
            var user = await context.Employees.FindAsync(userId);
            
            if (user == null)
                return false;

            // Verify current password
            var currentHashedPassword = HashPassword(currentPassword);
            if (user.PasswordHash != currentHashedPassword)
                return false;

            // Update to new password
            user.PasswordHash = HashPassword(newPassword);
            await context.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateProfilePictureAsync(this AuthService authService, int userId, string imageData)
        {
            using var context = authService.CreateDbContext();
            var user = await context.Employees.FindAsync(userId);
            
            if (user == null)
                return false;

            user.ProfilePicture = imageData;
            await context.SaveChangesAsync();
            return true;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
