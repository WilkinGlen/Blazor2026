# DARK MODE DEBUG STEPS

## Quick Diagnostic Steps

### Step 1: Run the Diagnostic Page

1. Start your application
2. Navigate to `/dark-mode-test`
3. Check if CSS variables are showing values or "Variable not found!"
4. Toggle dark mode (click sun/moon icon)
5. Click "Refresh Computed Values" button
6. Check if the values change

**Expected Results:**
- Light mode: Surface should be white/light color
- Dark mode: Surface should be dark color  (#1e1e1e or similar)

### Step 2: Browser DevTools Inspection

1. **Open DevTools** (F12)
2. **Inspect the header element**: Right-click on the header → Inspect
3. **Check Computed tab**: Look for the element's computed background-color
4. **Check Styles tab**: Look for:
   ```css
   .zoom-control-header {
       background-color: var(--mud-palette-surface);
   }
   ```
5. **Click on the variable**: DevTools should show the resolved color value
6. **Toggle dark mode** and watch if the value changes

### Step 3: Check MudThemeProvider

1. In DevTools, find the `<html>` or `<body>` element
2. Look for data attributes like:
   - `data-theme="dark"` or `data-theme="light"`
   - `data-mud-theme="dark"` or similar
3. If you don't see these, MudThemeProvider might not be working correctly

### Step 4: Manual CSS Test

Add this temporarily to `app.css` to test if CSS is loading at all:

```css
.zoom-control-header {
  background-color: red !important;
}
```

- If header turns red → CSS is loading, but variables aren't working
- If header stays white → CSS file isn't being loaded or applied

### Step 5: Check Browser Console

1. Open browser console (F12 → Console tab)
2. Look for errors related to:
 - MudBlazor
   - CSS loading
   - JavaScript errors
3. Type this in console to check variables manually:
   ```javascript
getComputedStyle(document.documentElement).getPropertyValue('--mud-palette-surface')
   ```

## Common Fixes

### Fix 1: Clear Everything

```bash
# Stop the app
# In terminal:
dotnet clean
# Delete bin and obj folders manually
# Then:
dotnet build
# Hard refresh browser (Ctrl+Shift+R or Cmd+Shift+R)
```

### Fix 2: Check App.razor

Ensure MudBlazor CSS is loaded in `App.razor`:

```html
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```

### Fix 3: Use Inline Styles (Temporary Workaround)

In `MainLayout.razor`, change the header to:

```razor
<header class="zoom-control-header" 
        style="background-color: @(_isDarkMode ? "#1e1e1e" : "#ffffff"); 
            border-bottom: 1px solid @(_isDarkMode ? "#4a4a4a" : "#e0e0e0")">
```

This bypasses CSS variables entirely.

### Fix 4: Check MudBlazor Version

Run in terminal:
```bash
dotnet list package | findstr MudBlazor
```

Should show version 6.0.0 or higher for dark mode support.

## What to Report

If still not working, report these details:

1. **Diagnostic page results**: What does /dark-mode-test show?
2. **Browser**: Which browser and version?
3. **CSS Variable values**: From DevTools or diagnostic page
4. **Console errors**: Any errors in browser console?
5. **MudBlazor version**: From `dotnet list package`
6. **Does inline style work**: Did Fix 3 work?

## Nuclear Option: Replace with MudAppBar

If nothing works, replace the custom header with MudBlazor's AppBar:

```razor
<MudAppBar Elevation="1">
    <ZoomControl />
    <MudSpacer />
    <MudIconButton Icon="@(_isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode)" 
     Color="Color.Inherit"
        OnClick="@ToggleDarkMode" />
</MudAppBar>
```

MudAppBar automatically handles dark mode.
