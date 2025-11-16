namespace Blazor2026.Components.Controls;

using Blazor2026.Components.Controls.SqlDiagram.Services;
using Blazor2026.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public partial class SqlDiagramComponent : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<SqlDiagramComponent>? _dotNetHelper;
    private SqlDiagramData? diagramData;
    private bool needsLineUpdate = false;
    private static readonly Dictionary<string, bool> dictionary = [];
    private readonly Dictionary<string, bool> collapsedTables = dictionary;
    private string? _previousSqlQuery;

    [Parameter]
    public string? SqlQuery { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (this.SqlQuery != this._previousSqlQuery && !string.IsNullOrWhiteSpace(this.SqlQuery))
        {
            this._previousSqlQuery = this.SqlQuery;
            await this.GenerateDiagram();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (this.needsLineUpdate && this._module != null)
        {
            this.needsLineUpdate = false;
            await Task.Delay(200);
            try
            {
                await this._module.InvokeVoidAsync("updateAllLines");
            }
            catch (JSDisconnectedException)
            {
                // Ignore if circuit disconnected
            }
        }
    }

    [JSInvokable]
    public void UpdateTablePosition(string tableId, double x, double y)
    {
        if (this.diagramData == null)
        {
            return;
        }

        var table = this.diagramData.Tables.FirstOrDefault(t =>
            t.Alias.Equals(tableId, StringComparison.OrdinalIgnoreCase));

        if (table != null)
        {
            table.X = x;
            table.Y = y;
        }
    }

    private async Task GenerateDiagram()
    {
        if (string.IsNullOrWhiteSpace(this.SqlQuery))
        {
            this.diagramData = null;
            return;
        }

        this.diagramData = SqlDiagramParser.ParseSql(this.SqlQuery);
        this.StateHasChanged();

        if (this.diagramData?.Tables.Count != 0 == true)
        {
            await Task.Delay(100);

            if (this._module != null)
            {
                try
                {
                    await this._module.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // Ignore if circuit is disconnected
                }
            }

            this._module = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/diagram.js");
            this._dotNetHelper = DotNetObjectReference.Create(this);
            await this._module.InvokeVoidAsync("initializeDraggable", this._dotNetHelper);

            this.needsLineUpdate = true;
            this.StateHasChanged();
        }
    }

    private void ToggleTableCollapse(string tableAlias)
    {
        this.collapsedTables[tableAlias] = !this.collapsedTables.TryGetValue(tableAlias, out var value) || (this.collapsedTables[tableAlias] = !value);
    }

    private bool IsTableCollapsed(string tableAlias)
    {
        return this.collapsedTables.TryGetValue(tableAlias, out var collapsed) && collapsed;
    }

    private static string GetJoinColor(string joinType)
    {
        return joinType.ToUpperInvariant() switch
        {
            "INNER" => "#1976d2",
            "LEFT" => "#388e3c",
            "RIGHT" => "#f57c00",
            "FULL" => "#7b1fa2",
            "CROSS" => "#c62828",
            _ => "#666666"
        };
    }

    private bool IsJoinColumn(string tableAlias, string columnName)
    {
        if (this.diagramData == null)
        {
            return false;
        }

        return this.diagramData.Joins.Any(join =>
            join.FromTable.Equals(tableAlias, StringComparison.OrdinalIgnoreCase) &&
            join.FromColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase) ||
            join.ToTable.Equals(tableAlias, StringComparison.OrdinalIgnoreCase) &&
            join.ToColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask DisposeAsync()
    {
        if (this._module != null)
        {
            try
            {
                await this._module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already disconnected
            }
        }

        this._dotNetHelper?.Dispose();
        GC.SuppressFinalize(this);
    }
}
