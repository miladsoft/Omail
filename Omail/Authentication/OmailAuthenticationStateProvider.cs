using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Omail.Data;
using System.Security.Claims;

namespace Omail.Authentication
{
    public class OmailAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly AppDbContext _context;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public OmailAuthenticationStateProvider(ProtectedLocalStorage localStorage, AppDbContext context)
        {
            _localStorage = localStorage;
            _context = context;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userIdResult = await _localStorage.GetAsync<int>("userId");
                
                if (!userIdResult.Success)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var userId = userIdResult.Value;
                var user = await _context.Employees
                    .Include(e => e.Section)
                    .ThenInclude(s => s.Department)
                    .FirstOrDefaultAsync(e => e.Id == userId);
                
                if (user == null)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("DepartmentId", user.Section.DepartmentId.ToString()),
                    new Claim("SectionId", user.SectionId.ToString())
                };

                if (user.IsManager)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Manager"));
                }

                var identity = new ClaimsIdentity(claims, "OmailAuth");
                var claimsPrincipal = new ClaimsPrincipal(identity);

                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task NotifyUserAuthenticationAsync()
        {
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
    }
}
