namespace Blazor2026.Services;

public class ThemeService
{
    public event Action? OnThemeChanged;

    public bool IsDarkMode
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                this.OnThemeChanged?.Invoke();
            }
        }
    }
}
