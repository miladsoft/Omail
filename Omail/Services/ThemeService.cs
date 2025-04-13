using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Omail.Services
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public bool IsDarkMode { get; set; }
    }

    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isDarkMode;
        private bool _isSystemTheme;

        public bool IsDarkMode => _isDarkMode;
        public bool IsSystemTheme => _isSystemTheme;

        public event EventHandler<ThemeChangedEventArgs>? OnThemeChange;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            // Default to dark mode until browser preferences are loaded
            _isDarkMode = true;
            _isSystemTheme = true;
        }

        public async Task InitializeThemeAsync()
        {
            try
            {
                var prefersDarkMode = await _jsRuntime.InvokeAsync<bool>("appFunctions.prefersDarkMode");
                _isDarkMode = prefersDarkMode;
                _isSystemTheme = true;
                OnThemeChange?.Invoke(this, new ThemeChangedEventArgs { IsDarkMode = _isDarkMode });
            }
            catch
            {
                // If JavaScript interop fails, we keep the default values
                // This could happen during prerendering
            }
        }

        public async Task SetDarkMode(bool isDarkMode)
        {
            _isDarkMode = isDarkMode;
            _isSystemTheme = false;
            
            try
            {
                await _jsRuntime.InvokeVoidAsync("appFunctions.setThemePreference", isDarkMode);
            }
            catch
            {
                // Handle the case where JavaScript interop isn't available
            }
            
            OnThemeChange?.Invoke(this, new ThemeChangedEventArgs { IsDarkMode = isDarkMode });
        }

        public async Task ToggleDarkMode()
        {
            await SetDarkMode(!_isDarkMode);
        }

        public async Task SetSystemTheme(bool isSystemTheme)
        {
            _isSystemTheme = isSystemTheme;
            if (isSystemTheme)
            {
                try
                {
                    var prefersDarkMode = await _jsRuntime.InvokeAsync<bool>("appFunctions.prefersDarkMode");
                    await SetDarkMode(prefersDarkMode);
                }
                catch
                {
                    // If JavaScript interop fails, keep current theme
                }
            }
        }
    }
}
