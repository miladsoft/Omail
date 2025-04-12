using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace Omail.Services
{
    public class ThemeService
    {
        private bool _isDarkMode;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly IJSRuntime _jsRuntime;
        
        public bool IsDarkMode => _isDarkMode;
        
        public event Action OnThemeChange;
        
        public ThemeService(ProtectedLocalStorage localStorage, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<bool>("darkMode");
                _isDarkMode = result.Success ? result.Value : false;
                await _jsRuntime.InvokeVoidAsync("setTheme", _isDarkMode);
            }
            catch
            {
                _isDarkMode = false;
                await _jsRuntime.InvokeVoidAsync("setTheme", false);
            }
        }
        
        public async Task ToggleThemeAsync()
        {
            _isDarkMode = !_isDarkMode;
            await _localStorage.SetAsync("darkMode", _isDarkMode);
            await _jsRuntime.InvokeVoidAsync("setTheme", _isDarkMode);
            OnThemeChange?.Invoke();
        }
    }
}
