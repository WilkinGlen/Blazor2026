# Dark Mode Toggle Feature

This Blazor application includes MudBlazor-based dark mode toggle functionality that allows users to switch between light and dark themes.

## Features

- **Toggle Button**: Icon button in the header to switch between light and dark modes
- **Dynamic Icons**: 
  - 🌙 Dark Mode icon (moon) shown in light mode
  - ☀️ Light Mode icon (sun) shown in dark mode
- **Persistent Theme**: MudBlazor theme applied across all components
- **Smooth Transitions**: MudBlazor handles theme transitions automatically

## Implementation

### Components

#### MainLayout.razor
The main layout component that hosts the theme provider and toggle button.

**Location**: `Blazor2026/Components/Layout/MainLayout.razor`

**Key Features**:
- `MudThemeProvider` with two-way binding to `_isDarkMode`
- `MudIconButton` for toggling dark mode
- Dynamic icon switching based on current mode
- Positioned in the header alongside zoom controls

#### MainLayout.razor.cs
Code-behind file managing dark mode state.

**Location**: `Blazor2026/Components/Layout/MainLayout.razor.cs`

**State**:
- `_isDarkMode` (bool) - Tracks current theme mode

**Methods**:
- `ToggleDarkMode()` - Toggles between light and dark modes

### Layout Structure

```
┌─────────────────────────────────────────┐
│ Header (zoom-control-header)      │
│  ┌────────────────────────────────────┐ │
│  │ ZoomControl    [Dark Mode Button] │ │
│  └────────────────────────────────────┘ │
├─────────────────────────────────────────┤
│               │
│ Main Content (ZoomContainer)    │
│        │
│           │
└─────────────────────────────────────────┘
```

## How It Works

1. **MudThemeProvider** is bound to `_isDarkMode` state variable
2. User clicks the icon button in the header
3. `ToggleDarkMode()` method toggles the `_isDarkMode` flag
4. MudBlazor automatically applies the corresponding theme to all MudBlazor components
5. Icon changes to reflect the new mode

## Usage

### For End Users

Click the sun/moon icon button in the top-right corner of the header:
- **Light Mode**: Click the moon icon (🌙) to switch to dark mode
- **Dark Mode**: Click the sun icon (☀️) to switch to light mode

### For Developers

The dark mode state is managed in `MainLayout.razor.cs`:

```csharp
private bool _isDarkMode;

private void ToggleDarkMode()
{
    this._isDarkMode = !this._isDarkMode;
}
```

To programmatically set the theme:
```csharp
// Set to dark mode
this._isDarkMode = true;

// Set to light mode
this._isDarkMode = false;
```

## Customization

### Custom Theme Colors

You can customize the dark and light theme colors by creating a custom theme:

```csharp
private MudTheme _theme = new()
{
    Palette = new PaletteLight()
    {
    Primary = Colors.Blue.Default,
        Secondary = Colors.Green.Default,
        // ... other light mode colors
    },
    PaletteDark = new PaletteDark()
    {
        Primary = Colors.Blue.Lighten1,
        Secondary = Colors.Green.Lighten1,
        // ... other dark mode colors
    }
};
```

Then pass it to the provider:
```razor
<MudThemeProvider @bind-IsDarkMode="@_isDarkMode" Theme="@_theme" />
```

### Persistence

To persist the user's theme preference across sessions, you can:

1. **LocalStorage** (Recommended):
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
 {
        var isDark = await JSRuntime.InvokeAsync<bool>("localStorage.getItem", "isDarkMode");
 _isDarkMode = isDark;
        StateHasChanged();
    }
}

private async Task ToggleDarkMode()
{
    _isDarkMode = !_isDarkMode;
    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "isDarkMode", _isDarkMode);
}
```

2. **User Preferences API** (Server-side):
Store the preference in a database and load it on user login.

## Styling Considerations

### Custom Components

For custom components (non-MudBlazor), use MudBlazor CSS variables to ensure dark mode compatibility:

```css
/* ✅ Correct - Uses CSS variables for automatic dark mode */
.my-component {
background-color: var(--mud-palette-surface);
    color: var(--mud-palette-text-primary);
    border: 1px solid var(--mud-palette-divider);
    box-shadow: var(--mud-elevation-2);
}

/* ❌ Incorrect - Hardcoded colors won't change with theme */
.my-component {
    background-color: white;
    color: black;
    border: 1px solid #e0e0e0;
}
```

### Updated Components

The following components have been updated to use MudBlazor CSS variables:

1. **MainLayout Header** (`MainLayout.razor.css`):
   - `background-color: var(--mud-palette-surface)`
   - `border-bottom: var(--mud-palette-divider)`

2. **ZoomControl** (`ZoomControl.razor.css`):
   - `background-color: var(--mud-palette-background-grey)`
   - `box-shadow: var(--mud-elevation-2)`

See [MUDBLAZOR_CSS_VARIABLES.md](MUDBLAZOR_CSS_VARIABLES.md) for a complete reference of available CSS variables.

### Current Limitations

- All components now use MudBlazor CSS variables for proper dark mode support
- Consider using MudBlazor components for automatic consistent theming

## Browser Support

Dark mode works in all modern browsers:
- Chrome/Edge: ✓
- Firefox: ✓
- Safari: ✓

## Future Enhancements

Potential improvements:
- **System Preference Detection**: Auto-detect OS dark mode preference
- **Persistent State**: Save preference to localStorage or user settings
- **Smooth Transitions**: Add custom transition effects
- **Custom Themes**: Multiple theme options beyond light/dark
- **Per-Component Theming**: Allow specific components to override theme
- **CSS Variable Integration**: Better integration with custom CSS

## Benefits

✅ **User Preference** - Respects user's visual preference  
✅ **Accessibility** - Reduces eye strain in low-light environments  
✅ **Modern UX** - Expected feature in modern applications  
✅ **MudBlazor Integration** - Seamless theming for all MudBlazor components  
✅ **Simple Implementation** - Single boolean flag controls entire theme  
✅ **Responsive** - Instant theme switching  

## MudBlazor Documentation

For more information about MudBlazor theming:
- [MudBlazor Themes Documentation](https://mudblazor.com/customization/default-theme)
- [MudBlazor Dark Mode](https://mudblazor.com/features/dark-mode)
