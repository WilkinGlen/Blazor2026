namespace Blazor2026.Components.Pages;

using Blazor2026.Models;
using Blazor2026.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public partial class SqlDiagram : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<SqlDiagram>? _dotNetHelper;
    private string sqlQuery = string.Empty;
    private SqlDiagramData? diagramData;
    private bool hasAttemptedParse = false;
    private bool needsLineUpdate = false;

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
