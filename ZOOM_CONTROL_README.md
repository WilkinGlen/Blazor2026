# Zoom Control Feature

This Blazor application includes a page zoom control that allows users to dynamically adjust the zoom level of the entire page.

## Features

- **Zoom In/Out Buttons**: Click the `+` and `−` buttons to zoom in and out
- **Slider Control**: Use the slider for precise zoom adjustment
- **Percentage Display**: Shows current zoom level (click to reset to 100%)
- **Keyboard Shortcuts**:
  - `Ctrl + +` or `Ctrl + =`: Zoom in
  - `Ctrl + -`: Zoom out
  - `Ctrl + 0`: Reset zoom to 100%
- **Zoom Range**: 50% to 200%
- **Smooth Transitions**: CSS transitions for smooth zooming experience

## Components

### ZoomService
A singleton service that manages the zoom state across the application.

**Location**: `Blazor2026/Services/ZoomService.cs`

**Methods**:
- `ZoomIn()`: Increase zoom by 10%
- `ZoomOut()`: Decrease zoom by 10%
- `ResetZoom()`: Reset to 100%
- `SetZoom(double level)`: Set specific zoom level
- `GetZoomPercentage()`: Get current zoom as percentage

### ZoomControl
A UI control component that provides buttons, slider, and percentage display.

**Location**: `Blazor2026/Components/Controls/ZoomControl.razor`

### ZoomContainer
A wrapper component that applies the zoom transformation to its child content.

**Location**: `Blazor2026/Components/Layout/ZoomContainer.razor`

## How It Works

1. The `ZoomService` is registered as a singleton in `Program.cs`
2. The `MainLayout` includes the `ZoomControl` component in the header
3. The page `@Body` is wrapped in a `ZoomContainer` component
4. CSS transforms (`scale`) are applied to zoom the content
5. Keyboard shortcuts are registered via JavaScript interop

## Customization

### Change Zoom Limits
Edit `ZoomService.cs`:
```csharp
private const double MinZoom = 0.5;  // Change minimum zoom
private const double MaxZoom = 2.0;  // Change maximum zoom
```

### Change Zoom Step
Edit `ZoomService.cs`:
```csharp
private const double ZoomStep = 0.1;  // Change zoom increment
```

### Style the Control
Edit `Blazor2026/Components/Controls/ZoomControl.razor.css` to customize appearance.

### Position the Control
Edit `MainLayout.razor` to reposition the zoom control (currently in the header).

## Browser Compatibility

The zoom feature uses CSS transforms which are supported in all modern browsers:
- Chrome/Edge: ✓
- Firefox: ✓
- Safari: ✓

## Notes

- The zoom state is maintained globally across all pages
- The zoom resets when the application restarts
- For persistence across sessions, consider adding localStorage support
