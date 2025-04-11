using Microsoft.JSInterop;

namespace Omail.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "omail-theme";
    private bool _isDarkMode;

    public event Action? OnThemeChange;

    public bool IsDarkMode 
    {
        get => _isDarkMode;
        set 
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                OnThemeChange?.Invoke();
                UpdateThemeAsync();
            }
        }
    }

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
        _isDarkMode = theme == "dark";
        await UpdateThemeAsync();
    }

    private async Task UpdateThemeAsync()
    {
        var theme = _isDarkMode ? "dark" : "light";
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, theme);
        await _jsRuntime.InvokeVoidAsync("document.documentElement.classList.toggle", "dark", _isDarkMode);
    }

    public async Task ToggleThemeAsync()
    {
        IsDarkMode = !IsDarkMode;
    }
}
