namespace Blazor2026.Components.Layout;

using Blazor2026.Services;
using Microsoft.AspNetCore.Components;

public partial class MainLayout : IDisposable
{
    [Inject]
    public required ThemeService ThemeService { get; set; }

    private bool IsDarkMode
    {
        get => this.ThemeService.IsDarkMode;
        set => this.ThemeService.IsDarkMode = value;
    }

    protected override void OnInitialized()
    {
        this.ThemeService.OnThemeChanged += this.StateHasChanged;
    }

    private void ToggleDarkMode()
    {
        this.IsDarkMode = !this.IsDarkMode;
    }

    public void Dispose()
    {
        this.ThemeService.OnThemeChanged -= this.StateHasChanged;
        GC.SuppressFinalize(this);
    }
}
