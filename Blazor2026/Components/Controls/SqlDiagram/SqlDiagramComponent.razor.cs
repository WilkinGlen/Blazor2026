namespace Blazor2026.Components.Controls;

using Blazor2026.Models;
using Blazor2026.Components.Controls.SqlDiagram.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

public partial class SqlDiagramComponent : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<SqlDiagramComponent>? _dotNetHelper;
    private string sqlQuery = string.Empty;
    private string searchTerm = string.Empty;
    private SqlDiagramData? diagramData;
    private bool hasAttemptedParse = false;
    private bool needsLineUpdate = false;
    private static readonly Dictionary<string, bool> dictionary = new();
    private readonly Dictionary<string, bool> collapsedTables = dictionary;

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

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
        this.hasAttemptedParse = true;
        this.diagramData = SqlDiagramParser.ParseSql(this.sqlQuery);
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

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "f" or "F")
        {
            // TODO: Implement fit to viewport
        }

        // Delete functionality removed - no table selection
    }

    private IEnumerable<TableInfo> GetFilteredTables()
    {
        if (this.diagramData == null)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(this.searchTerm))
        {
            return this.diagramData.Tables;
        }

        var search = this.searchTerm.ToLowerInvariant();
        return this.diagramData.Tables.Where(t =>
            t.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            t.Alias.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            t.Columns.Any(c => c.Contains(search, StringComparison.OrdinalIgnoreCase)));
    }

    private void LoadSampleQuery()
    {
        this.sqlQuery = @"SELECT 
    o.OrderId, 
    o.OrderDate, 
    c.CustomerName, 
    p.ProductName, 
    od.Quantity, 
    od.Price
FROM Orders AS o
INNER JOIN Customers AS c ON o.CustomerId = c.CustomerId
INNER JOIN OrderDetails AS od ON o.OrderId = od.OrderId
LEFT JOIN Products AS p ON od.ProductId = p.ProductId
WHERE o.OrderDate >= '2024-01-01'";

        _ = this.GenerateDiagram();
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
