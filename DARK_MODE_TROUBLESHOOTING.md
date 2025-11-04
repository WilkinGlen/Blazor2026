# Dark Mode Troubleshooting Guide

This guide helps resolve common dark mode issues in the Blazor2026 application.

## Problem: Header Not Changing to Dark Mode

### Symptoms
- The header (`zoom-control-header`) stays white/light even when dark mode is toggled
- Only MudBlazor components change theme, custom CSS doesn't respond

### Root Cause
Blazor's scoped CSS can sometimes create specificity conflicts or the CSS variables might not penetrate through component boundaries properly.

### Solution Applied

We implemented **multiple layers** of dark mode support to ensure it works:

#### 1. Scoped CSS with ::deep (MainLayout.razor.css)
```css
::deep .zoom-control-header {
    background-color: var(--mud-palette-surface);
    border-bottom: 1px solid var(--mud-palette-divider);
}
```

#### 2. Global CSS with !important (app.css)
```css
.zoom-control-header {
    background-color: var(--mud-palette-surface) !important;
    border-bottom: 1px solid var(--mud-palette-divider) !important;
    transition: background-color 0.3s ease, border-color 0.3s ease;
}

.zoom-control {
 background-color: var(--mud-palette-background-grey) !important;
    transition: background-color 0.3s ease;
}
```

### Why This Works

1. **`::deep` selector** penetrates component boundaries in scoped CSS
2. **Global CSS** in `app.css` applies to all elements regardless of scope
3. **`!important`** overrides any other conflicting styles
4. **CSS transitions** provide smooth theme switching
5. **MudBlazor CSS variables** automatically change when theme toggles

## Verification Steps

1. **Clear browser cache** (Ctrl+Shift+Delete or Cmd+Shift+Delete)
2. **Hard refresh** (Ctrl+F5 or Cmd+Shift+R)
3. **Check browser DevTools**:
   - Inspect the header element
   - Look for `background-color: var(--mud-palette-surface)`
 - Verify the computed value changes when toggling dark mode
   - Check if the variable resolves to a color value

4. **Verify MudBlazor is working**:
   - Toggle dark mode
   - Check if MudBlazor buttons/components change theme
   - If they don't, MudThemeProvider might not be set up correctly

## Common Issues & Fixes

### Issue 1: CSS Variables Not Defined

**Symptom**: Browser shows `var(--mud-palette-surface)` but it doesn't resolve to a color

**Fix**: Ensure `MudThemeProvider` is at the root of your layout:
```razor
<MudThemeProvider @bind-IsDarkMode="@_isDarkMode" />
```

### Issue 2: Scoped CSS Taking Priority

**Symptom**: Styles in component `.css` files override global styles

**Fix**: 
- Use `!important` in global CSS
- Or use `::deep` in scoped CSS
- Or move all dark mode styles to `app.css`

### Issue 3: Browser Caching Old Styles

**Symptom**: Changes to CSS files don't appear

**Fix**:
1. Stop the application
2. Clean the solution: `dotnet clean`
3. Rebuild: `dotnet build`
4. Hard refresh browser (Ctrl+F5)
5. Clear browser cache

### Issue 4: CSS Variable Not Updating

**Symptom**: The variable exists but doesn't change value when toggling

**Fix**: Check if `@bind-IsDarkMode` is properly bound in `MudThemeProvider`:
```razor
<MudThemeProvider @bind-IsDarkMode="@_isDarkMode" />
```

And the code-behind has:
```csharp
private bool _isDarkMode;

private void ToggleDarkMode()
{
    _isDarkMode = !_isDarkMode;
}
```

## Testing Dark Mode

### Manual Test Checklist

- [ ] Header background changes (white → dark grey)
- [ ] Header border changes (light grey → dark grey)
- [ ] Zoom control background changes
- [ ] MudBlazor buttons change
- [ ] Text remains readable in both modes
- [ ] Icon changes (moon ↔ sun)
- [ ] Transitions are smooth

### Browser DevTools Test

1. Open DevTools (F12)
2. Inspect the `<header class="zoom-control-header">` element
3. In the Styles panel, look for:
   ```css
   background-color: var(--mud-palette-surface);
   ```
4. Click on the variable name to see its computed value
5. Toggle dark mode
6. Watch the computed value change

### Expected Values

| Variable | Light Mode | Dark Mode |
|----------|-----------|-----------|
| `--mud-palette-surface` | `#ffffff` (white) | `#1e1e1e` (dark grey) |
| `--mud-palette-divider` | `#e0e0e0` (light grey) | `#4a4a4a` (dark grey) |
| `--mud-palette-background-grey` | `#f5f5f5` | `#2a2a2a` |

## Files Modified for Dark Mode

1. **Blazor2026/Components/Layout/MainLayout.razor**
   - Added `@bind-IsDarkMode` to `MudThemeProvider`
   - Added dark mode toggle button

2. **Blazor2026/Components/Layout/MainLayout.razor.cs**
   - Added `_isDarkMode` field
   - Added `ToggleDarkMode()` method

3. **Blazor2026/Components/Layout/MainLayout.razor.css**
   - Used `::deep` selector
   - Applied MudBlazor CSS variables

4. **Blazor2026/wwwroot/app.css** ⭐ **Critical for fixing header**
   - Added global rules with `!important`
   - Applied to both `.zoom-control-header` and `.zoom-control`

5. **Blazor2026/Components/Controls/ZoomControl.razor.css**
   - Changed to MudBlazor CSS variables

## Alternative Solutions

If the above doesn't work, try these alternatives:

### Option 1: Inline Styles
```razor
<header style="background-color: var(--mud-palette-surface); border-bottom: 1px solid var(--mud-palette-divider)">
```

### Option 2: Dynamic Class Binding
```razor
<header class="@(_isDarkMode ? "dark-header" : "light-header")">
```

With CSS:
```css
.light-header { background-color: white; }
.dark-header { background-color: #1e1e1e; }
```

### Option 3: Use MudAppBar
Replace the custom header with MudBlazor's `MudAppBar`:
```razor
<MudAppBar>
    <ZoomControl />
    <MudSpacer />
    <MudIconButton Icon="..." OnClick="@ToggleDarkMode" />
</MudAppBar>
```

## Support

If dark mode still doesn't work after trying all solutions:

1. Check MudBlazor version (should be 6.0+)
2. Verify .NET version compatibility
3. Review browser console for CSS errors
4. Check if custom themes are conflicting
5. Ensure `MudThemeProvider` is rendered before components

## References

- [MudBlazor Dark Mode Documentation](https://mudblazor.com/features/dark-mode)
- [MUDBLAZOR_CSS_VARIABLES.md](MUDBLAZOR_CSS_VARIABLES.md)
- [DARK_MODE_README.md](DARK_MODE_README.md)
