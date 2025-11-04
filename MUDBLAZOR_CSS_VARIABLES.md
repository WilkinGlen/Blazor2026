# MudBlazor Dark Mode CSS Variables Reference

This document lists the MudBlazor CSS variables used in the application for dark mode support.

## CSS Variables Used

### Header Styling (MainLayout.razor.css)

| Variable | Purpose | Light Mode Value | Dark Mode Value |
|----------|---------|------------------|-----------------|
| `--mud-palette-surface` | Background color for surfaces like headers | White (#FFFFFF) | Dark grey (#1E1E1E) |
| `--mud-palette-divider` | Border and divider color | Light grey (#E0E0E0) | Dark grey (#4A4A4A) |

### Zoom Control Styling (ZoomControl.razor.css)

| Variable | Purpose | Light Mode Value | Dark Mode Value |
|----------|---------|------------------|-----------------|
| `--mud-palette-background-grey` | Background for UI controls | Light grey (#F5F5F5) | Darker grey (#2A2A2A) |
| `--mud-elevation-2` | Box shadow for elevation | Light shadow | Subtle dark shadow |

## Common MudBlazor CSS Variables

Here are additional MudBlazor CSS variables you can use:

### Colors
- `--mud-palette-primary` - Primary brand color
- `--mud-palette-secondary` - Secondary accent color
- `--mud-palette-background` - Page background
- `--mud-palette-surface` - Card/surface background
- `--mud-palette-appbar-background` - AppBar background
- `--mud-palette-drawer-background` - Drawer background

### Text
- `--mud-palette-text-primary` - Primary text color
- `--mud-palette-text-secondary` - Secondary text color
- `--mud-palette-text-disabled` - Disabled text color

### Borders & Dividers
- `--mud-palette-divider` - Border and divider lines
- `--mud-palette-divider-light` - Lighter divider

### Elevation (Shadows)
- `--mud-elevation-0` through `--mud-elevation-25` - Various shadow depths

### States
- `--mud-palette-action-default` - Default action color
- `--mud-palette-action-disabled` - Disabled state
- `--mud-palette-action-disabled-background` - Disabled background

## Usage Example

```css
.my-component {
    background-color: var(--mud-palette-surface);
    color: var(--mud-palette-text-primary);
    border: 1px solid var(--mud-palette-divider);
    box-shadow: var(--mud-elevation-4);
}
```

## Benefits of Using CSS Variables

✅ **Automatic Dark Mode** - No need to write separate dark mode CSS  
✅ **Consistency** - Matches MudBlazor component theming  
✅ **Maintainability** - Easy to update theme colors globally  
✅ **Accessibility** - Proper contrast ratios maintained  
✅ **Performance** - CSS variables are fast and efficient  

## Custom Components

When creating custom components, always use MudBlazor CSS variables instead of hardcoded colors to ensure they work in both light and dark modes.

### ❌ Don't Do This
```css
.my-header {
    background-color: white;
    color: black;
    border: 1px solid #e0e0e0;
}
```

### ✅ Do This Instead
```css
.my-header {
    background-color: var(--mud-palette-surface);
    color: var(--mud-palette-text-primary);
    border: 1px solid var(--mud-palette-divider);
}
```

## Documentation

For a complete list of MudBlazor CSS variables and theming options:
- [MudBlazor Theming Documentation](https://mudblazor.com/customization/default-theme)
- [MudBlazor CSS Variables](https://mudblazor.com/customization/css-variables)

## Testing Dark Mode

To test that your custom CSS works in dark mode:

1. Run the application
2. Click the dark mode toggle button (sun/moon icon)
3. Verify all custom styled elements update correctly
4. Check that text remains readable and contrast is appropriate
