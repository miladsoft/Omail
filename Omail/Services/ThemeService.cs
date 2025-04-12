using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Omail.Services
{
    public class ThemeService
    {
        private bool _isDarkMode;
        private readonly ProtectedLocalStorage _localStorage;
        
        public bool IsDarkMode => _isDarkMode;
        
        public event Action OnThemeChange;
        
        public ThemeService(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<bool>("darkMode");
                _isDarkMode = result.Success ? result.Value : false;
            }
            catch
            {
                _isDarkMode = false;
            }
        }
        
        public async Task ToggleThemeAsync()
        {
            _isDarkMode = !_isDarkMode;
            await _localStorage.SetAsync("darkMode", _isDarkMode);
            OnThemeChange?.Invoke();
        }
    }
}
