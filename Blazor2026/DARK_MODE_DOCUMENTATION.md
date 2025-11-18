# Dark Mode Implementation Guide

## Overview
This application now includes a fully functional dark mode toggle feature using MudBlazor's theming system.

## Features

### 1. Dark Mode Toggle Button
- Located in the top-right header next to the zoom controls
- Shows a sun icon (☀️) in light mode and moon icon (🌙) in dark mode
- Tooltip displays current mode and action ("Switch to Dark Mode" / "Switch to Light Mode")
- Instant theme switching with smooth transitions

### 2. Theme Service
The `ThemeService` manages the application's theme state:

```csharp
public class ThemeService
{
    public bool IsDarkMode { get; set; }
    public event Action? OnThemeChanged;
    public void ToggleDarkMode();
}
```

**Usage:**
```csharp
@inject ThemeService ThemeService

// Check current theme
bool isDark = ThemeService.IsDarkMode;

// Toggle theme
ThemeService.ToggleDarkMode();

// Listen to theme changes
ThemeService.OnThemeChanged += HandleThemeChange;
```

### 3. Custom Theme Configuration
The application uses custom color palettes for both light and dark modes:

**Light Mode:**
- Primary: Blue
- Background: White
- Surface: White

**Dark Mode:**
- Primary: Light Blue
- Background: #121212 (dark gray)
- Surface: #1e1e1e (slightly lighter dark gray)
- Text: White with varying opacity levels

## Implementation Details

### Components Modified

1. **ThemeService.cs**
   - Added `IsDarkMode` property with change notification
   - Added `ToggleDarkMode()` method
   - Implements event-based state management

2. **MainLayout.razor**
   - Added `MudThemeProvider` with two-way binding to `_isDarkMode`
   - Added toggle button in header with dynamic icon
   - Configured custom theme

3. **MainLayout.razor.cs**
   - Implements theme initialization
   - Handles theme change events
   - Configures custom `PaletteLight` and `PaletteDark`

4. **app.css**
   - Added smooth transitions for theme changes
   - Updated to use MudBlazor CSS variables
   - Improved header styling with proper dark mode support

## CSS Variables

The theme system uses the following CSS variables that automatically update:

- `--mud-palette-background`: Main background color
- `--mud-palette-surface`: Component surface color
- `--mud-palette-text-primary`: Primary text color
- `--mud-palette-divider`: Border and divider color

**Example usage in CSS:**
```css
.my-component {
    background-color: var(--mud-palette-surface);
    color: var(--mud-palette-text-primary);
    border-color: var(--mud-palette-divider);
    transition: background-color 0.3s ease, color 0.3s ease;
}
```

## Testing

Visit `/dark-mode-test` to see a diagnostic page that displays:
- Current CSS variable values
- Visual examples of themed components
- Real-time value updates when toggling theme

## Future Enhancements

Potential improvements to consider:

1. **Persistence**
   - Save theme preference to browser localStorage
   - Restore user's preference on page load

2. **System Preference Detection**
   - Auto-detect user's OS theme preference
   - Option to follow system theme

3. **Custom Themes**
   - Allow users to select from multiple theme options
   - Custom color pickers for personalization

4. **Animations**
   - Add smooth transition animations when switching themes
   - Consider using view transitions API for modern browsers

## Example Implementation for Persistence

```csharp
// In ThemeService.cs
private readonly IJSRuntime _jsRuntime;

public async Task LoadThemePreferenceAsync()
{
    var darkMode = await _jsRuntime.InvokeAsync<bool?>("localStorage.getItem", "darkMode");
    if (darkMode.HasValue)
    {
        IsDarkMode = darkMode.Value;
    }
}

public async Task SaveThemePreferenceAsync()
{
    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "darkMode", IsDarkMode);
}
```

## Troubleshooting

**Theme doesn't change:**
- Ensure `MudThemeProvider` is in the layout
- Check that `@bind-IsDarkMode` is properly bound
- Verify ThemeService is registered in Program.cs as a singleton

**Custom components don't respond to theme:**
- Use MudBlazor CSS variables instead of hard-coded colors
- Add transition CSS for smooth color changes
- Ensure components are using MudBlazor components or CSS variables

**State not persisting:**
- Dark mode currently resets on page refresh (by design)
- Implement localStorage persistence for permanent storage
