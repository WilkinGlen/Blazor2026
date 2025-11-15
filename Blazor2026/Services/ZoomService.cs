namespace Blazor2026.Services;

public class ZoomService
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 1.5;

    public event Action? OnZoomChanged;

    private double _zoomLevel = 1.0;
    public double ZoomLevel
    {
        get => this._zoomLevel;
        private set
        {
            if (this._zoomLevel != value)
            {
                this._zoomLevel = value;
                OnZoomChanged?.Invoke();
            }
        }
    }

    public double MinimumZoom => MinZoom;
    public double MaximumZoom => MaxZoom;

    public void ZoomIn()
    {
        if (this.ZoomLevel < MaxZoom)
        {
            this.ZoomLevel = Math.Min(this.ZoomLevel + ZoomStep, MaxZoom);
        }
    }

    public void ZoomOut()
    {
        if (this.ZoomLevel > MinZoom)
        {
            this.ZoomLevel = Math.Max(this.ZoomLevel - ZoomStep, MinZoom);
        }
    }

    public void ResetZoom()
    {
        this.ZoomLevel = 1.0;
    }

    public void SetZoom(double level)
    {
        this.ZoomLevel = Math.Clamp(level, MinZoom, MaxZoom);
    }

    public int GetZoomPercentage()
    {
        return (int)Math.Round(this.ZoomLevel * 100);
    }
}
