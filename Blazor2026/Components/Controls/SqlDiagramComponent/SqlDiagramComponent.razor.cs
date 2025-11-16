namespace Blazor2026.Components.Controls;

using Blazor2026.Models;
using Blazor2026.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using System.Security.Cryptography;
using System.Text;

public partial class SqlDiagramComponent : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<SqlDiagramComponent>? _dotNetHelper;
    private string sqlQuery = string.Empty;
    private string searchTerm = string.Empty;
    private SqlDiagramData? diagramData;
    private bool hasAttemptedParse = false;
    private bool needsLineUpdate = false;
    private TableInfo? selectedTable = null;
    private Dictionary<string, bool> collapsedTables = new();

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
        
        // Try to load saved layout
        if (this.diagramData?.Tables.Any() == true && this._module != null)
        {
            var queryHash = this.GetQueryHash(this.sqlQuery);
            try
            {
                var savedLayout = await this._module.InvokeAsync<Dictionary<string, TablePosition>?>("loadLayout", queryHash);
                if (savedLayout != null)
                {
                    this.ApplyLayout(savedLayout);
                }
            }
            catch
            {
                // Ignore if no saved layout
            }
        }
        
        this.StateHasChanged();
        
        // Initialize drag after diagram is generated
        if (this.diagramData?.Tables.Any() == true)
        {
            await Task.Delay(100);
            
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
            this.needsLineUpdate = true;
            this.StateHasChanged();
        }
    }

    private async Task SaveLayout()
    {
        if (this.diagramData == null || this._module == null)
        {
            return;
        }

        var queryHash = this.GetQueryHash(this.sqlQuery);
        var tables = this.diagramData.Tables.Select(t => new
        {
            alias = t.Alias,
            x = t.X,
            y = t.Y
        }).ToArray();

        await this._module.InvokeVoidAsync("saveLayout", queryHash, tables);
    }

    private async Task LoadLayout()
    {
        if (this.diagramData == null || this._module == null)
        {
            return;
        }

        var queryHash = this.GetQueryHash(this.sqlQuery);
        try
        {
            var savedLayout = await this._module.InvokeAsync<Dictionary<string, TablePosition>?>("loadLayout", queryHash);
            if (savedLayout != null)
            {
                this.ApplyLayout(savedLayout);
                this.needsLineUpdate = true;
                this.StateHasChanged();
            }
        }
        catch
        {
            // No saved layout
        }
    }

    private void ApplyLayout(Dictionary<string, TablePosition> layout)
    {
        if (this.diagramData == null)
        {
            return;
        }

        foreach (var table in this.diagramData.Tables)
        {
            if (layout.TryGetValue(table.Alias, out var position))
            {
                table.X = position.X;
                table.Y = position.Y;
            }
        }
    }

    private string GetQueryHash(string query)
    {
        var bytes = Encoding.UTF8.GetBytes(query.Trim());
        var hash = MD5.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task ExportAsSvg()
    {
        if (this._module == null)
        {
            return;
        }
        
        await this.JSRuntime.InvokeVoidAsync("eval", @"
            const svg = document.querySelector('.diagram-container svg');
            const serializer = new XMLSerializer();
            const svgString = serializer.serializeToString(svg);
            const blob = new Blob([svgString], { type: 'image/svg+xml' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'diagram.svg';
            a.click();
            URL.revokeObjectURL(url);
        ");
    }

    private void ToggleTableCollapse(string tableAlias)
    {
        if (this.collapsedTables.ContainsKey(tableAlias))
        {
            this.collapsedTables[tableAlias] = !this.collapsedTables[tableAlias];
        }
        else
        {
            this.collapsedTables[tableAlias] = true;
        }
    }

    private bool IsTableCollapsed(string tableAlias)
    {
        return this.collapsedTables.TryGetValue(tableAlias, out var collapsed) && collapsed;
    }

    private void SelectTable(TableInfo table)
    {
        this.selectedTable = table;
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "f" || e.Key == "F")
        {
            // TODO: Implement fit to viewport
        }
        
        if (e.CtrlKey && e.Key == "s")
        {
            await this.SaveLayout();
        }
        
        if (e.Key == "Delete" && this.selectedTable != null)
        {
            this.diagramData?.Tables.Remove(this.selectedTable);
            this.selectedTable = null;
            this.StateHasChanged();
        }
    }

    private IEnumerable<TableInfo> GetFilteredTables()
    {
        if (this.diagramData == null)
        {
            return Enumerable.Empty<TableInfo>();
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

    private static Color GetJoinChipColor(string joinType)
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
        if (this.diagramData == null)
        {
            return false;
        }

        return this.diagramData.Joins.Any(join =>
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

    private class TablePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
