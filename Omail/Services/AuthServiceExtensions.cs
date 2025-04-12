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
            return await authService.Context.Sections
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public static async Task<List<Employee>> GetEmployeesBySectionAsync(this AuthService authService, int sectionId)
        {
            return await authService.Context.Employees
                .Where(e => e.SectionId == sectionId)
                .ToListAsync();
        }

        public static async Task<bool> UpdateProfileAsync(this AuthService authService, int userId, object profileData)
        {
            var user = await authService.Context.Employees.FindAsync(userId);
            
            if (user == null)
                return false;

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

            await authService.Context.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> ChangePasswordAsync(this AuthService authService, int userId, string currentPassword, string newPassword)
        {
            var user = await authService.Context.Employees.FindAsync(userId);
            
            if (user == null)
                return false;

            var currentHashedPassword = HashPassword(currentPassword);
            if (user.PasswordHash != currentHashedPassword)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            await authService.Context.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateProfilePictureAsync(this AuthService authService, int userId, string imageData)
        {
            var user = await authService.Context.Employees.FindAsync(userId);
            
            if (user == null)
                return false;

            user.ProfilePicture = imageData;
            await authService.Context.SaveChangesAsync();
            return true;
        }

        public static async Task<List<Section>> GetSectionsByDepartmentAsync(this AuthService authService, int departmentId)
        {
            return await authService.Context.Sections
                .Where(s => s.DepartmentId == departmentId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
