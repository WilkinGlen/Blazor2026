namespace Blazor2026.Components.Layout;

using Blazor2026.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public partial class MainLayout : IDisposable
{
    private bool _isDarkMode;
    private MudTheme _theme = new();

    [Inject]
    public required ThemeService ThemeService { get; set; }

    protected override void OnInitialized()
    {
        this._isDarkMode = this.ThemeService.IsDarkMode;
        this.ThemeService.OnThemeChanged += this.HandleThemeChanged;
        
        // Configure custom theme
        this._theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = Colors.Blue.Default,
                AppbarBackground = Colors.Blue.Default,
            },
            PaletteDark = new PaletteDark
            {
                Primary = Colors.Blue.Lighten1,
                AppbarBackground = Colors.Gray.Darken4,
                Surface = "#1e1e1e",
                Background = "#121212",
                BackgroundGray = "#1a1a1a",
                DrawerBackground = "#1e1e1e",
                DrawerText = "rgba(255,255,255, 0.7)",
                AppbarText = "rgba(255,255,255, 0.7)",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.6)",
                ActionDefault = "rgba(255,255,255, 0.7)",
                ActionDisabled = "rgba(255,255,255, 0.3)",
                Divider = "rgba(255,255,255, 0.12)",
            }
        };
    }

    private void HandleThemeChanged()
    {
        this._isDarkMode = this.ThemeService.IsDarkMode;
        this.StateHasChanged();
    }

    private void ToggleTheme()
    {
        this.ThemeService.ToggleDarkMode();
    }

    public void Dispose()
    {
        this.ThemeService.OnThemeChanged -= this.HandleThemeChanged;
        GC.SuppressFinalize(this);
    }
}
