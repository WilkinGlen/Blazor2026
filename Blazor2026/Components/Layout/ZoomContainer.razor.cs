namespace Blazor2026.Components.Layout;

using Blazor2026.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public partial class ZoomContainer : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<ZoomContainer>? dotNetHelper;

    [Inject]
    public required ZoomService ZoomService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        this.ZoomService.OnZoomChanged += this.StateHasChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this._module = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/zoom.js");
            this.dotNetHelper = DotNetObjectReference.Create(this);
            await this._module.InvokeVoidAsync("initializeZoomKeyboardShortcuts", this.dotNetHelper);
        }
    }

    [JSInvokable]
    public void ZoomIn()
    {
        this.ZoomService.ZoomIn();
    }

    [JSInvokable]
    public void ZoomOut()
    {
        this.ZoomService.ZoomOut();
    }

    [JSInvokable]
    public void ZoomReset()
    {
        this.ZoomService.ResetZoom();
    }

    public async ValueTask DisposeAsync()
    {
        this.ZoomService.OnZoomChanged -= this.StateHasChanged;

        if (this._module != null)
        {
            await this._module.DisposeAsync();
        }

        this.dotNetHelper?.Dispose();
        GC.SuppressFinalize(this);
    }
}
