namespace Blazor2026.Components.Controls;

using Microsoft.AspNetCore.Components;

public enum TextBoxBorderStyle
{
    Default,
    Raised,
    Flat,
    Underlined,
    None
}

public sealed partial class TextBox : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;
    private string? _internalValue;
    private ElementReference inputElement;

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public TextBoxBorderStyle BorderStyle { get; set; } = TextBoxBorderStyle.Default;

    [Parameter]
    public int? Width { get; set; }

    [Parameter]
    public char? PasswordChar { get; set; }

    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    [Parameter]
    public int TextChangedDelayMilliseconds { get; set; } = 0;

    [Parameter]
    public bool Disabled { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        this._internalValue = this.Text;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only sync if text changed from outside
        if (this.Text != this._internalValue)
        {
            this._internalValue = this.Text;
        }
    }

    protected override bool ShouldRender()
    {
        // Only render on initial load or when parameters change from outside
        // Never render during user input
        return string.IsNullOrEmpty(this._internalValue) || this.Text == this._internalValue;
    }

    private string GetComputedStyle()
    {
        var width = this.Width.HasValue ? $"width: {this.Width}%;" : "width: 100%;";
        var passwordStyle = this.PasswordChar.HasValue ? $"-webkit-text-security: disc; font-family: text-security-disc;" : string.Empty;
        return $"{width} {passwordStyle} {this.Style}".Trim();
    }

    private async Task OnInputAsync(ChangeEventArgs e)
    {
        // Ignore input events when disabled
        if (this.Disabled)
        {
            return;
        }

        var newValue = e.Value?.ToString();
        this._internalValue = newValue;

        if (this.TextChangedDelayMilliseconds > 0)
        {
            // Cancel any pending delayed invocation
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource?.Dispose();
            this._cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(this.TextChangedDelayMilliseconds, this._cancellationTokenSource.Token);
                this.Text = newValue;
                await this.TextChanged.InvokeAsync(this.Text);
            }
            catch (TaskCanceledException)
            {
                // Delay was cancelled, do nothing
            }
        }
        else
        {
            this.Text = newValue;
            await this.TextChanged.InvokeAsync(this.Text);
        }
    }

    private string GetBorderClass()
    {
        return this.BorderStyle switch
        {
            TextBoxBorderStyle.Raised => "textbox-raised",
            TextBoxBorderStyle.Flat => "textbox-flat",
            TextBoxBorderStyle.Underlined => "textbox-underlined",
            TextBoxBorderStyle.None => "textbox-no-border",
            _ => "textbox-default"
        };
    }

    private string GetInputType()
    {
        return this.PasswordChar.HasValue ? "password" : "text";
    }

    public void Dispose()
    {
        this._cancellationTokenSource?.Cancel();
        this._cancellationTokenSource?.Dispose();
    }
}
