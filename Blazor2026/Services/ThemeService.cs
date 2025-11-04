namespace Blazor2026.Services;

public class ThemeService
{
    private bool _isDarkMode;

    public event Action? OnThemeChanged;

    public bool IsDarkMode
    {
        get => this._isDarkMode;
        set
        {
            if (this._isDarkMode != value)
            {
                this._isDarkMode = value;
                this.OnThemeChanged?.Invoke();
            }
        }
    }
}
