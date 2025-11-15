namespace Blazor2026.Components.Pages;

using Blazor2026.Models;
using Blazor2026.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

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
        if (needsLineUpdate && this._module != null)
        {
            needsLineUpdate = false;
            // Call updateAllLines after render is complete - increased delay for full render
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
        if (diagramData == null)
        {
            return;
        }

        var table = diagramData.Tables.FirstOrDefault(t => 
            t.Alias.Equals(tableId, StringComparison.OrdinalIgnoreCase));
        
        if (table != null)
        {
            table.X = x;
            table.Y = y;
            // Don't call StateHasChanged - it will re-render and reset the lines
            // JavaScript is already updating the lines in real-time
            // this.StateHasChanged();
        }
    }

    private async Task GenerateDiagram()
    {
        hasAttemptedParse = true;
        diagramData = SqlDiagramParser.ParseSql(sqlQuery);
        this.StateHasChanged();
        
        // Initialize drag after diagram is generated
        if (diagramData?.Tables.Any() == true)
        {
            await Task.Delay(100); // Wait for DOM to update and render
            
            // Dispose existing module if any
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
            
            // Initialize new module
            this._module = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/diagram.js");
            this._dotNetHelper = DotNetObjectReference.Create(this);
            await this._module.InvokeVoidAsync("initializeDraggable", this._dotNetHelper);
            
            // Trigger line update on next render
            needsLineUpdate = true;
            this.StateHasChanged();
        }
    }

    private void LoadSampleQuery()
    {
        sqlQuery = @"SELECT 
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
        
        GenerateDiagram();
    }

    private string GetJoinColor(string joinType)
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

    private Color GetJoinChipColor(string joinType)
    {
        return joinType.ToUpperInvariant() switch
        {
            "INNER" => Color.Primary,
            "LEFT" => Color.Success,
            "RIGHT" => Color.Warning,
            "FULL" => Color.Secondary,
            "CROSS" => Color.Error,
            _ => Color.Default
        };
    }

    private (double x, double y) GetEdgePoint(double boxX, double boxY, double boxWidth, double boxHeight, 
                                               double centerX, double centerY, double targetX, double targetY)
    {
        // Calculate direction from center to target
        var dx = targetX - centerX;
        var dy = targetY - centerY;
        
        // Normalize
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance == 0)
        {
            return (centerX, centerY);
        }
        
        dx /= distance;
        dy /= distance;
        
        // Calculate intersection with box edges
        var halfWidth = boxWidth / 2;
        var halfHeight = boxHeight / 2;
        
        // Check which edge the line intersects
        double t;
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Intersects left or right edge
            t = dx > 0 ? halfWidth : -halfWidth;
            var x = centerX + t;
            var y = centerY + (t / dx) * dy;
            return (x, y);
        }
        else
        {
            // Intersects top or bottom edge
            t = dy > 0 ? halfHeight : -halfHeight;
            var y = centerY + t;
            var x = centerX + (t / dy) * dx;
            return (x, y);
        }
    }

    private bool IsJoinColumn(string tableAlias, string columnName)
    {
        if (diagramData == null)
        {
            return false;
        }

        // Check if this column is used in any join condition for this specific table
        return diagramData.Joins.Any(join =>
            (join.FromTable.Equals(tableAlias, StringComparison.OrdinalIgnoreCase) &&
             join.FromColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase)) ||
            (join.ToTable.Equals(tableAlias, StringComparison.OrdinalIgnoreCase) &&
             join.ToColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase)));
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
