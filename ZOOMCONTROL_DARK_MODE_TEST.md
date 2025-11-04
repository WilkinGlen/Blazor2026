# Dark Mode Testing Guide for ZoomControl

## Quick Test Steps

1. **Run the application**
2. **Look at the ZoomControl** (should have light grey background `#f5f5f5`)
3. **Click the sun/moon icon** in the top-right
4. **ZoomControl should change** to dark grey (`#2a2a2a`)

## If Dark Mode Still Not Working

### Check 1: Is ThemeService Registered?

Open browser DevTools Console (F12) and check for errors related to:
- "Cannot read property 'IsDarkMode'"
- "ThemeService" injection errors

### Check 2: Verify the Background Color

1. **Right-click on the ZoomControl** → Inspect
2. **Check the inline style** in DevTools:
   - Should show: `style="background-color: #f5f5f5; ..."`
   - After toggling: `style="background-color: #2a2a2a; ..."`
3. If the color value doesn't change, the component isn't re-rendering

### Check 3: Enable Debug Output

Uncomment this line in `ZoomControl.razor`:

```razor
<div style="font-size: 10px; color: red;">Dark Mode: @ThemeService.IsDarkMode</div>
```

Add it right after the closing `</div>` of zoom-control.

- Should show "Dark Mode: False" in light mode
- Should show "Dark Mode: True" in dark mode
- If it doesn't change when you click the toggle, ThemeService isn't being updated

### Check 4: Manual Test with Breakpoint

Add this to `ZoomControl.razor.cs`:

```csharp
private string GetBackgroundColor()
{
    var isDark = this.ThemeService.IsDarkMode; // <- Set breakpoint here
    return isDark ? "#2a2a2a" : "#f5f5f5";
}
```

Set a breakpoint and click the dark mode toggle. The breakpoint should hit and `isDark` should change.

### Check 5: Verify Event Subscription

The `OnThemeChanged` event should be subscribed. Verify this in `ZoomControl.razor.cs`:

```csharp
protected override void OnInitialized()
{
    this.ZoomService.OnZoomChanged += this.StateHasChanged;
    this.ThemeService.OnThemeChanged += this.StateHasChanged; // <- This line
}
```

## Expected Behavior

✅ **Light Mode**: ZoomControl has light grey background (`#f5f5f5`)  
✅ **Dark Mode**: ZoomControl has dark grey background (`#2a2a2a`)  
✅ **Smooth Transition**: 0.3s fade when switching modes  
✅ **Header Also Changes**: Header background should also change  

## Colors Reference

| Mode | ZoomControl BG | Header BG | Header Border |
|------|---------------|-----------|---------------|
| Light | `#f5f5f5` | `#ffffff` | `#e0e0e0` |
| Dark | `#2a2a2a` | `#1e1e1e` | `#4a4a4a` |

## Common Issues

### Issue: Background Doesn't Change

**Cause**: Component not re-rendering when theme changes

**Fix**: Ensure `ThemeService.OnThemeChanged` is subscribed in `OnInitialized()`

### Issue: Error "ThemeService not found"

**Cause**: Service not registered in DI

**Fix**: Check `Program.cs` has:
```csharp
builder.Services.AddSingleton<ThemeService>();
```

### Issue: Jumpy/No Transition

**Cause**: CSS transition might be overridden

**Fix**: Check that inline style has `transition: background-color 0.3s ease;`

## Still Not Working?

If none of the above helps:

1. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

2. Hard refresh browser (Ctrl+Shift+R)

3. Check browser console for JavaScript errors

4. Try the diagnostic page: Navigate to `/dark-mode-test`

5. Report back with:
   - What color the ZoomControl shows
   - What the debug output shows (if enabled)
   - Any console errors
