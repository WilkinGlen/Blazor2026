namespace Blazor2026.Components.Layout;

using Blazor2026.Services;
using Microsoft.AspNetCore.Components;

public partial class MainLayout : IDisposable
{
    [Inject]
    public required ThemeService ThemeService { get; set; }

    protected override void OnInitialized()
    {
        this.ThemeService.OnThemeChanged += this.StateHasChanged;
    }

    public void Dispose()
    {
        this.ThemeService.OnThemeChanged -= this.StateHasChanged;
        GC.SuppressFinalize(this);
    }
}
