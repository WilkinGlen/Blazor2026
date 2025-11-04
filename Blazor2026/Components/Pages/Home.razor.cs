namespace Blazor2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

public sealed partial class Home
{
    private static char PasswordChar => '●';
    private bool isDisabled = false;
    private string? lastChangedText;

    [Inject]
    private ILogger<Home> Logger { get; set; } = default!;

    private void HandleTextChanged(string? newText)
    {
        this.lastChangedText = newText;
        this.Logger.LogInformation("Text changed to: {Text}", newText);
    }

    private void ToggleDisabled()
    {
        this.isDisabled = !this.isDisabled;
    }
}
