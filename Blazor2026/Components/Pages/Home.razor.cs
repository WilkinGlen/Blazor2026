namespace Blazor2026.Components.Pages;

using Blazor2026.Services;

public partial class Home
{
    private string? originalSql;

    private string? OriginalSql
    {
        get => this.originalSql;
        set
        {
            this.originalSql = value;
            this.FormatSql();
        }
    }

    private string? formattedSql;

    private void FormatSql()
    {
        this.formattedSql = SqlFormatterService.FormatSql(this.OriginalSql);
    }
}
