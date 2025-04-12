namespace Omail.Services
{
    public class ThemeService
    {
        private bool _isDarkMode = false;

        public event Action OnThemeChange;

        public bool IsDarkMode => _isDarkMode;

        public ThemeService()
        {
            // Default to light mode - no JavaScript needed
        }

        // This method doesn't use JavaScript and can be called anytime
        public void InitializeTheme()
        {
            // Set a default theme (light mode)
            _isDarkMode = false;
        }

        public void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            NotifyThemeChanged();
        }

        public void SetTheme(bool darkMode)
        {
            if (_isDarkMode != darkMode)
            {
                _isDarkMode = darkMode;
                NotifyThemeChanged();
            }
        }

        public string GetThemeClass()
        {
            return _isDarkMode ? "dark" : "";
        }

        private void NotifyThemeChanged()
        {
            OnThemeChange?.Invoke();
        }
    }
}
