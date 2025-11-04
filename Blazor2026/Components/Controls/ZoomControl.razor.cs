namespace Blazor2026.Components.Controls;

using Blazor2026.Services;
using Microsoft.AspNetCore.Components;

public partial class ZoomControl : IDisposable
{
    [Inject]
    public required ZoomService ZoomService { get; set; }

    protected override void OnInitialized()
    {
        this.ZoomService.OnZoomChanged += this.StateHasChanged;
    }

    private void OnSliderChange(ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out var value))
        {
            this.ZoomService.SetZoom(value);
        }
    }

    public void Dispose()
    {
        this.ZoomService.OnZoomChanged -= this.StateHasChanged;
        GC.SuppressFinalize(this);
    }
}
