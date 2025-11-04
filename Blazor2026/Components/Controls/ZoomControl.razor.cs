namespace Blazor2026.Components.Controls;

using Blazor2026.Services;
using Microsoft.AspNetCore.Components;

public partial class ZoomControl : IDisposable
{
    [Inject]
    public required ZoomService ZoomService { get; set; }

    [Inject]
    public required ThemeService ThemeService { get; set; }

    protected override void OnInitialized()
    {
        this.ZoomService.OnZoomChanged += this.StateHasChanged;
        this.ThemeService.OnThemeChanged += this.StateHasChanged;
    }

    private void OnSliderChange(double value)
    {
        this.ZoomService.SetZoom(value);
    }

    private string GetBackgroundColor()
    {
        return this.ThemeService.IsDarkMode ? "#2a2a2a" : "#f5f5f5";
    }

    public void Dispose()
    {
        this.ZoomService.OnZoomChanged -= this.StateHasChanged;
        this.ThemeService.OnThemeChanged -= this.StateHasChanged;
        GC.SuppressFinalize(this);
    }
}
