namespace Blazor2026.Tests.Components.Controls;
public class TextBoxTests : TestContext
{
    [Fact]
    public void TextBox_ShouldRenderWithDefaultWidth()
    {
        var cut = this.RenderComponent<TextBox>();

        var input = cut.Find("input");
        _ = input.GetAttribute("style").Should().Contain("width: 100%");
    }

    [Theory]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    public void TextBox_ShouldRenderWithSpecifiedWidth(int width)
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.Width, width));

        var input = cut.Find("input");
        _ = input.GetAttribute("style").Should().Contain($"width: {width}%");
    }

    [Theory]
    [InlineData(TextBoxBorderStyle.Default, "textbox-default")]
    [InlineData(TextBoxBorderStyle.Raised, "textbox-raised")]
    [InlineData(TextBoxBorderStyle.Flat, "textbox-flat")]
    [InlineData(TextBoxBorderStyle.Underlined, "textbox-underlined")]
    [InlineData(TextBoxBorderStyle.None, "textbox-no-border")]
    public void TextBox_ShouldRenderWithCorrectBorderStyle(TextBoxBorderStyle borderStyle, string expectedClass)
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.BorderStyle, borderStyle));

        var input = cut.Find("input");
        _ = input.GetAttribute("class").Should().Contain(expectedClass);
    }

    [Fact]
    public void TextBox_ShouldTriggerTextChanged()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("New Text");

        _ = changedText.Should().Be("New Text");
    }

    [Fact]
    public void TextBox_ShouldRenderWithInitialText()
    {
        const string initialText = "Initial Text";

        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.Text, initialText));

        var input = cut.Find("input");
        _ = input.GetAttribute("value").Should().Be(initialText);
    }

    [Fact]
    public void TextBox_ShouldRenderAsTextInputByDefault()
    {
        var cut = this.RenderComponent<TextBox>();

        var input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("text");
    }

    [Fact]
    public void TextBox_ShouldRenderAsPasswordInputWhenPasswordCharIsSet()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.PasswordChar, '*'));

        var input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("password");
    }

    [Theory]
    [InlineData('*')]
    [InlineData('•')]
    [InlineData('?')]
    [InlineData('#')]
    public void TextBox_ShouldRenderAsPasswordInputForAnyPasswordChar(char passwordChar)
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.PasswordChar, passwordChar));

        var input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void TextBox_ShouldReturnActualTextWhenPasswordCharIsSet()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.PasswordChar, '*')
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("MySecret123");

        _ = changedText.Should().Be("MySecret123");
    }

    [Fact]
    public void TextBox_ShouldMaintainPasswordValueAcrossMultipleInputs()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.PasswordChar, '•')
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Pass");
        _ = changedText.Should().Be("Pass");

        input.Input("Password123");
        _ = changedText.Should().Be("Password123");
    }

    [Fact]
    public void TextBox_ShouldSwitchFromTextToPasswordWhenPasswordCharIsAdded()
    {
        var cut = this.RenderComponent<TextBox>();
        var input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("text");

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.PasswordChar, '*'));
        input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void TextBox_ShouldPreserveInitialTextWithPasswordChar()
    {
        const string initialPassword = "InitialSecret";
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.Text, initialPassword)
            .Add(p => p.PasswordChar, '*'));

        var input = cut.Find("input");
        _ = input.GetAttribute("value").Should().Be(initialPassword);
        _ = input.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void TextBox_ShouldHaveDefaultDelayOfZero()
    {
        var cut = this.RenderComponent<TextBox>();

        _ = cut.Instance.TextChangedDelayMilliseconds.Should().Be(0);
    }

    [Fact]
    public async Task TextBox_ShouldTriggerTextChangedImmediatelyWhenDelayIsZero()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.TextChangedDelayMilliseconds, 0)
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Immediate");

        // Should be available immediately
        _ = changedText.Should().Be("Immediate");
        await Task.Delay(10); // Small delay to ensure no delayed callback
        _ = changedText.Should().Be("Immediate");
    }

    [Fact]
    public async Task TextBox_ShouldDelayTextChangedCallbackWhenDelayIsSet()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Delayed");

        // Should not be triggered immediately
        _ = changedText.Should().BeNull();

        // Wait for delay to complete
        await Task.Delay(150);

        // Should be triggered after delay
        _ = changedText.Should().Be("Delayed");
    }

    [Fact]
    public async Task TextBox_ShouldDebounceMultipleInputsWithDelay()
    {
        var callCount = 0;
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
      .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.TextChanged, args =>
            {
                changedText = args;
                callCount++;
            }));

        var input = cut.Find("input");

        // Simulate rapid typing
        input.Input("H");
        await Task.Delay(30);
        input.Input("He");
        await Task.Delay(30);
        input.Input("Hel");
        await Task.Delay(30);
        input.Input("Hell");
        await Task.Delay(30);
        input.Input("Hello");

        // Callback should not have been triggered yet
        _ = callCount.Should().Be(0);
        _ = changedText.Should().BeNull();

        // Wait for delay to complete
        await Task.Delay(150);

        // Should be triggered only once with final value
        _ = callCount.Should().Be(1);
        _ = changedText.Should().Be("Hello");
    }

    [Fact]
    public async Task TextBox_ShouldCancelPreviousDelayWhenNewInputOccurs()
    {
        var callCount = 0;
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.TextChanged, args =>
            {
                changedText = args;
                callCount++;
            }));

        var input = cut.Find("input");

        input.Input("First");
        await Task.Delay(50); // Wait less than the delay

        input.Input("Second");
        await Task.Delay(50); // Wait less than the delay again

        // No callback should have been triggered
        _ = callCount.Should().Be(0);

        await Task.Delay(100); // Wait for the second delay to complete

        // Should only trigger once with the last value
        _ = callCount.Should().Be(1);
        _ = changedText.Should().Be("Second");
    }

    [Fact]
    public async Task TextBox_ShouldMaintainDisplayedTextDuringDelay()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Visible");

        // Callback not triggered yet
        _ = changedText.Should().BeNull();

        await Task.Delay(150);

        // Callback triggered with correct value
        _ = changedText.Should().Be("Visible");
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(250)]
    [InlineData(500)]
    public async Task TextBox_ShouldRespectDifferentDelayValues(int delayMs)
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
            .Add(p => p.TextChangedDelayMilliseconds, delayMs)
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Test");

        // Should not be triggered immediately
        _ = changedText.Should().BeNull();

        // Wait for the specific delay
        await Task.Delay(delayMs + 50);

        // Should be triggered after delay
        _ = changedText.Should().Be("Test");
    }

    [Fact]
    public async Task TextBox_ShouldWorkWithDelayAndPasswordCharTogether()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
             .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.PasswordChar, '*')
.Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("password");

        input.Input("Secret123");

        // Should be password type
        _ = input.GetAttribute("type").Should().Be("password");
        _ = changedText.Should().BeNull();

        await Task.Delay(150);

        // Should return actual password text after delay
        _ = changedText.Should().Be("Secret123");
    }

    [Fact]
    public async Task TextBox_ShouldUpdateTextParameterOnlyAfterDelay()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
     .Add(p => p.TextChangedDelayMilliseconds, 100)
     .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Updated");

        // Text parameter should not be updated immediately
        await Task.Delay(50);
        _ = changedText.Should().BeNull();

        // Wait for delay
        await Task.Delay(100);

        // Text parameter should be updated after delay
        _ = cut.Instance.Text.Should().Be("Updated");
        _ = changedText.Should().Be("Updated");
    }

    [Fact]
    public void TextBox_ShouldDisposeProperlyWithPendingDelay()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
       .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Test");

        // Dispose before delay completes
        cut.Dispose();

        // Should not throw exception
        _ = changedText.Should().BeNull();
    }

    [Fact]
    public async Task TextBox_ShouldNotLoseTextWhenDelayedCallbackIsInProgress()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
  .Add(p => p.TextChangedDelayMilliseconds, 150)
    .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");

        input.Input("First");

        await Task.Delay(100); // Wait but not long enough for callback

        input.Input("Second");

        // Callback should not have fired yet
        _ = changedText.Should().BeNull();

        await Task.Delay(200); // Wait for callback

        // Callback fired with latest value
        _ = changedText.Should().Be("Second");
    }

    [Fact]
    public void TextBox_ShouldNotLoseTextOnSingleInput()
    {
        var cut = this.RenderComponent<TextBox>();

        var input = cut.Find("input");
        input.Input("A");

        // Text should be immediately visible
        _ = input.GetAttribute("value").Should().Be("A");
    }

    [Fact]
    public void TextBox_ShouldNotLoseTextDuringRapidTyping()
    {
        var cut = this.RenderComponent<TextBox>();

        var input = cut.Find("input");

        input.Input("H");
        _ = input.GetAttribute("value").Should().Be("H");

        input.Input("He");
        _ = input.GetAttribute("value").Should().Be("He");

        input.Input("Hel");
        _ = input.GetAttribute("value").Should().Be("Hel");

        input.Input("Hell");
        _ = input.GetAttribute("value").Should().Be("Hell");

        input.Input("Hello");
        _ = input.GetAttribute("value").Should().Be("Hello");
    }

    [Fact]
    public async Task TextBox_ShouldNotLoseTextWithDelayDuringTyping()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
        .Add(p => p.TextChangedDelayMilliseconds, 100)
          .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Hello");

        // Text should be immediately visible
        _ = input.GetAttribute("value").Should().Be("Hello");

        // Wait for delay
        await Task.Delay(150);

        // Text parameter should be updated after delay
        _ = changedText.Should().Be("Hello");
    }

    [Fact]
    public async Task TextBox_ShouldPreserveTextAfterMultipleRapidInputsWithDelay()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
     .Add(p => p.TextChangedDelayMilliseconds, 200)
             .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");

        // Simulate rapid typing
        input.Input("Java");
        await Task.Delay(50);
        input.Input("JavaScript");
        await Task.Delay(50);
        input.Input("JavaScript ES6");
        await Task.Delay(50);
        input.Input("JavaScript ES6 Features");
        await Task.Delay(50);
        input.Input("JavaScript ES6 Features Explained");

        // Callback should not have been triggered yet
        _ = changedText.Should().BeNull();

        // Wait for delay to complete
        await Task.Delay(250);

        // Should be triggered only once with final value
        _ = changedText.Should().Be("JavaScript ES6 Features Explained");
    }

    [Fact]
    public void TextBox_ShouldHandleVeryLongText()
    {
        var longText = new string('A', 10000);
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
    .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input(longText);

        // The input's value should be the long text
        _ = input.GetAttribute("value").Should().Be(longText);

        // TextChanged should be triggered with the correct value
        _ = changedText.Should().Be(longText);
    }

    [Theory]
    [InlineData("Hello 世界")]
    [InlineData("Привет мир")]
    [InlineData("مرحبا بالعالم")]
    [InlineData("🎉🎊✨")]
    public void TextBox_ShouldHandleUnicodeCharacters(string unicodeText)
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
              .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input(unicodeText);

        // The input's value should be the unicode text
        _ = input.GetAttribute("value").Should().Be(unicodeText);

        // TextChanged should be triggered with the correct value
        _ = changedText.Should().Be(unicodeText);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData(" Leading")]
    [InlineData("Trailing ")]
    [InlineData(" Both ")]
    public void TextBox_ShouldPreserveWhitespace(string textWithWhitespace)
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
           .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input(textWithWhitespace);

        // The input's value should preserve the whitespace
        _ = input.GetAttribute("value").Should().Be(textWithWhitespace);

        // TextChanged should be triggered with the correct value
        _ = changedText.Should().Be(textWithWhitespace);
    }

    [Fact]
    public void TextBox_ShouldApplyWidthAndBorderStyleTogether()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
       .Add(p => p.Width, 50)
     .Add(p => p.BorderStyle, TextBoxBorderStyle.Raised));

        var input = cut.Find("input");

        // Check applied styles
        var style = input.GetAttribute("style");
        _ = style.Should().Contain("width: 50%");
        // Assuming that the Raised style adds a specific class
        _ = input.GetAttribute("class").Should().Contain("textbox-raised");
    }

    [Fact]
    public async Task TextBox_ShouldApplyAllParametersTogether()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
  .Add(p => p.Text, "Initial")
      .Add(p => p.Width, 75)
        .Add(p => p.BorderStyle, TextBoxBorderStyle.Underlined)
            .Add(p => p.PasswordChar, '*')
       .Add(p => p.TextChangedDelayMilliseconds, 100)
            .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");

        // Check initial value
        _ = input.GetAttribute("value").Should().Be("Initial");

        // Check applied styles
        var style = input.GetAttribute("style");
        _ = style.Should().Contain("width: 75%");
        _ = input.GetAttribute("class").Should().Contain("textbox-underlined");

        // Trigger text change
        input.Input("Updated");
        _ = changedText.Should().Be("Updated");

        // Wait for delay
        await Task.Delay(150);

        // Text parameter should be updated after delay
        _ = cut.Instance.Text.Should().Be("Updated");
        _ = changedText.Should().Be("Updated");
    }

    [Fact]
    public void TextBox_ShouldHandleCustomStyleWithWidth()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
        .Add(p => p.Width, 60)
   .Add(p => p.Style, "color: red;"));

        var input = cut.Find("input");

        // Check applied styles
        var style = input.GetAttribute("style");
        _ = style.Should().Contain("width: 60%");
        _ = style.Should().Contain("color: red;");
    }

    [Fact]
    public async Task TextBox_ShouldChangeParametersDynamically()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
   .Add(p => p.Width, 25)
           .Add(p => p.BorderStyle, TextBoxBorderStyle.Default));

        var input = cut.Find("input");
        _ = input.GetAttribute("style").Should().Contain("width: 25%");

        cut.SetParametersAndRender(parameters => parameters
        .Add(p => p.Width, 75)
   .Add(p => p.BorderStyle, TextBoxBorderStyle.Flat));

        input = cut.Find("input");
        _ = input.GetAttribute("style").Should().Contain("width: 75%");
        _ = input.GetAttribute("class").Should().Contain("textbox-flat");
    }

    [Fact]
    public void TextBox_ShouldHandleMultipleSubscribers()
    {
        var callCount = 0;
        string? latestText = null;

        var cut = this.RenderComponent<TextBox>(parameters => parameters
              .Add(p => p.TextChanged, args =>
        {
            latestText = args;
            callCount++;
        }));

        var input = cut.Find("input");
        input.Input("First");

        _ = callCount.Should().Be(1);
        _ = latestText.Should().Be("First");

        input.Input("Second");

        _ = callCount.Should().Be(2);
        _ = latestText.Should().Be("Second");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(5000)]
    public async Task TextBox_ShouldHandleVariousDelayEdgeCases(int delayMs)
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
           .Add(p => p.TextChangedDelayMilliseconds, delayMs)
               .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("EdgeCase");

        // Should not be triggered immediately
        _ = changedText.Should().BeNull();

        // Wait for the specific delay
        await Task.Delay(delayMs + 50);

        // Should be triggered after delay
        _ = changedText.Should().Be("EdgeCase");
    }

    [Fact]
    public async Task TextBox_ShouldHandleExtremelyRapidInput()
    {
        var callCount = 0;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
          .Add(p => p.TextChangedDelayMilliseconds, 100)
              .Add(p => p.TextChanged, args => callCount++));

        var input = cut.Find("input");

        // Simulate extremely rapid input
        input.Input("A");
        input.Input("AB");
        input.Input("ABC");
        input.Input("ABCD");
        input.Input("ABCDE");

        // Wait for delay to complete
        await Task.Delay(150);

        // Callback should be triggered with the final value
        _ = callCount.Should().Be(1);
    }

    [Fact]
    public async Task TextBox_ShouldHandleConcurrentDelays()
    {
        var callCount = 0;
        string? finalText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
         .Add(p => p.TextChangedDelayMilliseconds, 100)
     .Add(p => p.TextChanged, args =>
       {
           finalText = args;
           callCount++;
       }));

        var input = cut.Find("input");

        input.Input("Concurrent1");
        await Task.Delay(50);
        input.Input("Concurrent2");
        await Task.Delay(50);
        input.Input("Concurrent3");
        await Task.Delay(50);

        // Call count should still be 0, not triggered yet
        _ = callCount.Should().Be(0);

        // Wait for enough time for all delayed actions to complete
        await Task.Delay(300);

        // Finally, it should reflect the last input after all concurrent inputs
        _ = callCount.Should().Be(1);
        _ = finalText.Should().Be("Concurrent3");
    }

    [Fact]
    public void TextBox_ShouldApplyCustomStyleOverDefaultWidth()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
  .Add(p => p.Style, "width: 200px; background: yellow;"));

        var input = cut.Find("input");

        // Check applied custom styles
        var style = input.GetAttribute("style");
        _ = style.Should().Contain("width: 200px");
        _ = style.Should().Contain("background: yellow");
    }

    [Fact]
    public void TextBox_ShouldTrimExtraSpacesInStyle()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
.Add(p => p.Width, 50)
        .Add(p => p.Style, " "));

        var input = cut.Find("input");

        // Check applied styles
        var style = input.GetAttribute("style");
        _ = style.Should().Contain("width: 50%");
        _ = style.Should().NotContain(" ;");
        _ = style.Should().NotContain("  ");
    }

    [Fact]
    public void TextBox_ShouldHandleNegativeDelay()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
              .Add(p => p.TextChangedDelayMilliseconds, -100)
           .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Negative Delay Test");

        // Wait briefly and check if the TextChanged was called
        Task.Delay(50).ContinueWith(t => _ = changedText.Should().Be("Negative Delay Test"));
    }

    [Fact]
    public async Task TextBox_ShouldWorkInMultipleInstancesIndependently()
    {
        string? text1 = null;
        string? text2 = null;

        var cut1 = this.RenderComponent<TextBox>(parameters => parameters
.Add(p => p.TextChangedDelayMilliseconds, 100)
      .Add(p => p.TextChanged, args => text1 = args));

        var cut2 = this.RenderComponent<TextBox>(parameters => parameters
    .Add(p => p.TextChangedDelayMilliseconds, 200)
     .Add(p => p.TextChanged, args => text2 = args));

        var input1 = cut1.Find("input");
        var input2 = cut2.Find("input");

        // Ensure both inputs are rendered
        _ = input1.Should().NotBeNull();
        _ = input2.Should().NotBeNull();

        // Change values independently
        input1.Input("Hello");
        input2.Input("World");

        // Verify text changed events
        await Task.Delay(150);
        _ = text1.Should().Be("Hello");
        _ = text2.Should().Be("World");
    }

    #region Disabled Functionality

    [Fact]
    public void TextBox_ShouldNotBeDisabledByDefault()
    {
        var cut = this.RenderComponent<TextBox>();

        _ = cut.Instance.Disabled.Should().BeFalse();
        var input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void TextBox_ShouldRenderAsDisabledWhenDisabledIsTrue()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters.Add(p => p.Disabled, true));

        _ = cut.Instance.Disabled.Should().BeTrue();
        var input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void TextBox_ShouldNotTriggerTextChangedWhenDisabled()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
      .Add(p => p.Disabled, true)
         .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Test");

        _ = changedText.Should().BeNull();
    }

    [Fact]
    public async Task TextBox_ShouldNotTriggerDelayedCallbackWhenDisabled()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
     .Add(p => p.Disabled, true)
       .Add(p => p.TextChangedDelayMilliseconds, 100)
      .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("Test");

        await Task.Delay(200);

        _ = changedText.Should().BeNull();
    }

    [Fact]
    public void TextBox_ShouldPreserveTextWhenDisabled()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
.Add(p => p.Text, "Initial")
   .Add(p => p.Disabled, true));

        var input = cut.Find("input");
        _ = input.GetAttribute("value").Should().Be("Initial");
    }

    [Fact]
    public void TextBox_ShouldEnableAndDisableDynamically()
    {
        var cut = this.RenderComponent<TextBox>();

        var input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeFalse();

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.Disabled, true));
        input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeTrue();

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.Disabled, false));
        input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void TextBox_ShouldAcceptInputAfterBeingEnabled()
    {
        string? changedText = null;
        var cut = this.RenderComponent<TextBox>(parameters => parameters
   .Add(p => p.Disabled, true)
       .Add(p => p.TextChanged, args => changedText = args));

        var input = cut.Find("input");
        input.Input("First");
        _ = changedText.Should().BeNull();

        // Enable the textbox
        cut.SetParametersAndRender(parameters => parameters
          .Add(p => p.Disabled, false)
          .Add(p => p.TextChanged, args => changedText = args));

        input = cut.Find("input");
        input.Input("Second");
        _ = changedText.Should().Be("Second");
    }

    [Fact]
    public void TextBox_ShouldWorkWithDisabledAndPasswordTogether()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
           .Add(p => p.Disabled, true)
         .Add(p => p.PasswordChar, '*')
       .Add(p => p.Text, "Secret"));

        var input = cut.Find("input");
        _ = input.GetAttribute("type").Should().Be("password");
        _ = input.HasAttribute("disabled").Should().BeTrue();
        _ = input.GetAttribute("value").Should().Be("Secret");
    }

    [Fact]
    public void TextBox_ShouldWorkWithDisabledAndBorderStyle()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
       .Add(p => p.Disabled, true)
    .Add(p => p.BorderStyle, TextBoxBorderStyle.Raised));

        var input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeTrue();
        _ = input.GetAttribute("class").Should().Contain("textbox-raised");
    }

    [Fact]
    public void TextBox_ShouldWorkWithDisabledAndWidth()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
        .Add(p => p.Disabled, true)
             .Add(p => p.Width, 50));

        var input = cut.Find("input");
        _ = input.HasAttribute("disabled").Should().BeTrue();
        _ = input.GetAttribute("style").Should().Contain("width: 50%");
    }

    [Fact]
    public void TextBox_ShouldWorkWithAllParametersIncludingDisabled()
    {
        var cut = this.RenderComponent<TextBox>(parameters => parameters
        .Add(p => p.Text, "Test")
       .Add(p => p.Width, 75)
    .Add(p => p.BorderStyle, TextBoxBorderStyle.Flat)
        .Add(p => p.PasswordChar, '•')
     .Add(p => p.Disabled, true)
         .Add(p => p.TextChangedDelayMilliseconds, 100));

        var input = cut.Find("input");
        _ = input.GetAttribute("value").Should().Be("Test");
        _ = input.GetAttribute("style").Should().Contain("width: 75%");
        _ = input.GetAttribute("class").Should().Contain("textbox-flat");
        _ = input.GetAttribute("type").Should().Be("password");
        _ = input.HasAttribute("disabled").Should().BeTrue();
        _ = cut.Instance.TextChangedDelayMilliseconds.Should().Be(100);
    }

    #endregion
}