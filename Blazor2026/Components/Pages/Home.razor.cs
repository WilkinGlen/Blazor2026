namespace Blazor2026.Components.Pages;

using Blazor2026.Services;

public partial class Home
{
    private string? OriginalSql
    {
        get;
        set
        {
            field = value;
            this.FormatSql();
        }
    }

    private string? formattedSql;

    private void FormatSql()
    {
        this.formattedSql = SqlFormatterService.FormatSql(this.OriginalSql);
    }
}
