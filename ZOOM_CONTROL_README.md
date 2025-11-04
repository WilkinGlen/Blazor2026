# Zoom Control Feature

This Blazor application includes a page zoom control built with **MudBlazor** that allows users to dynamically adjust the zoom level of the entire page.

## Features

- **MudBlazor Icon Buttons**: Modern Material Design zoom in/out buttons
- **MudSlider Control**: Smooth, accessible slider for precise zoom adjustment
- **MudChip Percentage Display**: Shows current zoom level (click to reset to 100%)
- **Keyboard Shortcuts**:
  - `Ctrl + +` or `Ctrl + =`: Zoom in
  - `Ctrl + -`: Zoom out
- `Ctrl + 0`: Reset zoom to 100%
- **Zoom Range**: 50% to 200%
- **Smooth Transitions**: CSS transitions for smooth zooming experience
- **Material Design**: Consistent UI with MudBlazor design system

## Dependencies

- **MudBlazor** (v8.13.0): Modern Blazor component library

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
A UI control component that uses MudBlazor components:
- `MudIconButton` for zoom in/out actions
- `MudSlider` for precise control
- `MudChip` for percentage display

**Location**: `Blazor2026/Components/Controls/ZoomControl.razor`

### ZoomContainer
A wrapper component that applies the zoom transformation to its child content.

**Location**: `Blazor2026/Components/Layout/ZoomContainer.razor`

## How It Works

1. MudBlazor services are registered in `Program.cs`
2. MudBlazor providers are added to `MainLayout.razor`
3. The `ZoomService` is registered as a singleton
4. The `MainLayout` includes the `ZoomControl` component in the header
5. The page `@Body` is wrapped in a `ZoomContainer` component
6. CSS transforms (`scale`) are applied to zoom the content
7. Keyboard shortcuts are registered via JavaScript interop in `wwwroot/js/zoom.js`

## Customization

### Change Zoom Limits
Edit `ZoomService.cs`:
```csharp
private const double MinZoom = 0.5;// Change minimum zoom
private const double MaxZoom = 2.0;  // Change maximum zoom
```

### Change Zoom Step
Edit `ZoomService.cs`:
```csharp
private const double ZoomStep = 0.1;  // Change zoom increment
```

### Customize MudBlazor Theme
Edit the `MudThemeProvider` in `MainLayout.razor` to apply custom themes:
```razor
<MudThemeProvider Theme="@_theme" />
```

### Style the Control
Edit `Blazor2026/Components/Controls/ZoomControl.razor.css` to customize appearance.

### Change Button Icons or Colors
Edit `ZoomControl.razor` to use different MudBlazor icons or color schemes:
```razor
<MudIconButton Icon="@Icons.Material.Filled.ZoomIn" Color="Color.Secondary" ... />
```

## MudBlazor Setup

The following MudBlazor configuration has been added:

**App.razor**:
- MudBlazor CSS reference
- MudBlazor JavaScript reference
- Roboto font (Material Design)

**Program.cs**:
- `AddMudServices()` registration

**MainLayout.razor**:
- `MudThemeProvider`
- `MudPopoverProvider`
- `MudDialogProvider`
- `MudSnackbarProvider`

**_Imports.razor**:
- `@using MudBlazor`

## Browser Compatibility

The zoom feature uses CSS transforms which are supported in all modern browsers:
- Chrome/Edge: ✓
- Firefox: ✓
- Safari: ✓

## Notes

- The zoom state is maintained globally across all pages
- The zoom resets when the application restarts
- MudBlazor provides WCAG 2.1 compliant accessible components
- For persistence across sessions, consider adding localStorage support
