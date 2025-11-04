namespace Blazor2026.Services;

public class ZoomService
{
    private double zoomLevel = 1.0;
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 1.5;

    public event Action? OnZoomChanged;

    public double ZoomLevel
    {
        get => this.zoomLevel;
        private set
        {
            if (this.zoomLevel != value)
            {
                this.zoomLevel = value;
                OnZoomChanged?.Invoke();
            }
        }
    }

    public double MinimumZoom => MinZoom;
    public double MaximumZoom => MaxZoom;

    public void ZoomIn()
    {
        if (this.zoomLevel < MaxZoom)
        {
            this.ZoomLevel = Math.Min(this.zoomLevel + ZoomStep, MaxZoom);
        }
    }

    public void ZoomOut()
    {
        if (this.zoomLevel > MinZoom)
        {
            this.ZoomLevel = Math.Max(this.zoomLevel - ZoomStep, MinZoom);
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
        return (int)Math.Round(this.zoomLevel * 100);
    }
}
